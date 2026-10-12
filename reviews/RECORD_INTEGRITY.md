# Missing records, unique names and stale updates

Completed locally on `major-update`, 18 September 2026.

## Behavior

- Missing rope reads and writes report `RopeNotFoundException` instead of mapping an empty entity or silently succeeding. The edit page displays a clear missing-record message and clears the selection. Existing ropes still load for editing.
- Rope names are globally unique across brands. Names compare without leading/trailing ordinary spaces and without case differences, using `Latin1_General_100_CI_AS` (accent-sensitive). Writes trim input. SQL enforces uniqueness with persisted `NormalizedName` and `UX_Ropes_NormalizedName`; a preflight name check remains for friendly validation, but correctness does not depend on that check winning a race. Names are required, nonblank, and limited to 150 UTF-16 code units.
- Each rope has a SQL Server `rowversion`, copied into its DTO. Direct updates, verification, deletion and suggestion approval require the version the caller viewed. Stale or missing versions are rejected with a refresh-and-review message. EF also checks the version in the actual SQL update/delete, protecting the interval after the command loads the row. Calculation-only changes advance the parent version too. No automatic retry overwrites user edits.
- The edit form deep-copies a single loaded snapshot rather than fetching two possibly different versions. Backend password authorization remains ahead of persistence; public additions and proposed edits remain public. Suggestion creation does not overwrite the rope; approval must carry the moderator's current rope version.

## Database migration

Migration: `20260918090336_RopeNameUniquenessAndConcurrency`.

It checks for null, blank, oversized and duplicate names before changing the schema. Invalid legacy data stops the migration with an actionable message; the migration never renames or removes duplicate records. The down migration removes the index, check, computed column and rowversion, then restores the old nullable name column. Downgrading removes these protections and invalidates saved version tokens.

Applied only to `localhost\SQLEXPRESS / RopeParison_Local`. Azure and `RopeParison_Review_20260917` were not changed. The review snapshot deliberately retains its original schema. The app's startup does not automatically apply migrations.

[Reviewed incremental SQL script](rope-integrity-migration.sql) is idempotent for databases already migrated through `Mig10`; it is not a bootstrap script for an empty database. For a new database, apply the complete EF migration chain. Before deployment, review/back up the target database, stop writes, apply the migration, and deploy the matching application. Existing sessions should reload to obtain version tokens.

EF design-time tooling requires an explicit `ROPEPARISON_MIGRATION_CONNECTION` environment variable. It does not silently read the application's Azure connection configuration. Example, from the repository root with the .NET 10 SDK and matching `dotnet-ef` available:

```powershell
$env:ROPEPARISON_MIGRATION_CONNECTION = 'Server=localhost\SQLEXPRESS;Database=RopeParison_Local;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;'
dotnet ef database update --project RopeParison.Data --startup-project RopeParison.Data --context DataContext
```

## Verification

- All **97 tests passed**: 82 unit cases and 15 SQL cases, zero failures/skips with SQL enabled.
- SQL tests now build disposable databases through the real migration chain, rather than `EnsureCreated`.
- Tests cover missing records, duplicate inserts after both callers pass preflight, duplicate renames, stale saves preserving newer data and calculations, stale moderation, missing version tokens, successful fresh saves/deletes, and EF's SQL concurrency predicate.
- A migration rollback/refusal test inserts legacy duplicates and verifies that migration fails while retaining both records.
- Full Release build passed, with existing code warnings remaining.
- Browser checks verified missing-ID feedback and loading an existing rope's edit form.

Stored thickness values are not backfilled by this migration. Grouped edit approval has since been implemented; see [grouped submission notes](GROUPED_EDIT_SUBMISSIONS.md). Individual sign-in and durable moderation history remain separate work. This addresses F10/F11 and the stale-write portion of F12 in the original review; it does not close every moderation/history concern in F12.

Implementation references: [EF concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency), [applying migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying).
