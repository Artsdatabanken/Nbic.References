import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Reference } from '../reference/reference';

@Injectable({ providedIn: 'root' })
export class ReferencesService {
  private apiBase = '/api/references';

  constructor(private http: HttpClient) {}

  getAll(offset: number, limit: number, search: string | null): Observable<Reference[]> {
    const params = new URLSearchParams({
      offset: offset.toString(),
      limit: limit.toString(),
    });
    if (search) params.set('search', search);
    return this.http.get<Reference[]>(`${this.apiBase}?${params.toString()}`);
  }

  getById(id: string): Observable<Reference> {
    return this.http.get<Reference>(`${this.apiBase}/${id}`);
  }
}
