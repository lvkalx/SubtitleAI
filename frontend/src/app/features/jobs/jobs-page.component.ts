import { Component, OnInit, computed, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { JobsService } from '../../core/api/jobs.service';
import { Job } from '../../core/api/models';
@Component({ selector: 'app-jobs-page', imports: [DatePipe], template: `<main><header><div><h1>Мои расшифровки</h1><p>Аудио и видео превращаются в субтитры и краткое AI-резюме.</p></div><button type="button" (click)="load()">Обновить</button></header><label class="upload">Загрузить аудио или видео <input type="file" accept="audio/*,video/*" (change)="upload($event)"></label>@if (error()) { <p class="error" role="alert">{{ error() }}</p> }<ul>@for (job of jobs(); track job.id) { <li><strong>{{ job.fileName }}</strong><span>{{ job.status }}</span><small>{{ job.createdAt | date:"short" }}</small></li>} @empty { <li>Задач пока нет.</li> }</ul>@if (totalPages() > 1) { <nav aria-label="Страницы истории"><button type="button" [disabled]="page() === 1" (click)="goTo(page() - 1)">Назад</button><span>Страница {{ page() }} из {{ totalPages() }}</span><button type="button" [disabled]="page() === totalPages()" (click)="goTo(page() + 1)">Вперёд</button></nav>}</main>` })
export class JobsPageComponent implements OnInit {
  readonly jobs = signal<Job[]>([]); readonly error = signal(''); readonly page = signal(1); readonly totalCount = signal(0); readonly pageSize = 20;
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));
  constructor(private jobsService: JobsService) {}
  ngOnInit() { this.load(); }
  load() { this.error.set(''); this.jobsService.list(this.page(), this.pageSize).subscribe({ next: x => { this.jobs.set(x.items); this.totalCount.set(x.totalCount); }, error: () => this.error.set('Не удалось загрузить задачи.') }); }
  goTo(page: number) { if (page >= 1 && page <= this.totalPages()) { this.page.set(page); this.load(); } }
  upload(event: Event) { const input = event.target as HTMLInputElement; const file = input.files?.[0]; if (!file) return; this.jobsService.upload(file).subscribe({ next: () => { input.value = ''; this.page.set(1); this.load(); }, error: () => this.error.set('Не удалось загрузить файл.') }); }
}
