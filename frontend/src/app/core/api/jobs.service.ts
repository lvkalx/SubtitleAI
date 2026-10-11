import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Job, Summary } from './models';
@Injectable({ providedIn: 'root' })
export class JobsService { constructor(private http: HttpClient) {} list() { return this.http.get<Job[]>('/api/jobs'); } get(id: string) { return this.http.get<Job>('/api/jobs/' + id); } upload(file: File) { const data = new FormData(); data.append('file', file); return this.http.post<Job>('/api/jobs', data); } transcript(id: string) { return this.http.get<{ text: string }>('/api/jobs/' + id + '/transcript'); } summary(id: string) { return this.http.get<Summary>('/api/jobs/' + id + '/summary'); } srtUrl(id: string) { return '/api/jobs/' + id + '/srt'; } vttUrl(id: string) { return '/api/jobs/' + id + '/vtt'; } }
