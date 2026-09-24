# 02-web-app-packages: Upgrade web application packages and fix breaking APIs

Update `Nbic.References/Nbic.References.csproj` to the recommended versions for its package references, including `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.CodeAnalysis.NetAnalyzers`, `Microsoft.EntityFrameworkCore.Tools`, health check packages, `Microsoft.Extensions.Logging.Debug`, `NuGet.Protocol`, `Swashbuckle.AspNetCore`, and `Swashbuckle.AspNetCore.Annotations`.

The assessment flagged breaking API diffs for `NuGet.Protocol` and `Swashbuckle.AspNetCore.Annotations`. If the build surfaces usage of removed APIs, migrate those call sites to the new package APIs or adjust configuration accordingly. Prefer code changes only where the compiler/build shows a real dependency on the removed members.

**Done when**: the web project restores and builds successfully on the upgraded package set, and any compile-time fallout from the flagged packages has been resolved.
