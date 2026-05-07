# Task Management System

Production-ready task management platform built with **ASP.NET Core 8** (Clean Architecture + CQRS) and **Angular** (Material + NgRx + SignalR).

> **Status:** This repository is being built up incrementally, layer by layer. Each commit produces a runnable artifact for the layer it touches. See the *Build progress* section below for what is currently complete.

---

## Tech stack

| Layer | Tech |
|---|---|
| Backend | ASP.NET Core 8 Web API, EF Core 8, MediatR, FluentValidation, AutoMapper, Serilog |
| Auth | ASP.NET Core Identity + JWT access tokens + refresh tokens |
| Realtime | SignalR |
| Database | SQL Server 2022 |
| Frontend | Angular (latest), Angular Material, NgRx |
| Infra | Docker, docker-compose |

---

## Repository layout

```
.
├── src/
│   ├── TaskManagement.Domain/          # Entities, enums, domain primitives — no dependencies
│   ├── TaskManagement.Application/     # CQRS, validators, DTOs, abstractions
│   ├── TaskManagement.Infrastructure/  # EF Core, Identity, repositories, external services
│   └── TaskManagement.Api/             # ASP.NET Core host, controllers, middleware, SignalR hubs
├── tests/                              # xUnit tests (added later)
├── client/                             # Angular workspace (added later)
├── docker/                             # Dockerfiles + compose (added later)
└── TaskManagementSystem.sln
```

---

## Build progress

- [x] **Turn 1** — Solution scaffold, Domain layer, thin Application layer, Infrastructure (DbContext, EF configurations, generic repository, UnitOfWork, auditing interceptor, soft delete, seeder), minimal API host so EF migrations run, initial EF migration.
- [ ] **Turn 2** — Application layer: MediatR commands/queries for Auth, Users, Tasks, Comments, Notifications; FluentValidation; AutoMapper profiles.
- [ ] **Turn 3** — API layer: controllers, JWT middleware, SignalR hubs, Swagger, global exception middleware, Serilog wiring.
- [ ] **Turn 4** — Angular frontend (Material + NgRx + Kanban + charts).
- [ ] **Turn 5** — Docker setup, tests, deployment docs.

---

## Prerequisites

- .NET 8 SDK (`winget install Microsoft.DotNet.SDK.8`)
- Node.js 20+ (for the Angular client, added later)
- SQL Server 2022 — local instance, LocalDB, or via the bundled `docker-compose.yml`
- `dotnet ef` global tool: `dotnet tool install --global dotnet-ef`

---

## Quick start (current state)

```powershell
# 1. Restore + build
dotnet restore
dotnet build

# 2. Apply database migrations (from repo root)
dotnet ef database update --project src/TaskManagement.Infrastructure --startup-project src/TaskManagement.Api

# 3. Run the API
dotnet run --project src/TaskManagement.Api
```

The default connection string targets `localhost,1433` with SQL Auth (`sa` / `Your_password123`). Override via `appsettings.Development.json`, `appsettings.Local.json`, or the `ConnectionStrings__DefaultConnection` environment variable.

### Default seeded admin

| Email | Password | Role |
|---|---|---|
| `admin@taskmgmt.local` | `Admin@123!` | Admin |

> The seeded password is for local development only — change it before any non-local deployment.

---

## Known issues to triage

`dotnet build` currently reports two NuGet audit warnings against the latest available versions of libraries the Application/Infrastructure layers will use in upcoming turns:

- `AutoMapper` — [GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x) (high). No upstream fix as of this commit.
- `MailKit` — [GHSA-9j88-vvj5-vhgr](https://github.com/advisories/GHSA-9j88-vvj5-vhgr) (moderate). No upstream fix as of this commit.

Before deploying to production, decide one of:
1. Wait for a patched release and bump the version pin in [Directory.Packages.props](Directory.Packages.props).
2. Swap the dependency (e.g. Mapster instead of AutoMapper, raw SmtpClient instead of MailKit).
3. Suppress with `<NuGetAuditSuppress>` after a documented risk assessment of the specific code paths you exercise.

These are intentionally **not** suppressed in source so the warnings stay visible until a deliberate decision is made.

---

## License

MIT — see [LICENSE](LICENSE).
