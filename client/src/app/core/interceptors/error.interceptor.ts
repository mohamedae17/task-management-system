import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { ProblemDetails } from '../models/dtos';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snack = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      // Don't pop a toast for 401s — the JWT interceptor will refresh or redirect.
      // 422 (BusinessRule) and validation errors are surfaced inline by forms.
      if (err.status === 401) return throwError(() => err);

      const message = extractMessage(err);
      if (message && err.status !== 400 && err.status !== 422) {
        snack.open(message, 'Dismiss', {
          duration: 5000,
          panelClass: ['snack-error']
        });
      }

      return throwError(() => err);
    })
  );
};

function extractMessage(err: HttpErrorResponse): string | null {
  if (typeof err.error === 'string') return err.error;

  const problem = err.error as ProblemDetails | null | undefined;
  if (problem?.detail) return problem.detail;
  if (problem?.title) return problem.title;

  if (err.status === 0) return 'Cannot reach the server. Is the API running?';
  if (err.status >= 500) return 'Something went wrong on the server.';
  return err.message || null;
}
