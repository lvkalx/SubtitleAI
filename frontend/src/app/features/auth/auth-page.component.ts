import { Component, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/api/auth.service';
import { I18nService } from '../../core/i18n.service';

@Component({
  selector: 'app-auth-page',
  imports: [ReactiveFormsModule],
  styleUrl: './auth-page.component.scss',
  template: `<main class="auth-shell">
    <section class="intro"><a class="brand" href="/">subly<span>AI</span></a><div class="intro-copy"><p class="eyebrow">{{ i18n.t('authHeroTag') }}</p><h1 [innerHTML]="i18n.t('authHeroTitle')"></h1><p>{{ i18n.t('authHeroLead') }}</p></div><div class="quote">{{ i18n.t('authQuote') }}</div></section>
    <section class="form-area"><form (submit)="onSubmit($event)" novalidate><div class="form-heading"><p class="eyebrow">{{ i18n.t(isRegister() ? 'newAccount' : 'welcomeBack') }}</p><h2>{{ i18n.t(isRegister() ? 'letsStart' : 'workspaceLogin') }}</h2><p>{{ i18n.t(isRegister() ? 'createAccountHint' : 'continueHint') }}</p></div>
      <label>{{ i18n.t('email') }}<input [formControl]="email" autocomplete="email" type="email" placeholder="you@example.com"></label>
      <label>{{ i18n.t('password') }}<span class="password-field"><input [formControl]="password" [autocomplete]="isRegister() ? 'new-password' : 'current-password'" [type]="showPassword() ? 'text' : 'password'" [placeholder]="i18n.t('minPassword')"><button type="button" class="reveal" (click)="togglePassword()" [attr.aria-label]="showPassword() ? i18n.t('hide') : i18n.t('show')">{{ i18n.t(showPassword() ? 'hide' : 'show') }}</button></span></label>
      @if (isRegister()) { <label>{{ i18n.t('confirmPassword') }}<span class="password-field"><input [formControl]="confirmPassword" autocomplete="new-password" [type]="showConfirmPassword() ? 'text' : 'password'" [placeholder]="i18n.t('repeatPassword')"><button type="button" class="reveal" (click)="toggleConfirmPassword()" [attr.aria-label]="showConfirmPassword() ? i18n.t('hide') : i18n.t('show')">{{ i18n.t(showConfirmPassword() ? 'hide' : 'show') }}</button></span></label> }
      @if (error()) { <p class="error" role="alert">{{ i18n.t(error()) }}</p> }<button class="primary" [disabled]="loading()" type="submit">{{ i18n.t(loading() ? 'creating' : (isRegister() ? 'createAccount' : 'login')) }} <span>→</span></button><button class="switch" type="button" (click)="toggleMode()">{{ i18n.t(isRegister() ? 'haveAccount' : 'firstTime') }}</button></form></section>
  </main>`
})
export class AuthPageComponent {
  readonly email = new FormControl('', [Validators.required, Validators.email]);
  readonly password = new FormControl('', [Validators.required, Validators.minLength(8)]);
  readonly confirmPassword = new FormControl('', [Validators.required]);
  readonly isRegister = signal(false);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);

  constructor(private auth: AuthService, private router: Router, readonly i18n: I18nService) {}

  togglePassword() { this.showPassword.update(value => !value); }
  toggleConfirmPassword() { this.showConfirmPassword.update(value => !value); }
  toggleMode() { this.isRegister.update(v => !v); this.error.set(''); this.confirmPassword.reset(); }
  onSubmit(event: SubmitEvent) { event.preventDefault(); this.send(); }

  send() {
    if (this.email.invalid || this.password.invalid) { this.error.set('invalidCredentials'); return; }
    if (this.isRegister() && this.confirmPassword.value !== this.password.value) { this.error.set('mismatch'); return; }
    this.loading.set(true); this.error.set('');
    const request = this.isRegister() ? this.auth.register(this.email.value!, this.password.value!) : this.auth.login(this.email.value!, this.password.value!);
    request.subscribe({ next: () => this.router.navigateByUrl('/jobs'), error: () => { this.error.set('authFailed'); this.loading.set(false); } });
  }
}
