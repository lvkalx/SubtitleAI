import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { AuthResponse, User } from './models';
@Injectable({ providedIn: 'root' })
export class AuthService { private readonly key = 'subtitleai.accessToken'; readonly authenticated = signal(!!localStorage.getItem(this.key)); readonly token = computed(() => localStorage.getItem(this.key)); constructor(private http: HttpClient) {} login(email: string, password: string) { return this.http.post<AuthResponse>('/api/auth/login', { email, password }).pipe(tap(x => this.save(x))); } register(email: string, password: string) { return this.http.post<AuthResponse>('/api/auth/register', { email, password }).pipe(tap(x => this.save(x))); } me() { return this.http.get<User>('/api/auth/me'); } logout() { localStorage.removeItem(this.key); localStorage.removeItem('subtitleai.refreshToken'); this.authenticated.set(false); } private save(x: AuthResponse) { localStorage.setItem(this.key, x.accessToken); localStorage.setItem('subtitleai.refreshToken', x.refreshToken); this.authenticated.set(true); } }
