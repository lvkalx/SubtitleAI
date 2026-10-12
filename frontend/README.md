# SubtitleAI frontend

Angular 20 frontend prepared for the existing .NET API.

## Run

1. Copy `env.example` to `.env` and fill in local values. Never commit it.
2. Start services: `docker compose up -d postgres azurite`.
2. Start API: `dotnet run --project src/SubtitleAI.Api --launch-profile http`.
3. Start client: `npm start`.

The dev server proxies `/api` to `http://localhost:5161` through `src/proxy.conf.json`.

## Docker

`docker compose --profile app up --build` starts the full stack at `http://localhost:8080`.
The Angular bundle is served by Nginx; `/api` and `/health` are reverse-proxied to an API that is not publicly published. In production put an HTTPS Azure ingress in front of this endpoint.

## Security notes

- The access token is kept in `sessionStorage`; the refresh token is an HttpOnly, `SameSite=Strict` cookie and is never returned in JSON.
- CORS has an explicit allow-list. The dev server proxy avoids CORS locally, while Docker uses a same-origin reverse proxy.
- Do not put Azure keys in Angular environments, images, GitHub variables visible to untrusted PRs, or source control. Use Azure Key Vault / App Service settings in Azure.

## Structure

- `src/app/core/api`: typed API models, services, and bearer-token interceptor;
- `src/app/features/auth`: login and registration;
- `src/app/features/jobs`: media upload and task list;
- routing in `src/app/app.routes.ts`.
