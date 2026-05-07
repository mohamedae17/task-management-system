import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, forkJoin, map, of, switchMap } from 'rxjs';
import { NotificationsApiService } from '../../core/api/notifications-api.service';
import { NotificationsActions } from './notifications.actions';

export const loadNotificationsEffect$ = createEffect((
  actions$ = inject(Actions),
  api = inject(NotificationsApiService)
) => actions$.pipe(
  ofType(NotificationsActions.load),
  switchMap(() => forkJoin([api.list(false, 1, 20), api.unreadCount()]).pipe(
    map(([page, unread]) => NotificationsActions.loadSuccess({ items: page.items, unreadCount: unread })),
    catchError(err => of(NotificationsActions.loadFailure({ error: err?.message ?? 'Failed to load notifications.' })))
  ))
), { functional: true });

export const markReadEffect$ = createEffect((
  actions$ = inject(Actions),
  api = inject(NotificationsApiService)
) => actions$.pipe(
  ofType(NotificationsActions.markRead),
  switchMap(({ id }) => api.markAsRead(id).pipe(
    map(() => NotificationsActions.markReadSuccess({ id })),
    catchError(() => of(NotificationsActions.markReadSuccess({ id })))
  ))
), { functional: true });

export const markAllReadEffect$ = createEffect((
  actions$ = inject(Actions),
  api = inject(NotificationsApiService)
) => actions$.pipe(
  ofType(NotificationsActions.markAllRead),
  switchMap(() => api.markAllAsRead().pipe(
    map(() => NotificationsActions.markAllReadSuccess()),
    catchError(() => of(NotificationsActions.markAllReadSuccess()))
  ))
), { functional: true });
