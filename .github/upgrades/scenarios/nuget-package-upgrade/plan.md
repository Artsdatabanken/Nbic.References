# NuGet Package Upgrade Plan

## Overview

**Target**: Upgrade the remaining NuGet packages in the `Nbic.References` solution to the latest supported stable versions, while preserving solution buildability and test coverage.
**Scope**: 3 projects with package references (`Nbic.References`, `Nbic.References.Infrastructure`, `Nbic.References.Tests`), with package updates applied per-project because Central Package Management is not enabled.

## Tasks

### 01-infrastructure-packages: Upgrade infrastructure packages

Update `Nbic.References.Infrastructure/Nbic.References.Infrastructure.csproj` to the recommended versions for its package references, including the Entity Framework Core packages and `SQLitePCLRaw.lib.e_sqlite3`. Keep the Lucene packages on their current prerelease versions for now because downgrading would break the dependency chain and the user requested that no packages be downgraded.

This task is expected to be mostly package-version edits, but it must still be validated by building the infrastructure project and any downstream consumers to catch transitive restore issues early.

**Done when**: the infrastructure project restores and builds with the updated package versions, and the package file reflects the chosen versions.

---

### 02-web-app-packages: Upgrade web application packages and fix breaking APIs

Update `Nbic.References/Nbic.References.csproj` to the recommended versions for its package references, including `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.CodeAnalysis.NetAnalyzers`, `Microsoft.EntityFrameworkCore.Tools`, health check packages, `Microsoft.Extensions.Logging.Debug`, `NuGet.Protocol`, `Swashbuckle.AspNetCore`, and `Swashbuckle.AspNetCore.Annotations`.

The assessment flagged breaking API diffs for `NuGet.Protocol` and `Swashbuckle.AspNetCore.Annotations`. If the build surfaces usage of removed APIs, migrate those call sites to the new package APIs or adjust configuration accordingly. Prefer code changes only where the compiler/build shows a real dependency on the removed members.

**Done when**: the web project restores and builds successfully on the upgraded package set, and any compile-time fallout from the flagged packages has been resolved.

---

### 03-test-packages: Upgrade test project packages

Update `Nbic.References.Tests/Nbic.References.Tests.csproj` to the recommended versions for `coverlet.collector`, `Microsoft.NET.Test.Sdk`, and `xunit.runner.visualstudio`, while keeping the already-current packages unchanged.

Validate the test project after the package changes so that the solution’s test tooling remains compatible with the upgraded dependencies.

**Done when**: the test project restores and builds successfully with the new package versions.

---

### 04-solution-validation: Build and test the upgraded solution

Run a full solution build and execute the test suite to confirm that the package upgrades are stable across project boundaries and that no hidden transitive issues remain.

**Done when**: the solution builds cleanly and the test suite passes.
