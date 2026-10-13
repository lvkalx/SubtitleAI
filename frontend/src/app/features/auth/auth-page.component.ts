import { Component, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/api/auth.service';

@Component({
  selector: 'app-auth-page',
  imports: [ReactiveFormsModule],
  styleUrl: './auth-page.component.scss',
  template: `<main class="auth-shell">
    <section class="intro"><a class="brand" href="/">subly<span>AI</span></a><div class="intro-copy"><p class="eyebrow">Субтитры за минуты</p><h1>Пусть каждое слово <em>звучит</em> дальше.</h1><p>Загружайте записи, получайте аккуратные субтитры и короткую выжимку смысла.</p></div><div class="quote">«Превращаю голос в текст, который хочется читать.»</div></section>
    <section class="form-area"><form (submit)="onSubmit($event)" novalidate><div class="form-heading"><p class="eyebrow">{{ isRegister() ? 'Новый аккаунт' : 'С возвращением' }}</p><h2>{{ isRegister() ? 'Начнём?' : 'Войти в рабочее пространство' }}</h2><p>{{ isRegister() ? 'Создайте аккаунт — это займёт меньше минуты.' : 'Продолжайте работать с вашими расшифровками.' }}</p></div>
      <label>Email<input [formControl]="email" autocomplete="email" type="email" placeholder="you@example.com"></label>
      <label>Пароль<span class="password-field"><input [formControl]="password" [autocomplete]="isRegister() ? 'new-password' : 'current-password'" [type]="showPassword() ? 'text' : 'password'" placeholder="Минимум 8 символов"><button type="button" class="reveal" (click)="togglePassword()" [attr.aria-label]="showPassword() ? 'Скрыть пароль' : 'Показать пароль'">{{ showPassword() ? 'Скрыть' : 'Показать' }}</button></span></label>
      @if (isRegister()) { <label>Подтвердите пароль<span class="password-field"><input [formControl]="confirmPassword" autocomplete="new-password" [type]="showConfirmPassword() ? 'text' : 'password'" placeholder="Повторите пароль"><button type="button" class="reveal" (click)="toggleConfirmPassword()" [attr.aria-label]="showConfirmPassword() ? 'Скрыть пароль' : 'Показать пароль'">{{ showConfirmPassword() ? 'Скрыть' : 'Показать' }}</button></span></label> }
      @if (error()) { <p class="error" role="alert">{{ error() }}</p> }<button class="primary" [disabled]="loading()" type="submit">{{ loading() ? 'Подождите…' : (isRegister() ? 'Создать аккаунт' : 'Войти') }} <span>→</span></button><button class="switch" type="button" (click)="toggleMode()">{{ isRegister() ? 'Уже есть аккаунт? Войти' : 'Впервые здесь? Создать аккаунт' }}</button></form></section>
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

  constructor(private auth: AuthService, private router: Router) {}

  togglePassword() { this.showPassword.update(value => !value); }
  toggleConfirmPassword() { this.showConfirmPassword.update(value => !value); }
  toggleMode() { this.isRegister.update(v => !v); this.error.set(''); this.confirmPassword.reset(); }
  onSubmit(event: SubmitEvent) { event.preventDefault(); this.send(); }

  send() {
    if (this.email.invalid || this.password.invalid) { this.error.set('Укажите корректный email и пароль от 8 символов.'); return; }
    if (this.isRegister() && this.confirmPassword.value !== this.password.value) { this.error.set('Пароли не совпадают.'); return; }
    this.loading.set(true); this.error.set('');
    const request = this.isRegister() ? this.auth.register(this.email.value!, this.password.value!) : this.auth.login(this.email.value!, this.password.value!);
    request.subscribe({ next: () => this.router.navigateByUrl('/jobs'), error: () => { this.error.set('Не удалось авторизоваться. Проверьте данные и повторите.'); this.loading.set(false); } });
  }
}
