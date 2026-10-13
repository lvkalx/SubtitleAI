import { Routes } from '@angular/router';
import { AuthPageComponent } from './features/auth/auth-page.component';
import { PasswordRecoveryComponent } from './features/auth/password-recovery.component';
import { EmailConfirmationComponent } from './features/auth/email-confirmation.component';
import { JobsPageComponent } from './features/jobs/jobs-page.component';
import { JobDetailComponent } from './features/jobs/job-detail.component';
import { HomePageComponent } from './features/home/home-page.component';
import { AdminPageComponent } from './features/admin/admin-page.component';
import { authGuard, guestGuard } from './core/api/auth.guards';

export const routes: Routes = [
  { path: 'auth', component: AuthPageComponent, canActivate: [guestGuard] },
  { path: 'auth/forgot-password', component: PasswordRecoveryComponent, canActivate: [guestGuard] },
  { path: 'auth/reset-password', component: PasswordRecoveryComponent, data: { mode: 'reset' }, canActivate: [guestGuard] },
  { path: 'auth/confirm-email', component: EmailConfirmationComponent, canActivate: [guestGuard] },
  { path: 'home', component: HomePageComponent },
  { path: 'jobs', component: JobsPageComponent, canActivate: [authGuard] },
  { path: 'jobs/:id', component: JobDetailComponent, canActivate: [authGuard] },
  { path: 'admin', component: AdminPageComponent, canActivate: [authGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'home' },
  { path: '**', redirectTo: 'home' }
];
