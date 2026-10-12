import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

// Public authentication requests must not include a bearer token or trigger a refresh loop.
const PUBLIC = /^\/api\/auth\/(login|register|refresh|logout)$/;

const withToken = (req: HttpRequest<unknown>, token: string | null) =>
  token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (PUBLIC.test(req.url)) return next(req);

  const sessionExpired = () => {
    auth.clear();
    router.navigateByUrl('/auth');
  };

  return next(withToken(req, auth.accessToken)).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status !== 401) return throwError(() => err);

      return auth.refresh().pipe(
        switchMap(res => next(withToken(req, res.accessToken))),
        catchError(refreshErr => {
          sessionExpired();
          return throwError(() => refreshErr);
        })
      );
    })
  );
};
