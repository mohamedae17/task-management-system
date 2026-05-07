export type ThemeMode = 'light' | 'dark';

export interface UiState {
  theme: ThemeMode;
  sidenavOpen: boolean;
}

export const initialUiState: UiState = {
  theme: (typeof localStorage !== 'undefined' && localStorage.getItem('tm.theme') === 'dark') ? 'dark' : 'light',
  sidenavOpen: true
};
