import { createFeature, createReducer, on } from '@ngrx/store';
import { UiActions } from './ui.actions';
import { initialUiState } from './ui.state';

export const uiFeature = createFeature({
  name: 'ui',
  reducer: createReducer(
    initialUiState,
    on(UiActions.setTheme, (state, { theme }) => ({ ...state, theme })),
    on(UiActions.toggleTheme, state => ({ ...state, theme: state.theme === 'dark' ? 'light' : 'dark' })),
    on(UiActions.toggleSidenav, state => ({ ...state, sidenavOpen: !state.sidenavOpen })),
    on(UiActions.setSidenav, (state, { open }) => ({ ...state, sidenavOpen: open }))
  )
});
