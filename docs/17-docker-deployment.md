# 17 — Docker & Deployment

## Required services

- `postgres`: PostgreSQL with named volume.
- `api`: ASP.NET Core API, port `8080`.
- `frontend`: production React build served by Nginx, port `3000`.

## Compose requirements

- Root `docker-compose.yml`.
- `depends_on` with PostgreSQL health check.
- API receives connection string and `HOLD_DURATION_SECONDS`.
- API health check uses `/health`.
- Frontend receives browser-reachable API base URL.
- PostgreSQL data stored in a named volume.
- Use multi-stage Dockerfiles.
- `.dockerignore` excludes build output, `.git`, `.env`, node_modules, bin, obj, and test artifacts.

## Critical networking detail

The API connects to PostgreSQL using `Host=postgres;Port=5432` inside Compose. The browser must call the API through a host-accessible URL such as `http://localhost:8080`, not `http://api:8080`.

For a remote deployment, set the frontend API URL to the public HTTPS API origin and configure CORS accordingly.

## Persistence

`docker compose down` preserves named volumes. `docker compose down -v` deletes volumes and database data; do not use it for normal shutdown.

## Build and run

```bash
cp .env.example .env
docker compose up --build
```

## Troubleshooting

- API cannot connect to DB: verify `Host=postgres`, credentials, and database health.
- Browser CORS error: verify allowed origin and API URL.
- Frontend calls `api:8080`: change browser-facing API URL to host/public URL.
- Port conflict: change host-side port mapping, keeping container API port 8080.
- Data missing: check named volume and avoid `down -v`.
- Migration failure: inspect API logs and ensure the database is healthy before migration.
