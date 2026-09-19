import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { CollectionDetail, LeaDashboard, SubmissionRecord } from './api.models';

@Injectable({ providedIn: 'root' })
export class LeaDashboardApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/leas`;

  getDashboard(leaId: string): Observable<LeaDashboard> {
    return this.http.get<LeaDashboard>(`${this.baseUrl}/${leaId}`);
  }

  getCollectionDetail(leaId: string, collectionId: string): Observable<CollectionDetail> {
    return this.http.get<CollectionDetail>(`${this.baseUrl}/${leaId}/collections/${collectionId}`);
  }

  uploadSubmission(leaId: string, collectionId: string, file: File, uploadedBy?: string): Observable<SubmissionRecord> {
    const formData = new FormData();
    formData.append('file', file);
    if (uploadedBy) {
      formData.append('uploadedBy', uploadedBy);
    }

    return this.http.post<SubmissionRecord>(
      `${this.baseUrl}/${leaId}/collections/${collectionId}/submissions`,
      formData,
    );
  }

  downloadUrl(leaId: string, collectionId: string, submissionId: string): string {
    return `${this.baseUrl}/${leaId}/collections/${collectionId}/submissions/${submissionId}/download`;
  }
}
