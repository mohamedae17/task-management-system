import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { ThemeMode } from './ui.state';

export const UiActions = createActionGroup({
  source: 'UI',
  events: {
    'Set Theme': props<{ theme: ThemeMode }>(),
    'Toggle Theme': emptyProps(),
    'Toggle Sidenav': emptyProps(),
    'Set Sidenav': props<{ open: boolean }>()
  }
});
