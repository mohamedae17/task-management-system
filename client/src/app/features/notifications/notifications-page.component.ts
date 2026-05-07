import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { NotificationDto } from '../../core/models/dtos';
import { NotificationsActions } from '../../store/notifications/notifications.actions';
import { notificationsFeature } from '../../store/notifications/notifications.reducer';

@Component({
  selector: 'app-notifications-page',
  imports: [CommonModule, DatePipe, MatButtonModule, MatIconModule, MatDividerModule],
  template: `
    <div class="tm-page" style="max-width: 800px;">
      <header class="tm-page-header">
        <mat-icon style="font-size: 32px; width: 32px; height: 32px;">notifications</mat-icon>
        <h1>Notifications</h1>
        <span class="spacer"></span>
        @if (unread() > 0) {
          <button mat-stroked-button (click)="markAllRead()">
            <mat-icon>done_all</mat-icon>
            Mark all read
          </button>
        }
      </header>

      @if (items().length === 0) {
        <div class="empty">
          <mat-icon>notifications_off</mat-icon>
          <p>You're all caught up.</p>
        </div>
      } @else {
        <ul class="list">
          @for (n of items(); track n.id) {
            <li class="item" [class.unread]="!n.isRead" (click)="open(n)">
              <div class="dot"></div>
              <div class="body">
                <strong>{{ n.title }}</strong>
                <p>{{ n.message }}</p>
                <small>{{ n.createdAt | date: 'medium' }}</small>
              </div>
            </li>
          }
        </ul>
      }
    </div>
  `,
  styles: [`
    .empty {
      text-align: center;
      padding: 64px;
      color: var(--mat-sys-on-surface-variant);
      mat-icon { font-size: 48px; width: 48px; height: 48px; opacity: 0.5; }
      p { margin: 8px 0 0; }
    }
    .list {
      list-style: none;
      padding: 0;
      margin: 0;
      background: var(--mat-sys-surface-container);
      border: 1px solid var(--mat-sys-outline-variant);
      border-radius: 12px;
      overflow: hidden;
    }
    .item {
      display: flex;
      gap: 12px;
      padding: 16px;
      cursor: pointer;
      border-bottom: 1px solid var(--mat-sys-outline-variant);
      transition: background 0.15s ease-out;

      &:last-child { border-bottom: 0; }
      &:hover { background: color-mix(in srgb, var(--mat-sys-primary) 6%, transparent); }
    }
    .item .dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: transparent;
      margin-top: 8px;
      flex-shrink: 0;
    }
    .item.unread .dot { background: var(--mat-sys-primary); }
    .item.unread { background: color-mix(in srgb, var(--mat-sys-primary) 4%, transparent); }
    .item .body {
      flex: 1;
      strong { display: block; margin-bottom: 4px; }
      p { margin: 0 0 4px; color: var(--mat-sys-on-surface-variant); }
      small { color: var(--mat-sys-on-surface-variant); font-size: 0.8rem; }
    }
  `]
})
export class NotificationsPageComponent {
  private readonly store = inject(Store);
  private readonly router = inject(Router);

  readonly items = this.store.selectSignal(notificationsFeature.selectItems);
  readonly unread = this.store.selectSignal(notificationsFeature.selectUnreadCount);

  markAllRead(): void {
    this.store.dispatch(NotificationsActions.markAllRead());
  }

  open(n: NotificationDto): void {
    if (!n.isRead) this.store.dispatch(NotificationsActions.markRead({ id: n.id }));
    if (n.actionUrl) this.router.navigateByUrl(n.actionUrl);
  }
}
