# 03-test-packages: Upgrade test project packages

Update `Nbic.References.Tests/Nbic.References.Tests.csproj` to the recommended versions for `coverlet.collector`, `Microsoft.NET.Test.Sdk`, and `xunit.runner.visualstudio`, while keeping the already-current packages unchanged.

Validate the test project after the package changes so that the solution’s test tooling remains compatible with the upgraded dependencies.

**Done when**: the test project restores and builds successfully with the new package versions.
