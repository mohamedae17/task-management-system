import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NotificationDto, PaginatedList } from '../models/dtos';

@Injectable({ providedIn: 'root' })
export class NotificationsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/notifications`;

  list(unreadOnly = false, pageNumber = 1, pageSize = 20): Observable<PaginatedList<NotificationDto>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    if (unreadOnly) params = params.set('unreadOnly', 'true');
    return this.http.get<PaginatedList<NotificationDto>>(this.base, { params });
  }

  unreadCount(): Observable<number> {
    return this.http.get<number>(`${this.base}/unread-count`);
  }

  markAsRead(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/read`, {});
  }

  markAllAsRead(): Observable<{ updated: number }> {
    return this.http.post<{ updated: number }>(`${this.base}/read-all`, {});
  }
}
