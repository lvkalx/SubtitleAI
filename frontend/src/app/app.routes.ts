import { Routes } from '@angular/router';
import { AuthPageComponent } from './features/auth/auth-page.component';
import { JobsPageComponent } from './features/jobs/jobs-page.component';
import { authGuard, guestGuard } from './core/api/auth.guards';

export const routes: Routes = [
  { path: 'auth', component: AuthPageComponent, canActivate: [guestGuard] },
  { path: 'jobs', component: JobsPageComponent, canActivate: [authGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'jobs' },
  { path: '**', redirectTo: 'jobs' }
];
