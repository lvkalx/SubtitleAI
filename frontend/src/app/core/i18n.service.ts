import { Injectable, signal } from '@angular/core';

type Language = 'en' | 'ru';
type Dictionary = Record<string, string>;

const en: Dictionary = {
  brandTag: 'Speech into text', homeHow: 'How it works', signIn: 'Sign in', startFree: 'Start for free →',
  homeTitle: 'Every spoken phrase\nmatters.', homeLead: 'Upload meetings, interviews, and video. Subly AI turns them into subtitles, text, and a concise summary.',
  createTranscript: 'Create a transcript', learnMore: 'Learn more', step1: 'Upload a recording', step1d: 'Audio and video files up to 500 MB are supported.',
  step2: 'Get the text', step2d: 'The service recognizes speech and creates subtitles.', step3: 'Download the result', step3d: 'Text, SRT, and VTT are always at hand.',
  newAccount: 'New account', welcomeBack: 'Welcome back', letsStart: 'Ready to begin?', workspaceLogin: 'Sign in to your workspace', createAccountHint: 'Create an account — it takes less than a minute.',
  continueHint: 'Continue working with your transcripts.', email: 'Email', password: 'Password', confirmPassword: 'Confirm password', minPassword: 'At least 8 characters', repeatPassword: 'Repeat password',
  show: 'Show', hide: 'Hide', creating: 'Please wait…', createAccount: 'Create account', login: 'Sign in', haveAccount: 'Already have an account? Sign in', firstTime: 'New here? Create an account',
  invalidCredentials: 'Enter a valid email and a password of at least 8 characters.', mismatch: 'Passwords do not match.', authFailed: 'Could not sign in. Check your details and try again.',
  workspace: 'Workspace', welcome: 'Welcome.', whatTranscribe: 'What shall we transcribe?', refresh: '↻ Refresh', logout: 'Sign out',
  dropFile: 'Drop a file here or choose one from your computer', uploading: 'Uploading file…', formats: 'MP3, WAV, M4A, MP4, MKV, MOV, WEBM, AVI · up to 500 MB',
  recordings: 'Your recordings', inLibrary: 'files in your library', emptyTitle: 'Your recordings will appear here', emptyText: 'Upload your first audio or video — we will handle the rest.',
  queued: 'Queued', processing: 'Processing', done: 'Done', failed: 'Failed', detectingLanguage: 'Detecting language', uploadFailed: 'Could not upload the file. Check its format and size (up to 500 MB).',
  back: '← All recordings', ready: 'Transcript ready', downloadSrt: '↓ Download .SRT', downloadVtt: '↓ Download .VTT', summary: 'Summary', about: 'What this recording is about', fullText: 'Full text', transcript: 'Transcript', copy: 'Copy', copied: 'Copied', loading: 'Loading…', pending: 'File is waiting to be processed', transcribing: 'Transcribing your recording', processingNote: 'This page will update automatically when the result is ready.', failedProcess: 'Could not process the file.'
};

const ru: Dictionary = {
  brandTag: 'Аудио становится текстом', homeHow: 'Как это работает', signIn: 'Войти', startFree: 'Начать бесплатно →',
  homeTitle: 'Смысл каждой\nсказанной фразы.', homeLead: 'Загружайте встречи, интервью и видео. Subly AI превратит их в субтитры, текст и краткое резюме.',
  createTranscript: 'Создать расшифровку', learnMore: 'Узнать больше', step1: 'Загрузите запись', step1d: 'Поддерживаются аудио и видео до 500 МБ.',
  step2: 'Получите текст', step2d: 'Сервис распознает речь и соберёт субтитры.', step3: 'Скачайте результат', step3d: 'Текст, SRT и VTT будут всегда под рукой.',
  newAccount: 'Новый аккаунт', welcomeBack: 'С возвращением', letsStart: 'Начнём?', workspaceLogin: 'Войти в рабочее пространство', createAccountHint: 'Создайте аккаунт — это займёт меньше минуты.',
  continueHint: 'Продолжайте работать с вашими расшифровками.', email: 'Email', password: 'Пароль', confirmPassword: 'Подтвердите пароль', minPassword: 'Минимум 8 символов', repeatPassword: 'Повторите пароль',
  show: 'Показать', hide: 'Скрыть', creating: 'Подождите…', createAccount: 'Создать аккаунт', login: 'Войти', haveAccount: 'Уже есть аккаунт? Войти', firstTime: 'Впервые здесь? Создать аккаунт',
  invalidCredentials: 'Укажите корректный email и пароль от 8 символов.', mismatch: 'Пароли не совпадают.', authFailed: 'Не удалось авторизоваться. Проверьте данные и повторите.',
  workspace: 'Рабочее пространство', welcome: 'Добро пожаловать.', whatTranscribe: 'Что расшифруем?', refresh: '↻ Обновить', logout: 'Выйти',
  dropFile: 'Перетащите файл сюда или выберите на компьютере', uploading: 'Загружаем файл…', formats: 'MP3, WAV, M4A, MP4, MKV, MOV, WEBM, AVI · до 500 МБ',
  recordings: 'Ваши записи', inLibrary: 'файлов в библиотеке', emptyTitle: 'Здесь появятся ваши записи', emptyText: 'Загрузите первое аудио или видео — мы сделаем остальное.',
  queued: 'В очереди', processing: 'В работе', done: 'Готово', failed: 'Ошибка', detectingLanguage: 'Определяем язык', uploadFailed: 'Не удалось загрузить файл. Проверьте формат и размер (до 500 МБ).',
  back: '← Все записи', ready: 'Расшифровка готова', downloadSrt: '↓ Скачать .SRT', downloadVtt: '↓ Скачать .VTT', summary: 'Кратко', about: 'О чём эта запись', fullText: 'Полный текст', transcript: 'Расшифровка', copy: 'Копировать', copied: 'Скопировано', loading: 'Загружаем…', pending: 'Файл ждёт обработки', transcribing: 'Расшифровываем запись', processingNote: 'Страница обновится автоматически, когда результат будет готов.', failedProcess: 'Не удалось обработать файл.'
};

@Injectable({ providedIn: 'root' })
export class I18nService {
  readonly language = signal<Language>(typeof navigator !== 'undefined' && navigator.language.toLowerCase().startsWith('ru') ? 'ru' : 'en');
  t(key: string) { return (this.language() === 'ru' ? ru : en)[key] ?? en[key] ?? key; }

  localizePage(root: Node) {
    if (this.language() === 'ru') return;
    const translations: Record<string, string> = {
      'Как это работает': 'How it works', 'Войти': 'Sign in', 'Начать бесплатно →': 'Start for free →',
      'Аудио становится текстом': 'Speech into text', 'Смысл каждой': 'Every spoken phrase', 'сказанной фразы.': 'matters.',
      'Загружайте встречи, интервью и видео. Subly AI превратит их в субтитры, текст и краткое резюме.': en['homeLead'],
      'Создать расшифровку': 'Create a transcript', 'Узнать больше': 'Learn more', 'Загрузите запись': 'Upload a recording',
      'Поддерживаются аудио и видео до 500 МБ.': 'Audio and video files up to 500 MB are supported.', 'Получите текст': 'Get the text',
      'Сервис распознает речь и соберёт субтитры.': 'The service recognizes speech and creates subtitles.', 'Скачайте результат': 'Download the result',
      'Текст, SRT и VTT будут всегда под рукой.': 'Text, SRT, and VTT are always at hand.', 'Новый аккаунт': 'New account', 'С возвращением': 'Welcome back',
      'Начнём?': 'Ready to begin?', 'Войти в рабочее пространство': 'Sign in to your workspace', 'Создайте аккаунт — это займёт меньше минуты.': 'Create an account — it takes less than a minute.',
      'Продолжайте работать с вашими расшифровками.': 'Continue working with your transcripts.', 'Пароль': 'Password', 'Подтвердите пароль': 'Confirm password',
      'Минимум 8 символов': 'At least 8 characters', 'Повторите пароль': 'Repeat password', 'Показать': 'Show', 'Скрыть': 'Hide',
      'Создать аккаунт': 'Create account', 'Уже есть аккаунт? Войти': 'Already have an account? Sign in', 'Впервые здесь? Создать аккаунт': 'New here? Create an account',
      'Рабочее пространство': 'Workspace', 'Добро пожаловать.': 'Welcome.', 'Что расшифруем?': 'What shall we transcribe?', 'Выйти': 'Sign out', 'Обновить': 'Refresh',
      'Перетащите файл сюда или выберите на компьютере': 'Drop a file here or choose one from your computer', 'Загружаем файл…': 'Uploading file…',
      'Ваши записи': 'Your recordings', 'Здесь появятся ваши записи': 'Your recordings will appear here', 'Загрузите первое аудио или видео — мы сделаем остальное.': 'Upload your first audio or video — we will handle the rest.',
      'В очереди': 'Queued', 'В работе': 'Processing', 'Готово': 'Done', 'Ошибка': 'Failed', 'Определяем язык': 'Detecting language',
      'Не удалось загрузить файл. Проверьте формат и размер (до 500 МБ).': 'Could not upload the file. Check its format and size (up to 500 MB).'
    };
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    const nodes: Text[] = []; let node: Node | null;
    while ((node = walker.nextNode())) nodes.push(node as Text);
    for (const text of nodes) { const value = text.nodeValue?.trim(); if (value && translations[value]) text.nodeValue = text.nodeValue!.replace(value, translations[value]); }
  }
}
