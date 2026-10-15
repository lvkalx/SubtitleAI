import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/api/auth.service';
import { I18nService } from '../../core/i18n.service';

@Component({
  selector: 'app-password-recovery',
  imports: [ReactiveFormsModule, RouterLink],
  styleUrl: './password-recovery.component.scss',
  template: `<main class="utility-shell"><a class="brand" routerLink="/">subly<span>AI</span></a><section>
    @if (!isReset()) {
      <p class="eyebrow">{{ i18n.t('passwordRecovery') }}</p><h1>{{ i18n.t('resetPasswordTitle') }}</h1><p>{{ i18n.t('resetPasswordLead') }}</p>
      <form (submit)="requestReset($event)" novalidate><label>{{ i18n.t('email') }}<input [formControl]="email" autocomplete="email" type="email" placeholder="you@example.com"></label>
      @if (message()) { <p class="success" role="status">{{ i18n.t(message()) }}</p> } @if (error()) { <p class="error" role="alert">{{ i18n.t(error()) }}</p> }
      <button class="primary" [disabled]="loading()" type="submit">{{ i18n.t(loading() ? 'sendingEmail' : 'sendResetLink') }}</button></form>
    } @else {
      <p class="eyebrow">{{ i18n.t('passwordRecovery') }}</p><h1>{{ i18n.t('chooseNewPassword') }}</h1><p>{{ i18n.t('newPasswordLead') }}</p>
      <form (submit)="resetPassword($event)" novalidate><label>{{ i18n.t('password') }}<span class="password-field"><input [formControl]="password" autocomplete="new-password" [type]="showPassword() ? 'text' : 'password'" [placeholder]="i18n.t('minPassword')"><button type="button" class="reveal" (click)="togglePassword()" [attr.aria-label]="i18n.t(showPassword() ? 'hide' : 'show')">{{ i18n.t(showPassword() ? 'hide' : 'show') }}</button></span></label><label>{{ i18n.t('confirmPassword') }}<span class="password-field"><input [formControl]="confirmPassword" autocomplete="new-password" [type]="showConfirmPassword() ? 'text' : 'password'" [placeholder]="i18n.t('repeatPassword')"><button type="button" class="reveal" (click)="toggleConfirmPassword()" [attr.aria-label]="i18n.t(showConfirmPassword() ? 'hide' : 'show')">{{ i18n.t(showConfirmPassword() ? 'hide' : 'show') }}</button></span></label>
      @if (message()) { <p class="success" role="status">{{ i18n.t(message()) }}</p> } @if (error()) { <p class="error" role="alert">{{ i18n.t(error()) }}</p> }
      <button class="primary" [disabled]="loading()" type="submit">{{ i18n.t(loading() ? 'savingPassword' : 'saveNewPassword') }}</button></form>
    }
    <button class="link" type="button" (click)="backToLogin()">{{ i18n.t('backToSignIn') }}</button>
  </section></main>`
})
export class PasswordRecoveryComponent {
  readonly email = new FormControl('', [Validators.required, Validators.email]);
  readonly password = new FormControl('', [Validators.required, Validators.minLength(8)]);
  readonly confirmPassword = new FormControl('', [Validators.required]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly message = signal('');
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly isReset = computed(() => this.route.snapshot.data['mode'] === 'reset');

  constructor(private auth: AuthService, private route: ActivatedRoute, private router: Router, readonly i18n: I18nService) {}

  requestReset(event: SubmitEvent) {
    event.preventDefault();
    if (this.email.hasError('required')) return this.error.set('emailRequired');
    if (this.email.invalid) return this.error.set('invalidEmail');
    this.loading.set(true); this.error.set(''); this.message.set('');
    this.auth.requestPasswordReset(this.email.value!).subscribe({
      next: () => { this.message.set('passwordResetEmailSent'); this.loading.set(false); },
      error: error => { this.error.set(this.errorKey(error, 'emailDeliveryUnavailable')); this.loading.set(false); }
    });
  }

  resetPassword(event: SubmitEvent) {
    event.preventDefault();
    const userId = this.route.snapshot.queryParamMap.get('userId');
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!userId || !token) return this.error.set('invalidOrExpiredPasswordReset');
    if (this.password.hasError('required')) return this.error.set('passwordRequired');
    if (this.password.hasError('minlength')) return this.error.set('passwordTooShort');
    if (this.confirmPassword.hasError('required')) return this.error.set('confirmPasswordRequired');
    if (this.password.value !== this.confirmPassword.value) return this.error.set('mismatch');
    this.loading.set(true); this.error.set(''); this.message.set('');
    this.auth.resetPassword(userId, token, this.password.value!).subscribe({
      next: () => { this.message.set('passwordChanged'); this.loading.set(false); },
      error: error => { this.error.set(this.errorKey(error, 'invalidOrExpiredPasswordReset')); this.loading.set(false); }
    });
  }

  backToLogin() { this.router.navigateByUrl('/auth'); }
  togglePassword() { this.showPassword.update(value => !value); }
  toggleConfirmPassword() { this.showConfirmPassword.update(value => !value); }

  private errorKey(error: unknown, fallback: string) {
    const response = error as HttpErrorResponse;
    const errors = response.error?.errors;
    if (Array.isArray(errors) && typeof errors[0] === 'string') return errors[0];
    if (response.status === 0) return 'networkError';
    if (response.status === 429) return 'tooManyAttempts';
    if (response.status >= 500) return 'serverError';
    return fallback;
  }
}
