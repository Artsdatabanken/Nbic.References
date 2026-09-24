# NuGet Package Upgrade

## Strategy
Per-project package upgrades in solution order, with code fixes only where the build shows a real dependency on breaking APIs.

## Preferences
- **Flow Mode**: Automatic
- **Scope**: Entire solution (all remaining NuGet packages)
- **Version Policy**: Latest stable versions unless a package requires a specific supported version
- **Package Downgrades**: Do not downgrade packages; keep current prerelease versions when the stable alternative would force a downgrade
- **Lucene.Net.Analysis.Common**: Keep current prerelease version for now; no supported stable target was resolved.

## Source Control
- **Source Branch**: develop
- **Working Branch**: upgrade-nuget-packages
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Build Tool Decisions
- **Nbic.References.Infrastructure/Nbic.References.Infrastructure.csproj**: dotnet build (SDK-style, net10.0)
