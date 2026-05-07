import { createFeature, createReducer, on } from '@ngrx/store';
import { AuthActions } from './auth.actions';
import { initialAuthState } from './auth.state';

export const authFeature = createFeature({
  name: 'auth',
  reducer: createReducer(
    initialAuthState,
    on(AuthActions.login, AuthActions.register, state => ({ ...state, loading: true, error: null })),
    on(AuthActions.loginSuccess, AuthActions.registerSuccess, (state, { user }) =>
      ({ ...state, user, loading: false, error: null })),
    on(AuthActions.loginFailure, AuthActions.registerFailure, (state, { error }) =>
      ({ ...state, loading: false, error })),
    on(AuthActions.hydrateFromStorage, (state, { user }) => ({ ...state, user })),
    on(AuthActions.logoutComplete, () => ({ ...initialAuthState }))
  )
});
