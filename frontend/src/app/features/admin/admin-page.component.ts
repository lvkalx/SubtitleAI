import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../core/api/admin.service';
import { AdminAnalytics } from '../../core/api/models';
import { AuthService } from '../../core/api/auth.service';
import { I18nService } from '../../core/i18n.service';

@Component({
  selector: 'app-admin-page', imports: [RouterLink], styleUrl: './admin-page.component.scss',
  template: `<main class="admin-shell"><header><a class="brand" routerLink="/jobs">subly<span>AI</span></a><div><a routerLink="/jobs">{{ i18n.t('workspace') }}</a><button type="button" (click)="logout()">{{ i18n.t('logout') }}</button></div></header><section class="heading"><p>{{ i18n.t('admin') }}</p><h1>{{ i18n.t('analyticsTitle') }}</h1><span>{{ i18n.t('analyticsLead') }}</span></section>@if (error()) { <section class="message error">{{ i18n.t(error()) }}</section> } @else if (!analytics()) { <section class="message">{{ i18n.t('loading') }}</section> } @else { <section class="metrics"><article><span>{{ i18n.t('usersTotal') }}</span><strong>{{ analytics()!.users }}</strong><small>{{ i18n.t('newUsers30') }}: {{ analytics()!.usersLast30Days }}</small></article><article><span>{{ i18n.t('transcriptionsTotal') }}</span><strong>{{ analytics()!.transcriptions }}</strong><small>{{ i18n.t('completed') }}: {{ analytics()!.completed }}</small></article><article><span>{{ i18n.t('inProgress') }}</span><strong>{{ analytics()!.processing }}</strong><small>{{ i18n.t('failed') }}: {{ analytics()!.failed }}</small></article><article><span>{{ i18n.t('mediaMinutes') }}</span><strong>{{ analytics()!.totalMediaMinutes }}</strong><small>{{ i18n.t('transcripts') }}: {{ analytics()!.transcripts }}</small></article></section><section class="activity"><div><h2>{{ i18n.t('last7Days') }}</h2><p>{{ i18n.t('activityLead') }}</p></div><div class="bars">@for (day of analytics()!.activity; track day.date) { <div class="bar"><div class="bar-track"><i [style.height.%]="barHeight(day.count)"></i></div><strong>{{ day.count }}</strong><span>{{ day.date.slice(5) }}</span></div> }</div></section> }</main>`
})
export class AdminPageComponent implements OnInit {
  readonly analytics = signal<AdminAnalytics | null>(null);
  readonly error = signal('');
  constructor(private admin: AdminService, private auth: AuthService, readonly i18n: I18nService) {}
  ngOnInit() { this.admin.analytics().subscribe({ next: value => this.analytics.set(value), error: () => this.error.set('adminAccessDenied') }); }
  logout() { this.auth.logout(); location.assign('/auth'); }
  barHeight(count: number) { return Math.max(8, Math.min(100, count * 18)); }
}
