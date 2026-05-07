import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LabelDto } from '../models/dtos';

@Injectable({ providedIn: 'root' })
export class LabelsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/labels`;

  list(): Observable<LabelDto[]> { return this.http.get<LabelDto[]>(this.base); }

  create(name: string, colorHex: string, description?: string | null): Observable<LabelDto> {
    return this.http.post<LabelDto>(this.base, { name, colorHex, description });
  }

  update(id: string, name: string, colorHex: string, description?: string | null): Observable<LabelDto> {
    return this.http.put<LabelDto>(`${this.base}/${id}`, { labelId: id, name, colorHex, description });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
