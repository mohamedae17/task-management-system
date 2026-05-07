import { Injectable, OnDestroy, computed, inject, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenStorageService } from '../auth/token-storage.service';
import { NotificationDto } from '../models/dtos';

export interface TaskChangedEvent {
  taskId: string;
  changeType: string;
  payload: unknown;
}

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private readonly tokens = inject(TokenStorageService);

  private connection?: HubConnection;
  private readonly _state = signal<HubConnectionState>(HubConnectionState.Disconnected);
  readonly state = this._state.asReadonly();
  readonly isConnected = computed(() => this._state() === HubConnectionState.Connected);

  readonly notification$ = new Subject<NotificationDto>();
  readonly taskChanged$ = new Subject<TaskChangedEvent>();

  async connect(): Promise<void> {
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) return;

    this.connection = new HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        accessTokenFactory: () => this.tokens.getAccessToken() ?? ''
      })
      .withAutomaticReconnect([0, 2_000, 5_000, 10_000, 30_000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('notification', (n: NotificationDto) => this.notification$.next(n));
    this.connection.on('task-changed', (e: TaskChangedEvent) => this.taskChanged$.next(e));

    this.connection.onreconnecting(() => this._state.set(HubConnectionState.Reconnecting));
    this.connection.onreconnected(() => this._state.set(HubConnectionState.Connected));
    this.connection.onclose(() => this._state.set(HubConnectionState.Disconnected));

    try {
      await this.connection.start();
      this._state.set(HubConnectionState.Connected);
    } catch (err) {
      this._state.set(HubConnectionState.Disconnected);
      console.warn('SignalR connection failed', err);
    }
  }

  async disconnect(): Promise<void> {
    if (!this.connection) return;
    try { await this.connection.stop(); } finally { this._state.set(HubConnectionState.Disconnected); }
  }

  subscribeToTask(taskId: string): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) return Promise.resolve();
    return this.connection.invoke('SubscribeToTask', taskId);
  }

  unsubscribeFromTask(taskId: string): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) return Promise.resolve();
    return this.connection.invoke('UnsubscribeFromTask', taskId);
  }

  ngOnDestroy(): void {
    void this.disconnect();
  }
}
