import { Routes } from '@angular/router';
import { authGuard, guestGuard, roleGuard } from './core/auth/auth.guard';
import { AppRole } from './core/models/enums';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },

  {
    path: 'auth',
    canMatch: [guestGuard],
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },

  {
    path: '',
    loadComponent: () => import('./layout/shell.component').then(m => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'tasks',
        loadChildren: () => import('./features/tasks/tasks.routes').then(m => m.TASKS_ROUTES)
      },
      {
        path: 'notifications',
        loadComponent: () => import('./features/notifications/notifications-page.component').then(m => m.NotificationsPageComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
      },
      {
        path: 'admin/users',
        canActivate: [roleGuard([AppRole.Admin, AppRole.Manager])],
        loadComponent: () => import('./features/users/users-list.component').then(m => m.UsersListComponent)
      }
    ]
  },

  {
    path: 'forbidden',
    loadComponent: () => import('./features/error/forbidden.component').then(m => m.ForbiddenComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];
