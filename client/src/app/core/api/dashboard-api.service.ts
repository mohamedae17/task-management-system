import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  DashboardStatisticsDto,
  TaskCountByPriorityDto,
  TaskCountByStatusDto,
  UserProductivityDto
} from '../models/dtos';

@Injectable({ providedIn: 'root' })
export class DashboardApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/dashboard`;

  statistics(): Observable<DashboardStatisticsDto> {
    return this.http.get<DashboardStatisticsDto>(`${this.base}/statistics`);
  }

  tasksByStatus(): Observable<TaskCountByStatusDto[]> {
    return this.http.get<TaskCountByStatusDto[]>(`${this.base}/tasks-by-status`);
  }

  tasksByPriority(): Observable<TaskCountByPriorityDto[]> {
    return this.http.get<TaskCountByPriorityDto[]>(`${this.base}/tasks-by-priority`);
  }

  productivity(top = 10): Observable<UserProductivityDto[]> {
    const params = new HttpParams().set('top', top);
    return this.http.get<UserProductivityDto[]>(`${this.base}/productivity`, { params });
  }
}
