import { Routes } from '@angular/router';
import { AuthPageComponent } from './features/auth/auth-page.component';
import { JobsPageComponent } from './features/jobs/jobs-page.component';
export const routes: Routes = [{ path: 'auth', component: AuthPageComponent }, { path: 'jobs', component: JobsPageComponent }, { path: '', pathMatch: 'full', redirectTo: 'jobs' }, { path: '**', redirectTo: 'jobs' }];
