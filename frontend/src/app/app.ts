import { afterNextRender, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { I18nService } from './core/i18n.service';
@Component({ selector: 'app-root', imports: [RouterOutlet], template: '<router-outlet />', styleUrl: './app.scss' })
export class App {
  private readonly i18n = inject(I18nService);
  constructor() { afterNextRender(() => { this.i18n.localizePage(document.body); new MutationObserver(() => this.i18n.localizePage(document.body)).observe(document.body, { childList: true, subtree: true }); }); }
}
