import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { AuthService } from '../core/auth/auth.service';
import { AppRole } from '../core/models/enums';
import { SignalRService } from '../core/notifications/signalr.service';
import { AuthActions } from '../store/auth/auth.actions';
import { NotificationsActions } from '../store/notifications/notifications.actions';
import { notificationsFeature } from '../store/notifications/notifications.reducer';
import { UiActions } from '../store/ui/ui.actions';
import { uiFeature } from '../store/ui/ui.reducer';

interface NavLink {
  path: string;
  icon: string;
  label: string;
  roles?: string[];
}

@Component({
  selector: 'app-shell',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatSidenavModule,
    MatIconModule,
    MatButtonModule,
    MatListModule,
    MatBadgeModule,
    MatMenuModule,
    MatDividerModule,
    MatTooltipModule
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly store = inject(Store);
  private readonly signalr = inject(SignalRService);
  private readonly router = inject(Router);
  private readonly subs = new Subscription();

  readonly user = this.auth.user;
  readonly theme = this.store.selectSignal(uiFeature.selectTheme);
  readonly sidenavOpen = this.store.selectSignal(uiFeature.selectSidenavOpen);
  readonly unreadCount = this.store.selectSignal(notificationsFeature.selectUnreadCount);

  readonly allLinks: NavLink[] = [
    { path: '/dashboard',     icon: 'dashboard',      label: 'Dashboard' },
    { path: '/tasks',         icon: 'task_alt',       label: 'Tasks' },
    { path: '/tasks/board',   icon: 'view_kanban',    label: 'Kanban Board' },
    { path: '/notifications', icon: 'notifications',  label: 'Notifications' },
    { path: '/profile',       icon: 'account_circle', label: 'Profile' },
    { path: '/admin/users',   icon: 'group',          label: 'Users (Admin)', roles: [AppRole.Admin, AppRole.Manager] }
  ];

  readonly navLinks = computed(() =>
    this.allLinks.filter(link => !link.roles || this.auth.hasAnyRole(link.roles))
  );

  ngOnInit(): void {
    void this.signalr.connect();
    this.store.dispatch(NotificationsActions.load());

    this.subs.add(this.signalr.notification$.subscribe(item =>
      this.store.dispatch(NotificationsActions.receivedLive({ item }))
    ));
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    void this.signalr.disconnect();
  }

  toggleSidenav(): void { this.store.dispatch(UiActions.toggleSidenav()); }
  toggleTheme(): void { this.store.dispatch(UiActions.toggleTheme()); }

  logout(): void {
    this.store.dispatch(AuthActions.logout());
  }

  goToProfile(): void { this.router.navigateByUrl('/profile'); }
}
