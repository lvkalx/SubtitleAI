# SubtitleAI frontend

Angular 20 frontend prepared for the existing .NET API.

## Run

1. Start services: `docker compose up -d postgres azurite`.
2. Start API: `dotnet run --project src/SubtitleAI.Api --launch-profile http`.
3. Start client: `npm start`.

The dev server proxies `/api` to `http://localhost:5161` through `src/proxy.conf.json`.

## Structure

- `src/app/core/api`: typed API models, services, and bearer-token interceptor;
- `src/app/features/auth`: login and registration;
- `src/app/features/jobs`: media upload and task list;
- routing in `src/app/app.routes.ts`.
