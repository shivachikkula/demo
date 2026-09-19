import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { AdminRosterResponse } from './api.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/admin`;

  getRoster(search?: string, sortBy?: string, sortDir?: 'asc' | 'desc'): Observable<AdminRosterResponse> {
    const params: Record<string, string> = {};
    if (search) {
      params['search'] = search;
    }
    if (sortBy) {
      params['sortBy'] = sortBy;
    }
    if (sortDir) {
      params['sortDir'] = sortDir;
    }

    return this.http.get<AdminRosterResponse>(`${this.baseUrl}/leas`, { params });
  }
}
