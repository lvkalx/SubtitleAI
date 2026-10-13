import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { JobsService } from '../../core/api/jobs.service';
import { Job, Summary } from '../../core/api/models';
import { I18nService } from '../../core/i18n.service';

@Component({
  selector: 'app-job-detail', imports: [RouterLink], styleUrl: './job-detail.component.scss',
  template: `<main class="page"><header><a class="brand" routerLink="/jobs">subly<span>AI</span></a><a class="back" routerLink="/jobs">{{ i18n.t('back') }}</a></header>@if (job()) { <section class="title"><p class="eyebrow">{{ job()!.status === 'Done' ? i18n.t('ready') : statusText() }}</p><h1>{{ job()!.fileName }}</h1><p>{{ i18n.formatDate(job()!.createdAt) }} · {{ job()!.language || i18n.t('languageNotDetected') }}</p></section>@if (job()!.status === 'Done') { <div class="downloads"><a [href]="jobs.srtUrl(job()!.id)">{{ i18n.t('downloadSrt') }}</a><a [href]="jobs.vttUrl(job()!.id)">{{ i18n.t('downloadVtt') }}</a></div><section class="content">@if (summary()) { <article class="summary"><p class="eyebrow">{{ i18n.t('summary') }}</p><h2>{{ i18n.t('about') }}</h2><p>{{ summary()!.text }}</p></article> }<article class="transcript"><div class="article-head"><div><p class="eyebrow">{{ i18n.t('fullText') }}</p><h2>{{ i18n.t('transcript') }}</h2></div><button type="button" (click)="copy()">{{ i18n.t(copied() ? 'copied' : 'copy') }}</button></div><p class="text">{{ transcript() || i18n.t('loadingTranscript') }}</p></article></section> } @else { <section class="processing"><span class="spinner">◌</span><h2>{{ statusText() }}</h2><p>{{ i18n.t('processingNote') }}</p></section> } } @else { <p class="loading">{{ i18n.t('loading') }}</p> }</main>`
})
export class JobDetailComponent implements OnInit, OnDestroy {
  readonly route = inject(ActivatedRoute);
  readonly jobs = inject(JobsService);
  readonly i18n = inject(I18nService);
  readonly job = signal<Job | null>(null);
  readonly transcript = signal('');
  readonly summary = signal<Summary | null>(null);
  readonly copied = signal(false);
  private timer?: number;

  ngOnInit() { this.load(); }
  ngOnDestroy() { window.clearTimeout(this.timer); }

  load() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.jobs.get(id).subscribe({
      next: job => {
        this.job.set(job);
        if (job.status === 'Done') {
          this.jobs.transcript(id).subscribe(result => this.transcript.set(result.text));
          this.jobs.summary(id).subscribe({ next: result => this.summary.set(result), error: () => {} });
        } else if (job.status !== 'Failed') {
          this.timer = window.setTimeout(() => this.load(), 3000);
        }
      }
    });
  }

  statusText() {
    return this.i18n.t({ Pending: 'pending', Processing: 'transcribing', Failed: 'failedProcess' }[this.job()?.status ?? ''] ?? 'processing');
  }

  async copy() {
    await navigator.clipboard.writeText(this.transcript());
    this.copied.set(true);
    window.setTimeout(() => this.copied.set(false), 1600);
  }
}
