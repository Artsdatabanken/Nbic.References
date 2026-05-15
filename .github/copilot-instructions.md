# Copilot Instructions for Nbic.References

## Overview
Nbic.References is a .NET 9 REST API for managing literature, person, and URL references, including usage tracking by user and application. The project was migrated from a class library to a modern API, with a focus on maintainability, testability, and flexible database support (Sqlite/SqlServer).

## Architecture
- **API Layer**: Controllers in `Nbic.References/Controllers` expose endpoints for references and usage. All endpoints are documented with Swagger (see root URL when running).
- **Core Layer**: Domain models and repository interfaces are in `Nbic.References.Core`. Models like `Reference` and `ReferenceUsage` are central.
- **Infrastructure Layer**: Implementations of repositories and EF Core DbContexts are in `Nbic.References.Infrastructure`. Supports both Sqlite and SqlServer via configuration.
- **Indexing**: An in-memory index (`Index` service) is injected into repositories for fast lookups and reindexing.

## Key Patterns & Conventions
- **Dependency Injection**: All services, repositories, and the index are registered in `Program.cs`.
- **Authorization**: Uses JWT Bearer tokens. The `WriteAccess` policy is enforced for mutating endpoints. Swagger UI is configured for OAuth2 flows.
- **Database Migrations**: Use EF Core migrations. See `Nbic.References.Infrastructure/Repositories/DbContext/ReadMe.md` for migration commands for both Sqlite and SqlServer contexts.
- **Error Handling**: Custom exceptions in `Nbic.References.Core/Exceptions` are thrown for domain errors (e.g., `NotFoundException`, `BadRequestException`).
- **Testing**: Tests are in `Nbic.References.Tests`. Use `dotnet test` to run. CI runs tests on every PR and push to `master`.

## Developer Workflows
- **Build**: `dotnet build --configuration Release`
- **Test**: `dotnet test Nbic.References.Tests/Nbic.References.Tests.csproj`
- **Run Locally**: `dotnet run --project Nbic.References`
- **Docker**: Build and run with Docker using the provided `Dockerfile`. Example:
  ```sh
  docker run -d -p 8080:8000 --name nbicreferences artsdatabanken/nbicreferences
  ```
- **Configuration**: Set environment variables for DB provider, connection string, and authentication. See `README.md` for defaults.

## Integration Points
- **Swagger/OpenAPI**: All endpoints are documented and discoverable at the root URL when running.
- **Authentication**: Integrates with external IdentityServer (see `AuthAuthority` in config).
- **Database**: Supports both Sqlite and SqlServer. Schema is auto-created on first run.

## Project-Specific Guidance
- Always update the in-memory index when references are added, updated, or deleted.
- Use the `WriteAccess` policy for any endpoint that mutates data.
- For migrations, use the correct context (`SqliteReferencesDbContext` or `SqlServerReferencesDbContext`).
- Prefer using repository interfaces from Core for testability.

## Key Files & Directories
- `Nbic.References/Program.cs`: Service registration, configuration, and app startup.
- `Nbic.References/Controllers/`: API endpoints.
- `Nbic.References.Infrastructure/Repositories/`: Repository implementations and DbContexts.
- `Nbic.References.Core/Models/`: Domain models.
- `README.md`: High-level project info and Docker usage.
- `.github/workflows/dotnetcore.yml`: CI/CD pipeline.

---
If you are unsure about a pattern or integration, check the referenced files or ask for clarification.
