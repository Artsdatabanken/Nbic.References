# 02-web-app-packages: Upgrade web application packages and fix breaking APIs

Update `Nbic.References/Nbic.References.csproj` to the recommended versions for its package references, including `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.CodeAnalysis.NetAnalyzers`, `Microsoft.EntityFrameworkCore.Tools`, health check packages, `Microsoft.Extensions.Logging.Debug`, `NuGet.Protocol`, `Swashbuckle.AspNetCore`, and `Swashbuckle.AspNetCore.Annotations`.

## Scope Inventory
- **Project affected**: `Nbic.References`
- **Primary file**: `Nbic.References/Nbic.References.csproj`
- **Concerns**: package-version updates plus potential API fallout from `NuGet.Protocol` and `Swashbuckle.AspNetCore.Annotations`; SDK-style `net10.0` web project so `dotnet build` is the correct validator

## Research Notes
- Assessment recommends upgrading the web project packages to `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.12`, `Microsoft.CodeAnalysis.NetAnalyzers 10.0.401`, `Microsoft.EntityFrameworkCore.Tools 10.0.12`, `Microsoft.Extensions.Diagnostics.HealthChecks 10.0.12`, `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 10.0.12`, `Microsoft.Extensions.Logging.Debug 10.0.12`, `NuGet.Protocol 7.9.0`, `Swashbuckle.AspNetCore 10.2.3`, and `Swashbuckle.AspNetCore.Annotations 10.2.3`.
- `NuGet.Protocol` and `Swashbuckle.AspNetCore.Annotations` are the only packages with breaking API diffs in the assessment; initial code searches did not find any direct usages of their removed members, so the first pass should be package-only unless the build exposes a real dependency.
- The project already targets `net10.0` and references the infrastructure project, so restoring/building the solution after the package bump will also validate downstream compatibility.

The assessment flagged breaking API diffs for `NuGet.Protocol` and `Swashbuckle.AspNetCore.Annotations`. If the build surfaces usage of removed APIs, migrate those call sites to the new package APIs or adjust configuration accordingly. Prefer code changes only where the compiler/build shows a real dependency on the removed members.

**Done when**: the web project restores and builds successfully on the upgraded package set, and any compile-time fallout from the flagged packages has been resolved.
