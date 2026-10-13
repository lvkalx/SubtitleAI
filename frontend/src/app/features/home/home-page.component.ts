import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { I18nService } from '../../core/i18n.service';

@Component({
  selector: 'app-home-page', imports: [RouterLink], styleUrl: './home-page.component.scss',
  template: `<main><header><a class="brand" routerLink="/home">subly<span>AI</span></a><nav><a href="#how-it-works">{{ i18n.t('homeHow') }}</a><a routerLink="/auth">{{ i18n.t('signIn') }}</a><a class="nav-cta" routerLink="/auth">{{ i18n.t('startFree') }}</a></nav></header><section class="hero"><p class="eyebrow">{{ i18n.t('brandTag') }}</p><h1>{{ i18n.t('homeTitle').split('\n')[0] }}<br><em>{{ i18n.t('homeTitle').split('\n')[1] }}</em></h1><p class="lead">{{ i18n.t('homeLead') }}</p><div class="actions"><a class="primary" routerLink="/auth">{{ i18n.t('createTranscript') }} <span>→</span></a><a class="secondary" href="#how-it-works">{{ i18n.t('learnMore') }}</a></div></section><section id="how-it-works" class="steps"><article><span>01</span><h2>{{ i18n.t('step1') }}</h2><p>{{ i18n.t('step1d') }}</p></article><article><span>02</span><h2>{{ i18n.t('step2') }}</h2><p>{{ i18n.t('step2d') }}</p></article><article><span>03</span><h2>{{ i18n.t('step3') }}</h2><p>{{ i18n.t('step3d') }}</p></article></section></main>`
})
export class HomePageComponent {
  constructor(readonly i18n: I18nService) {}
}
