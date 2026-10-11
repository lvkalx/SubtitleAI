import { HttpInterceptorFn } from '@angular/common/http';
export const authInterceptor: HttpInterceptorFn = (request, next) => { const token = localStorage.getItem('subtitleai.accessToken'); return next(token ? request.clone({ setHeaders: { Authorization: 'Bearer ' + token } }) : request); };
