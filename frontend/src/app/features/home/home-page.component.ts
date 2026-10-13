import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home-page', imports: [RouterLink], styleUrl: './home-page.component.scss',
  template: `<main><header><a class="brand" routerLink="/home">subly<span>AI</span></a><nav><a href="#how-it-works">Как это работает</a><a routerLink="/auth">Войти</a><a class="nav-cta" routerLink="/auth">Начать бесплатно →</a></nav></header><section class="hero"><p class="eyebrow">Аудио становится текстом</p><h1>Смысл каждой<br><em>сказанной фразы.</em></h1><p class="lead">Загружайте встречи, интервью и видео. Subly AI превратит их в субтитры, текст и краткое резюме.</p><div class="actions"><a class="primary" routerLink="/auth">Создать расшифровку <span>→</span></a><a class="secondary" href="#how-it-works">Узнать больше</a></div></section><section id="how-it-works" class="steps"><article><span>01</span><h2>Загрузите запись</h2><p>Поддерживаются аудио и видео до 500 МБ.</p></article><article><span>02</span><h2>Получите текст</h2><p>Сервис распознает речь и соберёт субтитры.</p></article><article><span>03</span><h2>Скачайте результат</h2><p>Текст, SRT и VTT будут всегда под рукой.</p></article></section></main>`
})
export class HomePageComponent {}
