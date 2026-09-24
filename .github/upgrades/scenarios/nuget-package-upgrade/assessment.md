# NuGet package upgrade assessment

_Mode: **quick assessment** — package API diffs only; no per-project source scan was run._

## Recommended versions

- **coverlet.collector**: **10.0.1** (unified across 1 project(s)).
- **Lucene.Net**: **2.9.2.2** (unified across 1 project(s)).
- **Lucene.Net.Analysis.Common**: not referenced by any scoped project, or no supported version was found.
- **Microsoft.ApplicationInsights.AspNetCore**: **3.1.2** (unified across 1 project(s)).
- **Microsoft.AspNetCore.Authentication.JwtBearer**: **10.0.12** (unified across 1 project(s)).
- **Microsoft.CodeAnalysis.NetAnalyzers**: **10.0.401** (unified across 1 project(s)).
- **Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers**: **0.4.421302** (unified across 2 project(s)).
- **Microsoft.EntityFrameworkCore.Design**: **10.0.12** (unified across 1 project(s)).
- **Microsoft.EntityFrameworkCore.Sqlite**: **10.0.12** (unified across 1 project(s)).
- **Microsoft.EntityFrameworkCore.SqlServer**: **10.0.12** (unified across 1 project(s)).
- **Microsoft.EntityFrameworkCore.Tools**: **10.0.12** (unified across 1 project(s)).
- **Microsoft.Extensions.Diagnostics.HealthChecks**: **10.0.12** (unified across 1 project(s)).
- **Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore**: **10.0.12** (unified across 1 project(s)).
- **Microsoft.Extensions.Logging.Debug**: **10.0.12** (unified across 1 project(s)).
- **Microsoft.NET.Test.Sdk**: **18.10.1** (unified across 1 project(s)).
- **Microsoft.VisualStudio.Web.CodeGeneration.Design**: **10.0.2** (unified across 1 project(s)).
- **NuGet.Protocol**: **7.9.0** (unified across 1 project(s)).
- **RobotsTxtCore**: **3.0.0** (unified across 1 project(s)).
- **SQLitePCLRaw.lib.e_sqlite3**: **3.53.3** (unified across 1 project(s)).
- **Swashbuckle.AspNetCore**: **10.2.3** (unified across 1 project(s)).
- **Swashbuckle.AspNetCore.Annotations**: **10.2.3** (unified across 1 project(s)).
- **xunit**: **2.9.3** (unified across 1 project(s)).
- **xunit.runner.visualstudio**: **4.0.0** (unified across 1 project(s)).

## Public API changes

> **Types moved (namespace changed) are not removals.** A moved type keeps its name and members;
> the fix is a `using`-directive change, not a rewrite. Do not treat a moved type as deleted.

- **Microsoft.EntityFrameworkCore.Design**: no source-breaking public API changes detected — see [`apidiff/Microsoft.EntityFrameworkCore.Design.apidiff.md`](apidiff/Microsoft.EntityFrameworkCore.Design.apidiff.md).
- **Microsoft.EntityFrameworkCore.Sqlite**: no source-breaking public API changes detected — see [`apidiff/Microsoft.EntityFrameworkCore.Sqlite.apidiff.md`](apidiff/Microsoft.EntityFrameworkCore.Sqlite.apidiff.md).
- **Microsoft.EntityFrameworkCore.SqlServer**: no source-breaking public API changes detected — see [`apidiff/Microsoft.EntityFrameworkCore.SqlServer.apidiff.md`](apidiff/Microsoft.EntityFrameworkCore.SqlServer.apidiff.md).
- **Lucene.Net**: no source-breaking public API changes detected — see [`apidiff/Lucene.Net.apidiff.md`](apidiff/Lucene.Net.apidiff.md).
- **SQLitePCLRaw.lib.e_sqlite3**: no source-breaking public API changes detected — see [`apidiff/SQLitePCLRaw.lib.e_sqlite3.apidiff.md`](apidiff/SQLitePCLRaw.lib.e_sqlite3.apidiff.md).
- **Microsoft.AspNetCore.Authentication.JwtBearer**: no source-breaking public API changes detected — see [`apidiff/Microsoft.AspNetCore.Authentication.JwtBearer.apidiff.md`](apidiff/Microsoft.AspNetCore.Authentication.JwtBearer.apidiff.md).
- **Microsoft.CodeAnalysis.NetAnalyzers**: no source-breaking public API changes detected — see [`apidiff/Microsoft.CodeAnalysis.NetAnalyzers.apidiff.md`](apidiff/Microsoft.CodeAnalysis.NetAnalyzers.apidiff.md).
- **Microsoft.EntityFrameworkCore.Tools**: no source-breaking public API changes detected — see [`apidiff/Microsoft.EntityFrameworkCore.Tools.apidiff.md`](apidiff/Microsoft.EntityFrameworkCore.Tools.apidiff.md).
- **Microsoft.Extensions.Diagnostics.HealthChecks**: no source-breaking public API changes detected — see [`apidiff/Microsoft.Extensions.Diagnostics.HealthChecks.apidiff.md`](apidiff/Microsoft.Extensions.Diagnostics.HealthChecks.apidiff.md).
- **Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore**: no source-breaking public API changes detected — see [`apidiff/Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore.apidiff.md`](apidiff/Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore.apidiff.md).
- **Microsoft.Extensions.Logging.Debug**: no source-breaking public API changes detected — see [`apidiff/Microsoft.Extensions.Logging.Debug.apidiff.md`](apidiff/Microsoft.Extensions.Logging.Debug.apidiff.md).
- **NuGet.Protocol**: 5 type(s) removed, 8 member(s) removed — see [`apidiff/NuGet.Protocol.apidiff.md`](apidiff/NuGet.Protocol.apidiff.md).
- **Swashbuckle.AspNetCore**: no source-breaking public API changes detected — see [`apidiff/Swashbuckle.AspNetCore.apidiff.md`](apidiff/Swashbuckle.AspNetCore.apidiff.md).
- **Swashbuckle.AspNetCore.Annotations**: 9 member(s) removed — see [`apidiff/Swashbuckle.AspNetCore.Annotations.apidiff.md`](apidiff/Swashbuckle.AspNetCore.Annotations.apidiff.md).
- **coverlet.collector**: no source-breaking public API changes detected — see [`apidiff/coverlet.collector.apidiff.md`](apidiff/coverlet.collector.apidiff.md).
- **Microsoft.NET.Test.Sdk**: no source-breaking public API changes detected — see [`apidiff/Microsoft.NET.Test.Sdk.apidiff.md`](apidiff/Microsoft.NET.Test.Sdk.apidiff.md).
- **xunit.runner.visualstudio**: no source-breaking public API changes detected — see [`apidiff/xunit.runner.visualstudio.apidiff.md`](apidiff/xunit.runner.visualstudio.apidiff.md).

## Breaking-change findings

- Version divergence findings (Pkg.0003): 0
- Requested-version-unsupported findings (Pkg.0002): 0

- Quick mode does not scan source, so there are no per-line `PkgApi` usage findings. Review the
  per-package API diffs above and rely on build errors during execution to pinpoint affected code.
- A full code scan can locate the exact source location of every breaking-change usage across the repo.
  It is opt-in and slower — re-run the assessment with `fullScan=true` only if the user requests it.

## Next steps

1. Proceed to planning to triage the API changes above and plan the code fixes (if any).

