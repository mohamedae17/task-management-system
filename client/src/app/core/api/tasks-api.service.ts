import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateTaskCommand,
  PaginatedList,
  TaskDto,
  TaskListItemDto,
  UpdateTaskCommand
} from '../models/dtos';
import { TaskItemStatus, TaskPriority } from '../models/enums';

export interface TaskFilters {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDescending?: boolean;
  assigneeId?: string;
  creatorId?: string;
  status?: TaskItemStatus;
  priority?: TaskPriority;
  labelId?: string;
  dueBefore?: string;
  dueAfter?: string;
  isOverdue?: boolean;
}

@Injectable({ providedIn: 'root' })
export class TasksApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/tasks`;

  list(filters: TaskFilters = {}): Observable<PaginatedList<TaskListItemDto>> {
    return this.http.get<PaginatedList<TaskListItemDto>>(this.base, {
      params: this.buildParams(filters as Record<string, unknown>)
    });
  }

  mine(filters: { pageNumber?: number; pageSize?: number; status?: TaskItemStatus } = {}): Observable<PaginatedList<TaskListItemDto>> {
    return this.http.get<PaginatedList<TaskListItemDto>>(`${this.base}/mine`, {
      params: this.buildParams(filters as Record<string, unknown>)
    });
  }

  get(id: string): Observable<TaskDto> {
    return this.http.get<TaskDto>(`${this.base}/${id}`);
  }

  create(command: CreateTaskCommand): Observable<TaskDto> {
    return this.http.post<TaskDto>(this.base, command);
  }

  update(command: UpdateTaskCommand): Observable<TaskDto> {
    return this.http.put<TaskDto>(`${this.base}/${command.taskId}`, command);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  assign(id: string, assigneeId: string | null): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/assign`, { assigneeId });
  }

  changeStatus(id: string, status: TaskItemStatus): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/status`, { status });
  }

  private buildParams(values: Record<string, unknown>): HttpParams {
    let params = new HttpParams();
    for (const [k, v] of Object.entries(values)) {
      if (v !== undefined && v !== null && v !== '') {
        params = params.set(k, String(v));
      }
    }
    return params;
  }
}
