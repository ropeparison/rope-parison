# Regression tests

Use the .NET 10 SDK in `global.json`. See `../reviews/DOTNET10_UPGRADE.md` for the local SDK installation instructions.

```powershell
# Unit tests; SQL tests are explicitly skipped unless opted in:
dotnet test RopeParison.Tests/RopeParison.Tests.csproj --configuration Release

# Include SQL Server integration tests on Windows with local SQL Express:
$env:ROPEPARISON_SQL_TESTS = '1'
dotnet test RopeParison.Tests/RopeParison.Tests.csproj --configuration Release
Remove-Item Env:ROPEPARISON_SQL_TESTS
```

The SQL tests use Windows integrated authentication to `localhost\SQLEXPRESS`. Each creates its own `RopeParison_Tests_<GUID>` database and removes that database on completion. The login needs permission to create databases. No connection-string override is accepted, and no BACPAC, Azure, app database or review database is used. If a test process is forcibly terminated, a database with that prefix may remain for manual cleanup.

## Current result — 3 October 2026

**84 unit cases and 26 SQL cases pass (110 total, zero failures, zero skips with SQL opted in).** The original 11 failing cases now pass; their assertions remain and `Finding` traits link them to `../MAJOR_UPDATE_REVIEW.md`. All `KnownDefect` exclusions have been removed, including from the build workflow. Standard hosted CI does not have this machine's SQL Express and therefore skips the 26 SQL cases explicitly.

Coverage includes geometry/density, clearing stale optional calculations, finite numeric inputs, required category-specific drop counts, required-field and duplicate-name validation, validation at add/update/suggestion boundaries, category normalization, equivalent categories, and approval recalculation/error propagation.

SQL tests exercise the real production service and SQL Server provider: successful approval, missing suggestion, repeat approval, mismatched target, invalid proposed value, brand changes, category cleanup, verification/thickness DTO mapping, and rollback. The rollback interceptor throws after SQL commands have executed but before the enclosing transaction commits; fresh reads verify that rope measurements, calculations, and suggestion deletion all rolled back.

## Limits

The initial baseline was 15 passing and 11 failing cases. Additional tests were added with the fixes; no failed assertion was weakened to accept a known defect.

The thickness test assumes sheath percentage is a cross-sectional area fraction, matching the existing calculation model. Manufacturer mass-vs-area interpretation still needs resolution before a bulk data repair. Existing stored values are not migrated by these tests or fixes.

Backend authorization, SQL name uniqueness and rowversion concurrency checks are now covered. Concurrent-load testing and durable moderation history remain follow-up work. Grouped submissions now support category changes and dependent measurements atomically. Two unit cases and six SQL cases cover grouping, no-op edits, complete category approval, rollback, independent rejection, invalid candidates and stale submissions. See [grouped submission notes](../reviews/GROUPED_EDIT_SUBMISSIONS.md).

Backend authorization adds 42 cases covering operation-specific passwords, denials before persistence, unconfigured credentials, and continued public submissions. See [authorization notes](../reviews/BACKEND_AUTHORIZATION.md).

The SQL fixture now applies the actual migration chain. Eight integrity cases cover missing records, duplicate-name races, stale writes/moderation, fresh deletion, SQL concurrency enforcement and migration refusal for legacy duplicates. See [record integrity notes](../reviews/RECORD_INTEGRITY.md).

Five additional SQL cases cover verification defaults, category query translation for every category, normalized/unknown category text, omitted versus empty selections, and verification enforcement for explicit rope IDs. Browser checks cover actual selector callback ordering, empty selections, mode switches and pruning unverified selected ropes. See [filtering and selection notes](../reviews/FILTERING_AND_SELECTION.md).
