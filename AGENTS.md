# AGENTS.md

## Setup & Commands

```
dotnet build --configuration Release
dotnet test Nbic.References.Tests/Nbic.References.Tests.csproj
dotnet run --project Nbic.References
```

## Structure

4 `projects` in the solution (see nbic.references.admin below):

| Project | Role |
|---------|------|
| `Nbic.References` | Web API entrypoint (Program.cs), controllers, Swagger config, Dockerfile |
| `Nbic.References.Core` | Models (`Reference`, `ReferenceUsage`), repository interfaces, exceptions |
| `Nbic.References.Infrastructure` | Repository implementations, EF Core DbContexts, in-memory `Index` service |
| `Nbic.References.Tests` | xUnit tests |

`Nbic.References.ServiceDefaults` and `SandBox/` are not part of normal dev workflow.

## Angular Frontend — `nbic.references.admin`

| Command | Description |
|-----|-----|
| `npm install` | Install deps (run in `nbic.references.admin/`) |
| `npm start` | Dev server on `:4200` with `/api` proxied to the backend |
| `npm run build` | Production build to `dist/` |

- Angular 21 standalone components, `provideHttpClient(withFetch())`.
- Uses **Artsdatabanken Design System** (`@artsdatabanken/components` version 1.1.2) — a lit-based Web Component library with design tokens, dark mode, and WCAG 2.1 AA compliance.
- Design system is registered in `main.ts` via `import '@artsdatabanken/components';`.
- Components available: `<adb-button>`, `<adb-icon-button>`, `<adb-minimal-button>`, `<adb-accordion>`, `<adb-accordion-item>`, `<adb-alert>`, `<adb-checkbox>`, `<adb-radio>`, `<adb-tabs>`, `<adb-icon>`.
- Design tokens are CSS custom properties with `--adb-` prefix (e.g., `--adb-surface-accent`, `--adb-text-primary`, `--adb-spacing-sm`). Set dark mode via `[data-theme="dark"]` on `<html>`.
- Search component (`src/app/search/`) calls `GET /api/references?offset=&limit=12&search=`.
- Pagination uses a lower-bound strategy: when page 1 returns fewer than limit results, total is known; when it returns exactly limit, more results may exist.
- Angular `schemas: [CUSTOM_ELEMENTS_SCHEMA]` is required on components using Web Components so Angular doesn't complain about custom element tags and their attributes.
- Dev proxy (`proxy.conf.json`) forwards `/api` → the backend URL.

## Architecture Notes

- Static Main in `Program.cs` — all service registration happens there via static helpers.
- DI registration order: config → identity → swagger → db context → index/repositories → middleware.
- `IReferencesRepository` and `IReferenceUsageRepository` are registered in `Program.cs`. Their implementations live in `Nbic.References.Infrastructure/Repositories/`.
- An in-memory `Index` service (singleton) supports fast lookups. Always reindex when references are added, updated, or deleted.
- JWT Bearer auth. `WriteAccess` policy requires the role configured via `WriteAccessRole` env var; defaults to `my_write_access_role`.
- Demo IdentityServer (`https://demo.identityserver.com`) swaps the role claim for the issuer claim to enable Swagger testing without roles.

## Database

- `DbProvider=Sqlite` (default, in-memory `DataSource=:memory:`) or `SqlServer`.
- SQLite data dir: `./Data/` — map `/app/Data/` in Docker for persistence.
- Schema auto-migrates on first run.
- Contexts: `SqliteReferencesDbContext` and `SqlServerReferencesDbContext`. Use the correct one for migrations.
- Migration commands in `Nbic.References.Infrastructure/Repositories/DbContext/ReadMe.md`.

## Key URLs (when running locally)

- Swagger (root): `https://localhost:XXXXX/`
- Health check: `/hc`

## Dev Conventions

- Nullable reference types enabled in Core project.
- CA analyzers enabled via `Microsoft.CodeAnalysis.NetAnalyzers`.
- `NoWarn` suppresses CS1591 (XML doc warnings) — do not add unchecked warnings blindly.
- Tests use xUnit + coverlet collector. CI runs tests with `--logger "trx;LogFileName=test-results.trx"`.
- Branch: `master`. CI runs on push/PR to `master`.
- Published to Docker Hub as `artsdatabanken/nbicreferences`.
