export interface AuthResponse { accessToken: string; refreshToken: string; accessTokenExpiresAt: string; }
export interface User { id: string; email: string; }
export interface Job { id: string; fileName: string; status: string; durationSeconds: number | null; language: string | null; errorMessage: string | null; createdAt: string; completedAt: string | null; }
export interface Summary { text: string; model: string; createdAt: string; }
