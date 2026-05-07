import { HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ProblemDetails } from '../../core/models/dtos';
import { AuthActions } from './auth.actions';

export const loginEffect$ = createEffect((
  actions$ = inject(Actions),
  auth = inject(AuthService)
) => actions$.pipe(
  ofType(AuthActions.login),
  switchMap(({ email, password, returnUrl }) =>
    auth.login(email, password).pipe(
      map(r => AuthActions.loginSuccess({ user: r.user, returnUrl })),
      catchError((err: HttpErrorResponse) => of(AuthActions.loginFailure({ error: explain(err) ?? 'Login failed.' })))
    ))
), { functional: true });

export const loginSuccessRedirect$ = createEffect((
  actions$ = inject(Actions),
  router = inject(Router)
) => actions$.pipe(
  ofType(AuthActions.loginSuccess),
  tap(({ returnUrl }) => router.navigateByUrl(returnUrl ?? '/dashboard'))
), { functional: true, dispatch: false });

export const registerSuccessRedirect$ = createEffect((
  actions$ = inject(Actions),
  router = inject(Router)
) => actions$.pipe(
  ofType(AuthActions.registerSuccess),
  tap(() => router.navigateByUrl('/dashboard'))
), { functional: true, dispatch: false });

export const registerEffect$ = createEffect((
  actions$ = inject(Actions),
  auth = inject(AuthService)
) => actions$.pipe(
  ofType(AuthActions.register),
  switchMap(payload =>
    auth.register(payload).pipe(
      map(r => AuthActions.registerSuccess({ user: r.user })),
      catchError((err: HttpErrorResponse) => of(AuthActions.registerFailure({ error: explain(err) ?? 'Registration failed.' })))
    ))
), { functional: true });

export const logoutEffect$ = createEffect((
  actions$ = inject(Actions),
  auth = inject(AuthService),
  router = inject(Router)
) => actions$.pipe(
  ofType(AuthActions.logout),
  switchMap(() =>
    auth.logout().pipe(
      catchError(() => of(void 0)),
      map(() => {
        auth.clear();
        router.navigateByUrl('/auth/login');
        return AuthActions.logoutComplete();
      })
    ))
), { functional: true });

function explain(err: HttpErrorResponse): string | null {
  const problem = err.error as ProblemDetails | null | undefined;
  if (problem?.errors) {
    const messages = Object.values(problem.errors).flat();
    if (messages.length) return messages[0]!;
  }
  if (problem?.detail) return problem.detail;
  if (problem?.title) return problem.title;
  if (typeof err.error === 'string') return err.error;
  if (err.status === 0) return 'Cannot reach the server.';
  return null;
}
