import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { tap } from 'rxjs';
import { UiActions } from './ui.actions';

export const persistThemeEffect$ = createEffect((actions$ = inject(Actions)) =>
  actions$.pipe(
    ofType(UiActions.setTheme, UiActions.toggleTheme),
    tap(() => {
      // Persist whatever the current document body class declares.
      const body = document.body;
      const isDark = body.classList.contains('theme-dark');
      try { localStorage.setItem('tm.theme', isDark ? 'dark' : 'light'); } catch { /* noop */ }
    })
  ), { functional: true, dispatch: false });
