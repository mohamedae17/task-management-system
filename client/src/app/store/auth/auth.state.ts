import { UserDto } from '../../core/models/dtos';

export interface AuthState {
  user: UserDto | null;
  loading: boolean;
  error: string | null;
}

export const initialAuthState: AuthState = {
  user: null,
  loading: false,
  error: null
};
