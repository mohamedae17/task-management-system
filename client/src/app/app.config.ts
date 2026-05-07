import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  isDevMode,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection
} from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter, withComponentInputBinding, withInMemoryScrolling } from '@angular/router';
import { provideEffects } from '@ngrx/effects';
import { provideStore, Store } from '@ngrx/store';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { jwtInterceptor } from './core/auth/jwt.interceptor';
import { AuthService } from './core/auth/auth.service';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { routes } from './app.routes';
import { AuthActions } from './store/auth/auth.actions';
import { authFeature } from './store/auth/auth.reducer';
import * as authEffects from './store/auth/auth.effects';
import { notificationsFeature } from './store/notifications/notifications.reducer';
import * as notificationsEffects from './store/notifications/notifications.effects';
import { uiFeature } from './store/ui/ui.reducer';
import * as uiEffects from './store/ui/ui.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideAnimations(),
    provideRouter(
      routes,
      withComponentInputBinding(),
      withInMemoryScrolling({ scrollPositionRestoration: 'enabled', anchorScrolling: 'enabled' })
    ),
    provideHttpClient(withInterceptors([jwtInterceptor, errorInterceptor])),
    provideStore({
      [authFeature.name]: authFeature.reducer,
      [notificationsFeature.name]: notificationsFeature.reducer,
      [uiFeature.name]: uiFeature.reducer
    }),
    provideEffects(authEffects, notificationsEffects, uiEffects),
    provideStoreDevtools({ maxAge: 25, logOnly: !isDevMode(), connectInZone: true }),
    provideCharts(withDefaultRegisterables()),

    // Hydrate the auth slice from local storage on app start so guards see the right state.
    provideAppInitializer(() => {
      const auth = inject(AuthService);
      const store = inject(Store);
      store.dispatch(AuthActions.hydrateFromStorage({ user: auth.user() }));
    })
  ]
};
