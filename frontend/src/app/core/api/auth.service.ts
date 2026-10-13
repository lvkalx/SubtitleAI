import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';
import { AuthResponse, User } from './models';

const ACCESS_KEY = 'subtitleai.accessToken';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private refreshing$: Observable<AuthResponse> | null = null;

  // The refresh token is an HttpOnly cookie and is never readable by JavaScript.
  readonly authenticated = signal(!!sessionStorage.getItem(ACCESS_KEY));

  get accessToken(): string | null {
    return sessionStorage.getItem(ACCESS_KEY);
  }

  login(email: string, password: string) {
    return this.http
      .post<AuthResponse>('/api/auth/login', { email, password }, { withCredentials: true })
      .pipe(tap(x => this.save(x)));
  }

  register(email: string, password: string) {
    return this.http
      .post<AuthResponse>('/api/auth/register', { email, password }, { withCredentials: true })
      .pipe(tap(x => this.save(x)));
  }

  requestPasswordReset(email: string) {
    return this.http.post('/api/auth/request-password-reset', { email });
  }

  resetPassword(userId: string, token: string, password: string) {
    return this.http.post('/api/auth/reset-password', { userId, token, password });
  }

  confirmEmail(userId: string, token: string) {
    return this.http.post('/api/auth/confirm-email', { userId, token });
  }

  me() {
    return this.http.get<User>('/api/auth/me');
  }

  restoreSession(): Observable<boolean> {
    if (this.accessToken) return of(true);
    return this.refresh().pipe(
      map(() => true),
      catchError(() => of(false))
    );
  }

  // Concurrent 401 responses share one refresh request.
  refresh(): Observable<AuthResponse> {
    this.refreshing$ ??= this.http
      .post<AuthResponse>('/api/auth/refresh', {}, { withCredentials: true })
      .pipe(
        tap(x => this.save(x)),
        finalize(() => (this.refreshing$ = null)),
        shareReplay(1)
      );
    return this.refreshing$;
  }

  logout() {
    this.clear();
    // The local session is already cleared if this request fails.
    this.http.post('/api/auth/logout', {}, { withCredentials: true }).subscribe({ error: () => {} });
  }

  clear() {
    sessionStorage.removeItem(ACCESS_KEY);
    this.authenticated.set(false);
  }

  private save(x: AuthResponse) {
    sessionStorage.setItem(ACCESS_KEY, x.accessToken);
    this.authenticated.set(true);
  }
}
