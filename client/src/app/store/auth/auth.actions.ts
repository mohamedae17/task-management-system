import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { UserDto } from '../../core/models/dtos';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Login': props<{ email: string; password: string; returnUrl?: string }>(),
    'Login Success': props<{ user: UserDto; returnUrl?: string }>(),
    'Login Failure': props<{ error: string }>(),

    'Register': props<{ email: string; password: string; firstName: string; lastName: string }>(),
    'Register Success': props<{ user: UserDto }>(),
    'Register Failure': props<{ error: string }>(),

    'Hydrate From Storage': props<{ user: UserDto | null }>(),

    'Logout': emptyProps(),
    'Logout Complete': emptyProps()
  }
});
