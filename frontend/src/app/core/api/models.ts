export interface AuthResponse { accessToken: string; accessTokenExpiresAt: string; }
export interface User { id: string; email: string; }
export interface Job { id: string; fileName: string; status: string; durationSeconds: number | null; language: string | null; errorMessage: string | null; createdAt: string; completedAt: string | null; }
export interface PagedResult<T> { items: T[]; page: number; pageSize: number; totalCount: number; }
export interface Summary { text: string; model: string; createdAt: string; }
export interface AdminAnalytics { users: number; usersLast30Days: number; transcriptions: number; completed: number; processing: number; failed: number; transcripts: number; summaries: number; totalMediaMinutes: number; activity: { date: string; count: number }[]; }
