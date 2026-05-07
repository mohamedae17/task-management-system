import { Injectable } from '@angular/core';

const ACCESS_KEY = 'tm.accessToken';
const REFRESH_KEY = 'tm.refreshToken';
const ACCESS_EXPIRES_KEY = 'tm.accessTokenExpiresAt';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getAccessToken(): string | null {
    return this.read(ACCESS_KEY);
  }

  getRefreshToken(): string | null {
    return this.read(REFRESH_KEY);
  }

  getAccessTokenExpiresAt(): Date | null {
    const raw = this.read(ACCESS_EXPIRES_KEY);
    return raw ? new Date(raw) : null;
  }

  store(accessToken: string, refreshToken: string, accessTokenExpiresAt: string): void {
    this.write(ACCESS_KEY, accessToken);
    this.write(REFRESH_KEY, refreshToken);
    this.write(ACCESS_EXPIRES_KEY, accessTokenExpiresAt);
  }

  clear(): void {
    this.remove(ACCESS_KEY);
    this.remove(REFRESH_KEY);
    this.remove(ACCESS_EXPIRES_KEY);
  }

  private read(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  private write(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      /* storage disabled — fall through silently */
    }
  }

  private remove(key: string): void {
    try {
      localStorage.removeItem(key);
    } catch {
      /* storage disabled — fall through silently */
    }
  }
}
