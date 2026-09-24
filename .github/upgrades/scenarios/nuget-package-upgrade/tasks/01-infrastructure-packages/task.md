# 01-infrastructure-packages: Upgrade infrastructure packages

Update `Nbic.References.Infrastructure/Nbic.References.Infrastructure.csproj` to the recommended versions for its package references, including the Entity Framework Core packages and `SQLitePCLRaw.lib.e_sqlite3`. Keep the Lucene packages on their current prerelease versions for now because downgrading would break the dependency chain and the user requested that no packages be downgraded.

## Scope Inventory
- **Project affected**: `Nbic.References.Infrastructure`
- **Primary file**: `Nbic.References.Infrastructure/Nbic.References.Infrastructure.csproj`
- **Concerns**: package-version updates only; no CPM in the repo; SDK-style `net10.0` project so `dotnet build` is the correct validator; Lucene packages must remain at the current prerelease version to avoid downgrade conflicts

## Research Notes
- Assessment recommends upgrading `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.SqlServer` to `10.0.12`, and `SQLitePCLRaw.lib.e_sqlite3` to `3.53.3`.
- `Lucene.Net` and `Lucene.Net.Analysis.Common` stay on their current prerelease versions because the stable Lucene package would downgrade the dependency chain and the user explicitly requested no downgrades.
- No code-level API migration is expected from this task; validation should focus on restore/build health after the version bump.

This task is expected to be mostly package-version edits, but it must still be validated by building the infrastructure project and any downstream consumers to catch transitive restore issues early.

**Done when**: the infrastructure project restores and builds with the updated package versions, and the package file reflects the chosen versions.
