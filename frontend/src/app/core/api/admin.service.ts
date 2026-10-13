import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AdminAnalytics } from './models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  analytics() { return this.http.get<AdminAnalytics>('/api/admin/analytics'); }
}
