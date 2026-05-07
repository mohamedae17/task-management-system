import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaginatedList, UserDto, UserSummaryDto } from '../models/dtos';

export interface UserListFilters {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  role?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
}

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/users`;

  me(): Observable<UserDto> { return this.http.get<UserDto>(`${this.base}/me`); }

  updateMe(payload: { firstName: string; lastName: string; avatarUrl?: string | null }): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.base}/me`, payload);
  }

  list(filters: UserListFilters = {}): Observable<PaginatedList<UserDto>> {
    let params = new HttpParams();
    for (const [k, v] of Object.entries(filters)) {
      if (v !== undefined && v !== null && v !== '') params = params.set(k, String(v));
    }
    return this.http.get<PaginatedList<UserDto>>(this.base, { params });
  }

  assignable(search?: string): Observable<UserSummaryDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<UserSummaryDto[]>(`${this.base}/assignable`, { params });
  }

  get(id: string): Observable<UserDto> { return this.http.get<UserDto>(`${this.base}/${id}`); }

  setRoles(id: string, roles: string[]): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/roles`, { roles });
  }

  setActive(id: string, isActive: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/active`, { isActive });
  }
}
