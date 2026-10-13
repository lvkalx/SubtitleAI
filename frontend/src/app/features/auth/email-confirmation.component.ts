import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/api/auth.service';
import { I18nService } from '../../core/i18n.service';

@Component({
  selector: 'app-email-confirmation',
  imports: [RouterLink],
  styleUrl: './password-recovery.component.scss',
  template: `<main class="utility-shell"><a class="brand" routerLink="/">subly<span>AI</span></a><section><p class="eyebrow">{{ i18n.t('emailConfirmation') }}</p><h1>{{ i18n.t('confirmingEmail') }}</h1>@if (message()) { <p class="success" role="status">{{ i18n.t(message()) }}</p> } @if (error()) { <p class="error" role="alert">{{ i18n.t(error()) }}</p> }<button class="link" type="button" (click)="goToLogin()">{{ i18n.t('backToSignIn') }}</button></section></main>`
})
export class EmailConfirmationComponent {
  readonly message = signal('');
  readonly error = signal('');

  constructor(private auth: AuthService, route: ActivatedRoute, private router: Router, readonly i18n: I18nService) {
    const userId = route.snapshot.queryParamMap.get('userId');
    const token = route.snapshot.queryParamMap.get('token');
    if (!userId || !token) { this.error.set('invalidOrExpiredEmailConfirmation'); return; }
    this.auth.confirmEmail(userId, token).subscribe({
      next: () => this.message.set('emailConfirmed'),
      error: (error: HttpErrorResponse) => this.error.set(error.error?.errors?.[0] ?? 'invalidOrExpiredEmailConfirmation')
    });
  }

  goToLogin() { this.router.navigateByUrl('/auth'); }
}
