import { Routes } from '@angular/router';
import { AuthPageComponent } from './features/auth/auth-page.component';
import { JobsPageComponent } from './features/jobs/jobs-page.component';
import { JobDetailComponent } from './features/jobs/job-detail.component';
import { HomePageComponent } from './features/home/home-page.component';
import { authGuard, guestGuard } from './core/api/auth.guards';

export const routes: Routes = [
  { path: 'auth', component: AuthPageComponent, canActivate: [guestGuard] },
  { path: 'home', component: HomePageComponent },
  { path: 'jobs', component: JobsPageComponent, canActivate: [authGuard] },
  { path: 'jobs/:id', component: JobDetailComponent, canActivate: [authGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'home' },
  { path: '**', redirectTo: 'home' }
];
