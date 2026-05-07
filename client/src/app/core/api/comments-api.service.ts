import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CommentDto, PaginatedList } from '../models/dtos';

@Injectable({ providedIn: 'root' })
export class CommentsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  list(taskId: string, pageNumber = 1, pageSize = 50): Observable<PaginatedList<CommentDto>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PaginatedList<CommentDto>>(`${this.base}/tasks/${taskId}/comments`, { params });
  }

  create(taskId: string, content: string): Observable<CommentDto> {
    return this.http.post<CommentDto>(`${this.base}/tasks/${taskId}/comments`, { content });
  }

  update(commentId: string, content: string): Observable<CommentDto> {
    return this.http.put<CommentDto>(`${this.base}/comments/${commentId}`, { content });
  }

  delete(commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/comments/${commentId}`);
  }
}
