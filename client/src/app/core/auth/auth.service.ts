import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponseDto, UserDto } from '../models/dtos';
import { TokenStorageService } from './token-storage.service';

interface RegisterPayload {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokens = inject(TokenStorageService);
  private readonly base = `${environment.apiBaseUrl}/auth`;

  private readonly _user = signal<UserDto | null>(this.restoreUser());
  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);
  readonly roles = computed(() => this._user()?.roles ?? []);

  hasRole(role: string): boolean {
    return this.roles().some(r => r.toLowerCase() === role.toLowerCase());
  }

  hasAnyRole(roles: string[]): boolean {
    if (!roles?.length) return true;
    return roles.some(r => this.hasRole(r));
  }

  login(email: string, password: string): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.base}/login`, { email, password })
      .pipe(tap(r => this.persist(r)));
  }

  register(payload: RegisterPayload): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.base}/register`, payload)
      .pipe(tap(r => this.persist(r)));
  }

  refresh(): Observable<AuthResponseDto> {
    const refreshToken = this.tokens.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available.');
    }
    return this.http.post<AuthResponseDto>(`${this.base}/refresh`, { refreshToken })
      .pipe(tap(r => this.persist(r)));
  }

  logout(allSessions = false): Observable<void> {
    const refreshToken = this.tokens.getRefreshToken();
    return this.http
      .post<void>(`${this.base}/logout`, { refreshToken, allSessions })
      .pipe(tap(() => this.clear()));
  }

  forgotPassword(email: string): Observable<void> {
    return this.http.post<void>(`${this.base}/forgot-password`, { email });
  }

  resetPassword(userId: string, token: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.base}/reset-password`, { userId, token, newPassword });
  }

  verifyEmail(userId: string, token: string): Observable<void> {
    return this.http.post<void>(`${this.base}/verify-email`, { userId, token });
  }

  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.base}/change-password`, { currentPassword, newPassword });
  }

  setUser(user: UserDto | null): void {
    this._user.set(user);
    if (user) {
      try { localStorage.setItem('tm.user', JSON.stringify(user)); } catch { /* noop */ }
    } else {
      try { localStorage.removeItem('tm.user'); } catch { /* noop */ }
    }
  }

  clear(): void {
    this.tokens.clear();
    this.setUser(null);
  }

  private persist(r: AuthResponseDto): void {
    this.tokens.store(r.accessToken, r.refreshToken, r.accessTokenExpiresAt);
    this.setUser(r.user);
  }

  private restoreUser(): UserDto | null {
    try {
      const raw = localStorage.getItem('tm.user');
      return raw ? (JSON.parse(raw) as UserDto) : null;
    } catch {
      return null;
    }
  }
}
