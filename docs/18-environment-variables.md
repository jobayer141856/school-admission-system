# 18 — Environment Variables

Keep `.env.example` safe for public GitHub. Never commit actual credentials.

## Root `.env.example`

```dotenv
POSTGRES_DB=school_admission
POSTGRES_USER=school_app
POSTGRES_PASSWORD=change_me_for_local_development
POSTGRES_PORT=5432

HOLD_DURATION_SECONDS=600
ASPNETCORE_ENVIRONMENT=Development
API_PORT=8080
FRONTEND_PORT=3000

# This value is used by the browser, not by the frontend container's internal network.
VITE_API_BASE_URL=http://localhost:8080

CORS_ALLOWED_ORIGINS=http://localhost:3000
```

## Backend configuration

- `ConnectionStrings__DefaultConnection`
- `HOLD_DURATION_SECONDS`
- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS=http://+:8080`
- `CORS_ALLOWED_ORIGINS`

Map Compose variables explicitly to .NET configuration. Do not rely on a developer-specific `appsettings.Development.json`.

## Frontend configuration

Vite variables prefixed with `VITE_` are exposed to browser code at build time. If using build-time injection, rebuild the image when the API URL changes. If runtime configurability is required, serve a generated `config.js` from Nginx and read it before app startup.

## Validation

Fail fast on missing required database configuration. Validate `HOLD_DURATION_SECONDS` is a positive integer and apply the default of 600 when omitted.
