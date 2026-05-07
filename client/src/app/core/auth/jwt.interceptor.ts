import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { TokenStorageService } from './token-storage.service';

// Module-scoped state shared across concurrent 401 retries so multiple in-flight requests
// trigger only one refresh attempt instead of stampeding the refresh endpoint.
let isRefreshing = false;
const refreshSignal$ = new BehaviorSubject<string | null>(null);

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const tokens = inject(TokenStorageService);
  const auth = inject(AuthService);
  const router = inject(Router);

  const authReq = attachToken(req, tokens.getAccessToken());

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      const isRefreshCall = req.url.endsWith('/auth/refresh');
      const isLoginCall = req.url.endsWith('/auth/login');
      if (err.status !== 401 || isRefreshCall || isLoginCall) {
        return throwError(() => err);
      }

      if (isRefreshing) {
        return refreshSignal$.pipe(
          filter(token => token !== null),
          take(1),
          switchMap(token => next(attachToken(req, token!)))
        );
      }

      isRefreshing = true;
      refreshSignal$.next(null);

      return auth.refresh().pipe(
        switchMap(r => {
          isRefreshing = false;
          refreshSignal$.next(r.accessToken);
          return next(attachToken(req, r.accessToken));
        }),
        catchError(refreshErr => {
          isRefreshing = false;
          auth.clear();
          router.navigate(['/auth/login'], { queryParams: { returnUrl: router.url } });
          return throwError(() => refreshErr);
        })
      );
    })
  );
};

function attachToken<T>(req: import('@angular/common/http').HttpRequest<T>, token: string | null): import('@angular/common/http').HttpRequest<T> {
  if (!token) return req;
  // Only attach to our API or hub origin so we don't leak the token to third parties.
  if (!req.url.startsWith(environment.apiBaseUrl) && !req.url.startsWith(environment.hubUrl)) return req;
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}
