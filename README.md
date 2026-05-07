# Task Management System

Production-ready task management platform built with **ASP.NET Core 8** (Clean Architecture + CQRS) and **Angular 21** (Material + NgRx + SignalR).

> **Status:** Built layer by layer in five focused commits. Each layer compiles, runs, and is independently reviewable on GitHub.

---

## Tech stack

| Layer | Tech |
|---|---|
| Backend | ASP.NET Core 8 Web API, EF Core 8, MediatR 12, FluentValidation 11, AutoMapper 14, Serilog |
| Auth | ASP.NET Core Identity + JWT access tokens + SHA-256-hashed refresh tokens with rotation |
| Realtime | SignalR (per-user + per-task groups) |
| Database | SQL Server 2022 |
| Frontend | Angular 21 (standalone components, lazy routes), Angular Material 21, NgRx 21, ng2-charts/Chart.js, @microsoft/signalr |
| Tests | xUnit + FluentAssertions + Moq + Microsoft.AspNetCore.Mvc.Testing |
| Infra | Docker, docker-compose, nginx (SPA + reverse proxy) |

---

## Repository layout

```
.
├── src/
│   ├── TaskManagement.Domain/          # Entities, enums, domain primitives
│   ├── TaskManagement.Application/     # CQRS handlers, validators, DTOs, abstractions
│   ├── TaskManagement.Infrastructure/  # EF Core, Identity, JWT, email, file storage, activity logger
│   └── TaskManagement.Api/             # Controllers, JWT auth, SignalR hubs, exception MW, Serilog
├── tests/
│   ├── TaskManagement.Application.UnitTests/
│   └── TaskManagement.Api.IntegrationTests/
├── client/                             # Angular 21 workspace
├── docker/
│   ├── backend.Dockerfile
│   ├── frontend.Dockerfile
│   └── nginx.conf
├── docker-compose.yml
├── .env.example
└── TaskManagementSystem.sln
```

---

## Build progress

- [x] **Turn 1** — Solution scaffold, Domain layer, thin Application layer, Infrastructure (DbContext, EF configurations, generic repository, UnitOfWork, auditing interceptor, soft delete, seeder), minimal API host so EF migrations run, initial EF migration.
- [x] **Turn 2** — Application layer: MediatR commands/queries for Auth, Users, Tasks, Comments, Notifications, Labels, Dashboard; FluentValidation; AutoMapper profiles; pipeline behaviors (validation/logging/performance/exception); abstractions for JWT, identity, email, SignalR notifications, file storage, activity logging.
- [x] **Turn 3** — API layer: controllers (Auth/Users/Tasks/Comments/Notifications/Labels/Dashboard), JWT bearer auth + role policies, SignalR notifications hub, Swagger with JWT support, ProblemDetails-based exception middleware, Serilog request logging + rolling file sink, CORS. Infrastructure implementations of all Application abstractions.
- [x] **Turn 4** — Angular 21 frontend (standalone, lazy routes, Material + NgRx + Chart.js + SignalR client). Auth flow, responsive shell, dashboard with charts, tasks list, Kanban board with drag-drop + live SignalR updates, task detail with comments, notifications, profile, admin users.
- [x] **Turn 5** — Docker (backend Dockerfile, multi-stage frontend → nginx with SPA + reverse proxy for /api and /hubs, docker-compose with SQL Server + api + web + seed profile). Test projects (xUnit unit tests + WebApplicationFactory integration tests).

---

## Quick start

### Option A — Docker (recommended)

Requires Docker Desktop. From the repo root:

```bash
cp .env.example .env
# (optional) edit .env to change ports / passwords / JWT key

# Build and start SQL Server, the API, and the Angular client behind nginx
docker compose up -d --build

# Apply migrations and seed roles + demo users + demo tasks (one-shot)
docker compose --profile seed run --rm api-seed
```

When the stack is up:

| Service | URL |
|---|---|
| Web (SPA via nginx) | http://localhost:4200 |
| API (direct) | http://localhost:5080 |
| Swagger | http://localhost:5080/swagger |
| SQL Server | localhost,1433 (sa / `Your_password123` by default) |

To stop: `docker compose down` (add `-v` to wipe the volumes).

### Option B — Local development

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), Node.js 20+, and a SQL Server instance.

```powershell
# 1. Backend
dotnet tool install --global dotnet-ef          # if you don't have it
dotnet restore
dotnet build

# 2. Apply migrations (configure ConnectionStrings__DefaultConnection if not localhost,1433)
dotnet ef database update `
  --project src/TaskManagement.Infrastructure `
  --startup-project src/TaskManagement.Api

# 3. Seed admin/manager/employee + 5 demo tasks
dotnet run --project src/TaskManagement.Api -- --seed

# 4. Run the API (HTTPS on 7080, HTTP on 5080)
dotnet run --project src/TaskManagement.Api

# 5. Run the Angular client (in another terminal)
cd client
npm install
npm start                                        # http://localhost:4200
```

---

## Default seeded test accounts

| Email | Password | Role |
|---|---|---|
| `admin@taskmgmt.local` | `Admin@123!` | Admin |
| `manager@taskmgmt.local` | `Manager@123!` | Manager |
| `employee@taskmgmt.local` | `Employee@123!` | Employee |

> Local-development passwords only — change them before any non-local deployment.

The seeder also creates 5 demo labels (Bug, Feature, Improvement, Documentation, Urgent) and 5 demo tasks across the workflow.

---

## Sample API requests

```bash
# Sign in
curl -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@taskmgmt.local","password":"Admin@123!"}'

# Create a task (uses access token from /login)
curl -X POST http://localhost:5080/api/tasks \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"My first task","description":"Hello","status":1,"priority":2}'

# Drag-drop on the Kanban board ↔ change-status
curl -X POST http://localhost:5080/api/tasks/$TASK_ID/status \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":2}'   # 1=Todo 2=InProgress 3=Review 4=Completed
```

```typescript
// Subscribe to live notifications (browser SDK)
import { HubConnectionBuilder } from '@microsoft/signalr';

const conn = new HubConnectionBuilder()
  .withUrl('/hubs/notifications', { accessTokenFactory: () => token })
  .build();
conn.on('notification', n => console.log(n));
conn.on('task-changed', e => console.log(e));
await conn.start();
```

---

## Architecture at a glance

```
┌────────────────────┐     HTTPS / WS      ┌────────────────────────────┐
│  Angular client    │ ──────────────────► │  ASP.NET Core 8 API        │
│  (Material + NgRx) │ ◄────────────────── │  Program.cs wires:         │
│  client/           │   JSON / SignalR    │   ├─ JWT Bearer            │
└────────────────────┘                     │   ├─ Swagger               │
                                           │   ├─ ProblemDetails MW     │
                                           │   ├─ CORS                  │
                                           │   ├─ Serilog               │
                                           │   ├─ Controllers           │
                                           │   └─ NotificationsHub      │
                                           ├────────────┬───────────────┤
                                           │ Application│ Infrastructure│
                                           │  (CQRS +   │  (EF Core +   │
                                           │ Validation │   JwtService  │
                                           │  + AutoMap)│ + IdentityMgr │
                                           │            │ + EmailSender │
                                           │            │ + FileStorage │
                                           │            │ + ActivityLog)│
                                           ├────────────┴───────────────┤
                                           │            Domain          │
                                           │   (entities + enums only)  │
                                           └────────────┬───────────────┘
                                                        │
                                                        ▼
                                                ┌──────────────┐
                                                │  SQL Server  │
                                                └──────────────┘
```

**Key design choices**

- **Soft delete** is implemented via a `SaveChangesInterceptor` that converts `EntityState.Deleted` to `Modified` and stamps `IsDeleted/DeletedAt/DeletedBy`. Global query filters drop deleted rows transparently.
- **Audit fields** (`CreatedAt/UpdatedAt/CreatedBy/UpdatedBy`) are written by the same interceptor — handlers never have to remember.
- **Refresh tokens** are 64 random bytes, stored as SHA-256 hashes, single-use with rotation, and reused-token detection revokes the user's entire chain (defense-in-depth against token theft).
- **CQRS pipeline** runs `UnhandledExceptionBehavior → LoggingBehavior → ValidationBehavior → PerformanceBehavior` (outermost first), so every command/query gets validation + structured logging + slow-query warnings for free.
- **Realtime** has two channels — per-user notification fan-out (assignee on assignment, creator on completion, both on new comments) and per-task subscription (Kanban / detail page sees status/comment changes from other users without polling).

---

## Running the tests

```powershell
# All backend tests (xUnit) — currently 18 tests, all green
dotnet test

# A specific test
dotnet test --filter "FullyQualifiedName~Handle_creates_a_task_for_the_current_user"

# Angular tests (Vitest)
cd client
npm test
```

The integration tests (`tests/TaskManagement.Api.IntegrationTests`) spin up the real ASP.NET host with `WebApplicationFactory<Program>` and swap SQL Server for the EF Core in-memory provider — no DB required to run them.

---

## Known issues to triage

`dotnet build` reports two NuGet audit warnings against the latest available versions:

- `AutoMapper` — [GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x) (high). No upstream fix as of this commit.
- `MailKit` — [GHSA-9j88-vvj5-vhgr](https://github.com/advisories/GHSA-9j88-vvj5-vhgr) (moderate). No upstream fix as of this commit.

Before deploying to production, decide one of:
1. Wait for a patched release and bump the version pin in [Directory.Packages.props](Directory.Packages.props).
2. Swap the dependency (e.g. Mapster instead of AutoMapper, raw `SmtpClient` instead of MailKit).
3. Suppress with `<NuGetAuditSuppress>` after a documented risk assessment.

These are intentionally **not** suppressed in source so the warnings stay visible until a deliberate decision is made.

---

## Troubleshooting

| Symptom | Likely cause / fix |
|---|---|
| `dotnet ef database update` fails: "Cannot connect to SQL Server" | The connection string defaults to `localhost,1433` with `sa` / `Your_password123`. Override via `ConnectionStrings__DefaultConnection` env var or edit `appsettings.Development.json`. The bundled `docker compose up db` works out of the box. |
| `Jwt:SigningKey must be configured and at least 32 characters long` | Set the `Jwt__SigningKey` environment variable (or update `appsettings.json`) to a string of at least 32 chars. |
| Angular build fails on `mat.$indigo-palette` | Material 21 renamed pre-built palettes. We use `$azure-palette` — see `client/src/styles.scss`. |
| SignalR client can't authenticate | The bearer token must be passed via `accessTokenFactory` in the `HubConnectionBuilder` — websocket upgrades can't carry an `Authorization` header. The API's JWT bearer config also accepts `?access_token=` on the hub URL. |
| Emails are sent but I never see them | The default `Email:Mode` is `PickupDirectory` — `.eml` files land in `./mail-pickup` (or the Docker volume `taskmgmt-mail`). Use a tool like Papercut or open the `.eml` in your mail client. |
| Build fails with "Cannot create a file when that file already exists" | EF Core 10 SDK on top of net8.0 sometimes emits this in a clean restore. Re-run `dotnet build` — it always succeeds on the second pass. |

---

## License

MIT — see [LICENSE](LICENSE).
