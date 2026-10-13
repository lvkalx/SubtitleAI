import { Injectable, signal } from '@angular/core';

type Language = 'en' | 'ru';
type Dictionary = Record<string, string>;

const en: Dictionary = {
  brandTag: 'Speech into text', homeHow: 'How it works', signIn: 'Sign in', startFree: 'Start for free →',
  homeTitle: 'Every spoken phrase\nmatters.', homeLead: 'Upload meetings, interviews, and video. Subly AI turns them into subtitles, text, and a concise summary.',
  createTranscript: 'Create a transcript', learnMore: 'Learn more', step1: 'Upload a recording', step1d: 'Audio and video files up to 500 MB are supported.',
  step2: 'Get the text', step2d: 'The service recognizes speech and creates subtitles.', step3: 'Download the result', step3d: 'Text, SRT, and VTT are always at hand.',
  authHeroTag: 'Subtitles in minutes', authHeroTitle: 'Let every word <em>carry</em> further.', authHeroLead: 'Upload recordings, get clear subtitles, and a short summary of what matters.', authQuote: '“I turn speech into text you will want to read.”',
  newAccount: 'New account', welcomeBack: 'Welcome back', letsStart: 'Ready to begin?', workspaceLogin: 'Sign in to your workspace', createAccountHint: 'Create an account — it takes less than a minute.',
  continueHint: 'Continue working with your transcripts.', email: 'Email', password: 'Password', confirmPassword: 'Confirm password', minPassword: 'At least 8 characters', repeatPassword: 'Repeat password',
  show: 'Show', hide: 'Hide', creating: 'Please wait…', createAccount: 'Create account', login: 'Sign in', haveAccount: 'Already have an account? Sign in', firstTime: 'New here? Create an account',
  invalidCredentials: 'Enter a valid email and a password of at least 8 characters.', mismatch: 'Passwords do not match.', authFailed: 'Could not sign in. Check your details and try again.',
  workspace: 'Workspace', welcome: 'Welcome.', whatTranscribe: 'What shall we transcribe?', logout: 'Sign out',
  dropFile: 'Drop a file here or choose one from your computer', uploading: 'Uploading file…', formats: 'MP3, WAV, M4A, MP4, MKV, MOV, WEBM, AVI · up to 500 MB',
  recordings: 'Your recordings', emptyTitle: 'Your recordings will appear here', emptyText: 'Upload your first audio or video — we will handle the rest.',
  queued: 'Queued', processing: 'Processing', done: 'Done', failed: 'Failed', detectingLanguage: 'Detecting language', uploadFailed: 'Could not upload the file. Check its format and size (up to 500 MB).', jobsLoadFailed: 'Could not load your recordings. Please try again.',
  previous: '← Previous', next: 'Next →', historyPages: 'Recording history pages',
  back: '← All recordings', ready: 'Transcript ready', languageNotDetected: 'Language not detected', downloadSrt: '↓ Download .SRT', downloadVtt: '↓ Download .VTT', summary: 'Summary', about: 'What this recording is about',
  listen: 'Listen', stop: 'Stop', speed: 'Speed', fullText: 'Full text', transcript: 'Transcript', copy: 'Copy', copied: 'Copied', loading: 'Loading…', loadingTranscript: 'Loading transcript…',
  pending: 'Your file is queued', transcribing: 'Transcribing your recording', processingNote: 'This may take a few minutes. This page will update automatically when the result is ready.', failedProcess: 'Could not process the file.'
};

const ru: Dictionary = {
  brandTag: 'Аудио становится текстом', homeHow: 'Как это работает', signIn: 'Войти', startFree: 'Начать бесплатно →',
  homeTitle: 'Смысл каждой\nсказанной фразы.', homeLead: 'Загружайте встречи, интервью и видео. Subly AI превратит их в субтитры, текст и краткое резюме.',
  createTranscript: 'Создать расшифровку', learnMore: 'Узнать больше', step1: 'Загрузите запись', step1d: 'Поддерживаются аудио и видео до 500 МБ.',
  step2: 'Получите текст', step2d: 'Сервис распознает речь и соберёт субтитры.', step3: 'Скачайте результат', step3d: 'Текст, SRT и VTT будут всегда под рукой.',
  authHeroTag: 'Субтитры за минуты', authHeroTitle: 'Пусть каждое слово <em>звучит</em> дальше.', authHeroLead: 'Загружайте записи, получайте аккуратные субтитры и короткую выжимку смысла.', authQuote: '«Превращаю голос в текст, который хочется читать.»',
  newAccount: 'Новый аккаунт', welcomeBack: 'С возвращением', letsStart: 'Начнём?', workspaceLogin: 'Войти в рабочее пространство', createAccountHint: 'Создайте аккаунт — это займёт меньше минуты.',
  continueHint: 'Продолжайте работать с вашими расшифровками.', email: 'Email', password: 'Пароль', confirmPassword: 'Подтвердите пароль', minPassword: 'Минимум 8 символов', repeatPassword: 'Повторите пароль',
  show: 'Показать', hide: 'Скрыть', creating: 'Подождите…', createAccount: 'Создать аккаунт', login: 'Войти', haveAccount: 'Уже есть аккаунт? Войти', firstTime: 'Впервые здесь? Создать аккаунт',
  invalidCredentials: 'Укажите корректный email и пароль от 8 символов.', mismatch: 'Пароли не совпадают.', authFailed: 'Не удалось авторизоваться. Проверьте данные и повторите.',
  workspace: 'Рабочее пространство', welcome: 'Добро пожаловать.', whatTranscribe: 'Что расшифруем?', logout: 'Выйти',
  dropFile: 'Перетащите файл сюда или выберите на компьютере', uploading: 'Загружаем файл…', formats: 'MP3, WAV, M4A, MP4, MKV, MOV, WEBM, AVI · до 500 МБ',
  recordings: 'Ваши записи', emptyTitle: 'Здесь появятся ваши записи', emptyText: 'Загрузите первое аудио или видео — мы сделаем остальное.',
  queued: 'В очереди', processing: 'В работе', done: 'Готово', failed: 'Ошибка', detectingLanguage: 'Определяем язык', uploadFailed: 'Не удалось загрузить файл. Проверьте формат и размер (до 500 МБ).', jobsLoadFailed: 'Не удалось загрузить записи. Попробуйте ещё раз.',
  previous: '← Назад', next: 'Вперёд →', historyPages: 'Страницы истории записей',
  back: '← Все записи', ready: 'Расшифровка готова', languageNotDetected: 'Язык не определён', downloadSrt: '↓ Скачать .SRT', downloadVtt: '↓ Скачать .VTT', summary: 'Кратко', about: 'О чём эта запись',
  listen: 'Слушать', stop: 'Стоп', speed: 'Скорость', fullText: 'Полный текст', transcript: 'Расшифровка', copy: 'Копировать', copied: 'Скопировано', loading: 'Загружаем…', loadingTranscript: 'Загружаем расшифровку…',
  pending: 'Файл ждёт обработки', transcribing: 'Расшифровываем запись', processingNote: 'Это может занять несколько минут. Страница обновится автоматически, когда результат будет готов.', failedProcess: 'Не удалось обработать файл.'
};

@Injectable({ providedIn: 'root' })
export class I18nService {
  readonly language = signal<Language>(typeof navigator !== 'undefined' && navigator.language.toLowerCase().startsWith('ru') ? 'ru' : 'en');

  t(key: string) {
    return (this.language() === 'ru' ? ru : en)[key] ?? en[key] ?? key;
  }

  status(value: string) {
    return this.t({ Pending: 'queued', Processing: 'processing', Done: 'done', Failed: 'failed' }[value] ?? value);
  }

  recordingsCount(count: number) {
    if (this.language() !== 'ru') return `${count} ${count === 1 ? 'file' : 'files'} in your library`;
    const lastTwo = count % 100;
    const last = count % 10;
    const form = lastTwo >= 11 && lastTwo <= 14 ? 'файлов' : last === 1 ? 'файл' : last >= 2 && last <= 4 ? 'файла' : 'файлов';
    return `${count} ${form} в библиотеке`;
  }

  formatDate(value: string) {
    return new Intl.DateTimeFormat(this.language() === 'ru' ? 'ru-RU' : 'en-US', { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' }).format(new Date(value));
  }

  speechLocale(contentLanguage?: string | null) {
    if (contentLanguage?.toLowerCase().startsWith('ru')) return 'ru-RU';
    if (contentLanguage?.toLowerCase().startsWith('uk')) return 'uk-UA';
    if (contentLanguage?.toLowerCase().startsWith('en')) return 'en-US';
    return this.language() === 'ru' ? 'ru-RU' : 'en-US';
  }
}
