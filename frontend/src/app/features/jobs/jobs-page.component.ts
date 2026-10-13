import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit, computed, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { JobsService } from '../../core/api/jobs.service';
import { AuthService } from '../../core/api/auth.service';
import { Job } from '../../core/api/models';
import { I18nService } from '../../core/i18n.service';

@Component({
  selector: 'app-jobs-page', imports: [RouterLink, ReactiveFormsModule], styleUrl: './jobs-page.component.scss',
  template: `<main class="dashboard"><header><a class="brand" routerLink="/jobs">subly<span>AI</span></a><div class="header-actions">@if (auth.isAdmin()) { <a class="admin-link" routerLink="/admin">{{ i18n.t('admin') }}</a> }<button class="logout" type="button" (click)="logout()">{{ i18n.t('logout') }}</button></div></header><section class="hero"><div><p class="eyebrow">{{ i18n.t('workspace') }}</p><h1>{{ i18n.t('welcome') }}<br><em>{{ i18n.t('whatTranscribe') }}</em></h1></div></section><label class="upload" [class.busy]="uploading()"><input type="file" accept="audio/*,video/*,.mkv,.avi" (change)="upload($event)" [disabled]="uploading()"><span class="upload-icon" [class.spinner]="uploading()">{{ uploading() ? '◌' : '↑' }}</span><strong>{{ i18n.t(uploading() ? 'uploading' : 'dropFile') }}</strong><small>{{ i18n.t('formats') }}</small></label><form class="url-import" (submit)="importUrl($event)" novalidate><label>{{ i18n.t('orFromLink') }}<input [formControl]="sourceUrl" type="url" [placeholder]="i18n.t('mediaLinkPlaceholder')"></label><button type="submit" [disabled]="importing()">{{ i18n.t(importing() ? 'downloadingFromLink' : 'transcribeLink') }}</button><small>{{ i18n.t('directMediaOnly') }}</small></form>@if (error()) { <p class="error" role="alert">{{ i18n.t(error()) }}</p> }<section class="library"><div class="library-head"><div><h2>{{ i18n.t('recordings') }}</h2><p>{{ i18n.recordingsCount(totalCount()) }}</p></div></div>@if (jobs().length) { <div class="jobs">@for (job of jobs(); track job.id) { <a class="job" [routerLink]="['/jobs', job.id]"><span class="file-icon">{{ iconFor(job.fileName) }}</span><span class="job-name"><strong>{{ job.fileName }}</strong><small>{{ i18n.formatDate(job.createdAt) }} · {{ job.language || i18n.t('detectingLanguage') }}</small></span><span class="status" [class.done]="job.status === 'Done'" [class.failed]="job.status === 'Failed'">{{ i18n.status(job.status) }}</span><span class="arrow">→</span></a> }</div> } @else { <div class="empty"><span>✦</span><h3>{{ i18n.t('emptyTitle') }}</h3><p>{{ i18n.t('emptyText') }}</p></div> }</section>@if (totalPages() > 1) { <nav [attr.aria-label]="i18n.t('historyPages')"><button type="button" [disabled]="page() === 1" (click)="goTo(page() - 1)">{{ i18n.t('previous') }}</button><span>{{ page() }} / {{ totalPages() }}</span><button type="button" [disabled]="page() === totalPages()" (click)="goTo(page() + 1)">{{ i18n.t('next') }}</button></nav>}</main>`
})
export class JobsPageComponent implements OnInit, OnDestroy {
  readonly jobs = signal<Job[]>([]);
  readonly error = signal('');
  readonly page = signal(1);
  readonly totalCount = signal(0);
  readonly uploading = signal(false);
  readonly importing = signal(false);
  readonly sourceUrl = new FormControl('', [Validators.required, Validators.pattern(/^https?:\/\//i)]);
  readonly pageSize = 20;
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));
  private refreshTimer?: number;

  constructor(private jobsService: JobsService, readonly i18n: I18nService, public readonly auth: AuthService) {}

  ngOnInit() { this.load(); }
  ngOnDestroy() { window.clearTimeout(this.refreshTimer); }

  load() {
    window.clearTimeout(this.refreshTimer);
    this.error.set('');
    this.jobsService.list(this.page(), this.pageSize).subscribe({
      next: result => {
        this.jobs.set(result.items);
        this.totalCount.set(result.totalCount);
        if (result.items.some(job => job.status === 'Pending' || job.status === 'Processing')) {
          this.refreshTimer = window.setTimeout(() => this.load(), 3000);
        }
      },
      error: () => this.error.set('jobsLoadFailed')
    });
  }

  goTo(page: number) { if (page >= 1 && page <= this.totalPages()) { this.page.set(page); this.load(); } }
  logout() { sessionStorage.removeItem('subtitleai.accessToken'); location.assign('/auth'); }
  iconFor(name: string) { return /\.(mp4|mkv|mov|webm|avi)$/i.test(name) ? '▶' : '♫'; }

  upload(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.uploading.set(true);
    this.error.set('');
    this.jobsService.upload(file).subscribe({
      next: () => { input.value = ''; this.page.set(1); this.uploading.set(false); this.load(); },
      error: () => { this.uploading.set(false); this.error.set('uploadFailed'); }
    });
  }

  importUrl(event: SubmitEvent) {
    event.preventDefault();
    if (this.sourceUrl.invalid) { this.error.set('invalidMediaUrl'); return; }
    this.importing.set(true); this.error.set('');
    this.jobsService.importUrl(this.sourceUrl.value!).subscribe({
      next: () => { this.sourceUrl.reset(); this.page.set(1); this.importing.set(false); this.load(); },
      error: error => { this.importing.set(false); this.error.set(this.errorKey(error)); }
    });
  }

  private errorKey(error: unknown) {
    const response = error as HttpErrorResponse;
    const errors = response.error?.errors;
    if (Array.isArray(errors) && typeof errors[0] === 'string') return errors[0];
    if (response.status === 0) return 'networkError';
    return 'mediaUrlUnavailable';
  }
}
