# .NET 10 upgrade — 17 September 2026

Implemented on `major-update`. All six projects, including tests, target `net10.0`. The app retains its existing Blazor Server hosting model; this does not introduce WebAssembly, Angular, or new render modes.

## Versions

| Component | Version |
| --- | --- |
| SDK (`global.json`, latest patch roll-forward) | 10.0.401 |
| Local ASP.NET Core/.NET runtime | 10.0.12 |
| EF Core and SQL Server provider/tools | 10.0.12 |
| Syncfusion.Blazor | 33.2.15 |
| Serilog.AspNetCore / Settings.Configuration | 10.0.0 / 10.0.1 |
| Blazored.FluentValidation / FluentValidation | 2.2.0 / 11.12.0 |
| Microsoft.NET.Test.Sdk | 18.10.1 |
| xUnit / Visual Studio runner | 2.9.3 / 4.0.0 |

The Blazored adapter is archived. FluentValidation remains on the compatible 11.x line for this bounded upgrade; replace the adapter and move to FluentValidation 12 in a separate change with form tests. Syncfusion's supported .NET 10 versions start at 31.2.10. Confirm the registered license covers the upgraded version before production deployment; no new license or entitlement was obtained here.

## Local use

The SDK was installed side by side at `C:\Users\Luke\.dotnet`; the system installation and PATH were not changed. In PowerShell, from the repository root:

```powershell
$dotnet10 = "$env:USERPROFILE\.dotnet\dotnet.exe"
& $dotnet10 build RopeParison.sln -c Release
& $dotnet10 test RopeParison.Tests/RopeParison.Tests.csproj -c Release --filter 'Category!=KnownDefect'

$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__dataConnection = 'Server=localhost\SQLEXPRESS;Database=RopeParison_Local;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;'
$env:ConnectionStrings__securityConnection = $env:ConnectionStrings__dataConnection
& $dotnet10 run --project RopeParison/RopeParison.csproj -c Release --no-launch-profile --urls http://localhost:5169
```

The existing local instance must be stopped before starting another instance on port 5169. On another machine, install the SDK specified in `global.json` and use its `dotnet` executable. Use an IDE version that supports that SDK; the previously installed Visual Studio 2022 is not the build environment verified here.

## Verification

- Release build: passed, 115 warnings, zero errors. Existing nullability, async and component-parameter warnings remain; they were not suppressed.
- Release publish of the web project: passed. The passing-only test command separately passed all 15 cases.
- Full tests: 15 passed, 11 known-defect cases failed, zero skipped. Compared failed test names with the original .NET 7 TRX: exactly the same failures.
- NuGet vulnerability audit, including transitive dependencies across all six projects: no known vulnerable packages reported at the time of the audit. This is not a general security clearance.
- Browser check against `RopeParison_Local`: rope grid loads; next-page interaction changes rows and page number; empty add-form submission displays required-field errors. No successful write was submitted.
- Database schema and data repairs are outside this change. No migration was applied.
- Read-only EF check: no pending model changes, no pending migrations, 164 ropes in the local database.

The GitHub workflow now reads `global.json`, runs the passing baseline, publishes only the web project, and uses supported v4 artifact actions and v3 Azure deploy action. It still deliberately excludes the known-defect tests from its passing gate; remove that filter when all defects are fixed. No workflow was dispatched and nothing was deployed to Azure. Confirm the App Service .NET 10 stack and Syncfusion license before deployment. Terraform adoption remains later work.

## References

- [Microsoft .NET 10 downloads](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [EF Core 8 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/breaking-changes)
- [EF Core 9 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes)
- [EF Core 10 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/breaking-changes)
- [Syncfusion version compatibility](https://blazor.syncfusion.com/documentation/common/how-to/version-compatibility)
- [Blazored.FluentValidation maintenance notice](https://github.com/Blazored/FluentValidation)
