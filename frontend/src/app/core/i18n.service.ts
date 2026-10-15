import { Injectable, signal } from '@angular/core';

type Language = 'en' | 'ru';
type Dictionary = Record<string, string>;

const en: Dictionary = {
  brandTag: 'Speech into text', homeHow: 'How it works', signIn: 'Sign in', startFree: 'Start for free →', freeForever: '100% free to use',
  homeTitle: 'Every spoken phrase\nmatters.', homeLead: 'Upload meetings, interviews, and video. Subly AI turns them into subtitles, text, and a concise summary.',
  createTranscript: 'Create a transcript', learnMore: 'Learn more', step1: 'Upload a recording', step1d: 'Audio and video files up to 500 MB are supported.',
  step2: 'Get the text', step2d: 'The service recognizes speech and creates subtitles.', step3: 'Download the result', step3d: 'Text, SRT, and VTT are always at hand.',
  authHeroTag: 'Subtitles in minutes', authHeroTitle: 'Let every word <em>carry</em> further.', authHeroLead: 'Upload recordings, get clear subtitles, and a short summary of what matters.', authQuote: '“I turn speech into text you will want to read.”',
  newAccount: 'New account', welcomeBack: 'Welcome back', letsStart: 'Ready to begin?', workspaceLogin: 'Sign in to your workspace', createAccountHint: 'Create an account — it takes less than a minute.',
  continueHint: 'Continue working with your transcripts.', email: 'Email', password: 'Password', confirmPassword: 'Confirm password', minPassword: 'At least 8 characters', repeatPassword: 'Repeat password',
  show: 'Show', hide: 'Hide', creating: 'Creating account…', signingIn: 'Signing in…', createAccount: 'Create account', login: 'Sign in', haveAccount: 'Already have an account? Sign in', firstTime: 'New here? Create an account', forgotPassword: 'Forgot password?',
  emailRequired: 'Enter your email address.', invalidEmail: 'Enter a valid email address.', passwordRequired: 'Enter your password.', passwordTooShort: 'Your password must be at least 8 characters.', passwordNotStrongEnough: 'Choose a stronger password.', confirmPasswordRequired: 'Confirm your password.', mismatch: 'Passwords do not match.', emailAlreadyRegistered: 'An account with this email already exists. Sign in instead.', invalidEmailOrPassword: 'Email or password is incorrect.', registrationCouldNotBeCompleted: 'We could not create your account. Please try again.', networkError: 'Could not reach the service. Check your connection and try again.', tooManyAttempts: 'Too many attempts. Please wait a moment and try again.', serverError: 'The service is temporarily unavailable. Please try again shortly.', sessionExpired: 'Your session has expired. Please sign in again.', passwordRecoveryNeedsEmail: 'Password recovery will be available after an email sender is connected.',
  passwordRecovery: 'Password recovery', resetPasswordTitle: 'Reset your password', resetPasswordLead: 'Enter your email and we will send a secure reset link.', sendResetLink: 'Send reset link', sendingEmail: 'Sending email…', passwordResetEmailSent: 'If this email belongs to an account, a reset link is on its way.', emailDeliveryUnavailable: 'Email delivery is not configured yet. Please try again later.', chooseNewPassword: 'Choose a new password', newPasswordLead: 'Use at least 8 characters. Your other signed-in sessions will be closed.', saveNewPassword: 'Save new password', savingPassword: 'Saving password…', passwordChanged: 'Password changed. You can now sign in.', invalidOrExpiredPasswordReset: 'This password-reset link is invalid or has expired. Request a new one.', backToSignIn: '← Back to sign in', emailConfirmation: 'Email confirmation', confirmingEmail: 'Confirming your email…', emailConfirmed: 'Your email is confirmed. You can now sign in.', invalidOrExpiredEmailConfirmation: 'This confirmation link is invalid or has expired.', emailConfirmationRequired: 'Your account was created. Check your email to confirm it before signing in.', emailNotConfirmed: 'Confirm your email before signing in.',
  workspace: 'Workspace', welcome: 'Welcome.', whatTranscribe: 'What shall we transcribe?', logout: 'Sign out', admin: 'Admin', analyticsTitle: 'Product analytics', analyticsLead: 'A private overview of sublyAI usage.', usersTotal: 'Registered users', newUsers30: 'New in 30 days', transcriptionsTotal: 'Transcriptions', completed: 'Completed', inProgress: 'In progress', mediaMinutes: 'Media minutes', transcripts: 'Saved transcripts', last7Days: 'Last 7 days', activityLead: 'New transcription requests per day.', adminAccessDenied: 'You do not have access to this page.',
  dropFile: 'Drop a file here or choose one from your computer', uploading: 'Uploading file…', formats: 'MP3, WAV, M4A, MP4, MKV, MOV, WEBM, AVI · up to 500 MB',
  recordings: 'Your recordings', emptyTitle: 'Your recordings will appear here', emptyText: 'Upload your first audio or video — we will handle the rest.',
  queued: 'Queued', processing: 'Processing', done: 'Done', failed: 'Failed', detectingLanguage: 'Detecting language', uploadFailed: 'Could not upload the file. Check its format and size (up to 500 MB).', jobsLoadFailed: 'Could not load your recordings. Please try again.',
  previous: '← Previous', next: 'Next →', historyPages: 'Recording history pages',
  back: '← All recordings', ready: 'Transcript ready', languageNotDetected: 'Language not detected', downloadSrt: '↓ Download .SRT', downloadVtt: '↓ Download .VTT', summary: 'Summary', about: 'What this recording is about',
  fullText: 'Full text', transcript: 'Transcript', copy: 'Copy', copied: 'Copied', loading: 'Loading…', loadingTranscript: 'Loading transcript…',
  pending: 'Your file is queued', transcribing: 'Transcribing your recording', processingNote: 'This can take a while for long recordings. You can safely leave this page — the file will keep processing and be ready in your library.', failedProcess: 'Could not process the file.'
};

const ru: Dictionary = {
  brandTag: 'Аудио становится текстом', homeHow: 'Как это работает', signIn: 'Войти', startFree: 'Начать бесплатно →', freeForever: 'Полностью бесплатно',
  homeTitle: 'Смысл каждой\nсказанной фразы.', homeLead: 'Загружайте встречи, интервью и видео. Subly AI превратит их в субтитры, текст и краткое резюме.',
  createTranscript: 'Создать расшифровку', learnMore: 'Узнать больше', step1: 'Загрузите запись', step1d: 'Поддерживаются аудио и видео до 500 МБ.',
  step2: 'Получите текст', step2d: 'Сервис распознает речь и соберёт субтитры.', step3: 'Скачайте результат', step3d: 'Текст, SRT и VTT будут всегда под рукой.',
  authHeroTag: 'Субтитры за минуты', authHeroTitle: 'Пусть каждое слово <em>звучит</em> дальше.', authHeroLead: 'Загружайте записи, получайте аккуратные субтитры и короткую выжимку смысла.', authQuote: '«Превращаю голос в текст, который хочется читать.»',
  newAccount: 'Новый аккаунт', welcomeBack: 'С возвращением', letsStart: 'Начнём?', workspaceLogin: 'Войти в рабочее пространство', createAccountHint: 'Создайте аккаунт — это займёт меньше минуты.',
  continueHint: 'Продолжайте работать с вашими расшифровками.', email: 'Email', password: 'Пароль', confirmPassword: 'Подтвердите пароль', minPassword: 'Минимум 8 символов', repeatPassword: 'Повторите пароль',
  show: 'Показать', hide: 'Скрыть', creating: 'Создаём аккаунт…', signingIn: 'Входим…', createAccount: 'Создать аккаунт', login: 'Войти', haveAccount: 'Уже есть аккаунт? Войти', firstTime: 'Впервые здесь? Создать аккаунт', forgotPassword: 'Забыли пароль?',
  emailRequired: 'Укажите адрес email.', invalidEmail: 'Укажите корректный адрес email.', passwordRequired: 'Введите пароль.', passwordTooShort: 'Пароль должен содержать не менее 8 символов.', passwordNotStrongEnough: 'Выберите более надёжный пароль.', confirmPasswordRequired: 'Подтвердите пароль.', mismatch: 'Пароли не совпадают.', emailAlreadyRegistered: 'Аккаунт с этим email уже существует. Войдите в него.', invalidEmailOrPassword: 'Неверный email или пароль.', registrationCouldNotBeCompleted: 'Не удалось создать аккаунт. Попробуйте ещё раз.', networkError: 'Не удалось связаться с сервисом. Проверьте интернет и повторите.', tooManyAttempts: 'Слишком много попыток. Подождите немного и повторите.', serverError: 'Сервис временно недоступен. Попробуйте чуть позже.', sessionExpired: 'Сессия истекла. Войдите снова.', passwordRecoveryNeedsEmail: 'Восстановление пароля станет доступно после подключения почтового отправителя.',
  passwordRecovery: 'Восстановление пароля', resetPasswordTitle: 'Сбросить пароль', resetPasswordLead: 'Укажите email — мы отправим безопасную ссылку для сброса.', sendResetLink: 'Отправить ссылку', sendingEmail: 'Отправляем письмо…', passwordResetEmailSent: 'Если аккаунт с таким email существует, ссылка для сброса уже отправлена.', emailDeliveryUnavailable: 'Отправка писем ещё не настроена. Попробуйте позже.', chooseNewPassword: 'Придумайте новый пароль', newPasswordLead: 'Используйте не менее 8 символов. Все остальные сессии будут завершены.', saveNewPassword: 'Сохранить новый пароль', savingPassword: 'Сохраняем пароль…', passwordChanged: 'Пароль изменён. Теперь можно войти.', invalidOrExpiredPasswordReset: 'Эта ссылка для сброса пароля недействительна или устарела. Запросите новую.', backToSignIn: '← Вернуться ко входу', emailConfirmation: 'Подтверждение email', confirmingEmail: 'Подтверждаем email…', emailConfirmed: 'Email подтверждён. Теперь можно войти.', invalidOrExpiredEmailConfirmation: 'Эта ссылка подтверждения недействительна или устарела.', emailConfirmationRequired: 'Аккаунт создан. Подтвердите email из письма перед входом.', emailNotConfirmed: 'Подтвердите email перед входом.',
  workspace: 'Рабочее пространство', welcome: 'Добро пожаловать.', whatTranscribe: 'Что расшифруем?', logout: 'Выйти', admin: 'Админка', analyticsTitle: 'Аналитика продукта', analyticsLead: 'Приватная сводка использования sublyAI.', usersTotal: 'Зарегистрировано', newUsers30: 'Новых за 30 дней', transcriptionsTotal: 'Расшифровок', completed: 'Готово', inProgress: 'В обработке', mediaMinutes: 'Минут медиа', transcripts: 'Сохранённых текстов', last7Days: 'Последние 7 дней', activityLead: 'Новые запросы на расшифровку по дням.', adminAccessDenied: 'У вас нет доступа к этой странице.',
  dropFile: 'Перетащите файл сюда или выберите на компьютере', uploading: 'Загружаем файл…', formats: 'MP3, WAV, M4A, MP4, MKV, MOV, WEBM, AVI · до 500 МБ',
  recordings: 'Ваши записи', emptyTitle: 'Здесь появятся ваши записи', emptyText: 'Загрузите первое аудио или видео — мы сделаем остальное.',
  queued: 'В очереди', processing: 'В работе', done: 'Готово', failed: 'Ошибка', detectingLanguage: 'Определяем язык', uploadFailed: 'Не удалось загрузить файл. Проверьте формат и размер (до 500 МБ).', jobsLoadFailed: 'Не удалось загрузить записи. Попробуйте ещё раз.',
  previous: '← Назад', next: 'Вперёд →', historyPages: 'Страницы истории записей',
  back: '← Все записи', ready: 'Расшифровка готова', languageNotDetected: 'Язык не определён', downloadSrt: '↓ Скачать .SRT', downloadVtt: '↓ Скачать .VTT', summary: 'Кратко', about: 'О чём эта запись',
  fullText: 'Полный текст', transcript: 'Расшифровка', copy: 'Копировать', copied: 'Скопировано', loading: 'Загружаем…', loadingTranscript: 'Загружаем расшифровку…',
  pending: 'Файл ждёт обработки', transcribing: 'Расшифровываем запись', processingNote: 'Длинные записи могут обрабатываться заметно дольше. Можете спокойно закрыть эту страницу: обработка продолжится, а готовый результат будет в вашей библиотеке.', failedProcess: 'Не удалось обработать файл.'
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

}
