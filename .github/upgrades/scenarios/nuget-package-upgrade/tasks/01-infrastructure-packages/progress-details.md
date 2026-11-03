# Progress Details

- Updated `Nbic.References.Infrastructure/Nbic.References.Infrastructure.csproj` to the recommended stable versions for `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.SqlServer`, and `SQLitePCLRaw.lib.e_sqlite3`.
- Preserved `Lucene.Net` and `Lucene.Net.Analysis.Common` on their current prerelease versions after the initial stable Lucene target caused a package downgrade conflict and the user requested that no packages be downgraded.
- Recorded the no-downgrade preference in `scenario-instructions.md` and updated the task/plan artifacts to match the final package choices.
- Validation: `dotnet build .\Nbic.References.sln` succeeded. The only remaining warnings were `NU1510` warnings in `Nbic.References.csproj` about `Microsoft.Extensions.Diagnostics.HealthChecks` and `Microsoft.Extensions.Logging.Debug` being implicitly available; no warnings remained in the infrastructure project itself.
