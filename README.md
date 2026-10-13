# SubtitleAI

Small full-stack portfolio project that accepts audio/video, produces subtitles, and optionally creates an Azure OpenAI summary.

## Stack

- Angular 20 client with typed API calls, protected routes, upload UI, and paginated job history.
- ASP.NET Core 9 API, PostgreSQL, Azure Blob Storage/Azurite, Whisper and optional Azure OpenAI.
- Docker Compose: Nginx serves the SPA and reverse-proxies the API; PostgreSQL and Azurite are bound to loopback only.

## Run locally

1. Copy `env.example` to `.env`, create unique values for `POSTGRES_PASSWORD` and `JWT_KEY`, and fill Azure OpenAI settings only when a summary is needed.
2. Run `docker compose --profile app up --build`.
3. Open `http://localhost:8080`.

The Compose stack is explicitly a local development environment, so its refresh cookie is allowed over loopback HTTP. Production uses the default `Production` environment, which marks that cookie `Secure`; terminate TLS before the application.

For Angular development, run the API on `http://localhost:5161` and `npm start` in `frontend`; its proxy forwards `/api` calls.
For direct API development, store the same local settings outside Git, for example with `dotnet user-secrets set "Jwt:Key" "<a unique 32+ character value>" --project src/SubtitleAI.Api` (and set the database/storage values from `appsettings.Development.json` as needed).

## Verification

```powershell
dotnet test SubtitleAI.sln -c Release
Set-Location frontend
npm ci
npm run build
npm run test:ci
```

The GitHub Actions workflow restores/builds/tests the API and builds/tests Angular on every pull request and push to `main`.

## Security baseline

- No checked-in secrets: `.env` is ignored and `env.example` contains placeholders only.
- Refresh sessions are `HttpOnly`, `SameSite=Strict` cookies; the SPA receives no refresh token. Access tokens live only for the current browser session.
- Explicit CORS allow-list, request rate limiting, security response headers, upload size/type limits, and ownership-scoped queries.
- Use HTTPS and store runtime secrets in Azure Key Vault/App Service configuration when deploying. Rotate any Azure key that was ever placed in a local `.env` or shared in a terminal/screenshot.

## Email recovery and confirmation

Password recovery and email confirmation use ordinary SMTP. A custom domain is not required: a free Gmail account with two-step verification and an app password is sufficient. Configure `EMAIL__HOST`, `EMAIL__PORT`, `EMAIL__USERNAME`, `EMAIL__PASSWORD`, `EMAIL__FROM`, and `EMAIL__PUBLICBASEURL` as runtime secrets/settings. Set `EMAIL__REQUIRECONFIRMEDEMAIL=true` only after delivery has been verified; otherwise existing registration and login continue to work without email confirmation.

## Azure deployment

This repository intentionally has no deployment credentials or Azure resource identifiers. Configure an HTTPS ingress, Azure Database for PostgreSQL, a Storage Account, and Key Vault/App Service settings, then provide those values as GitHub environment secrets or federated OIDC configuration. The container entrypoint is ready for that flow; do not add cloud keys to the repository.
