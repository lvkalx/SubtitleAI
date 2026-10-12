import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.authenticated()
    ? true
    : auth.restoreSession().pipe(map(valid => valid ? true : router.createUrlTree(['/auth'])));
};

export const guestGuard: CanActivateFn = () =>
  inject(AuthService).authenticated() ? inject(Router).createUrlTree(['/jobs']) : true;
