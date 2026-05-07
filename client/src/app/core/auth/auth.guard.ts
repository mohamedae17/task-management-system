import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) return true;

  router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
  return false;
};

export const guestGuard: CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) return true;

  router.navigate(['/dashboard']);
  return false;
};

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (_route, state) => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
      router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    if (auth.hasAnyRole(allowedRoles)) return true;

    router.navigate(['/forbidden']);
    return false;
  };
};
