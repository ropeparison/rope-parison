# Grouped rope edit submissions

Implemented 18 September 2026 on `major-update`.

## Behaviour

- One submitted edit creates a server-generated submission ID shared by all changed fields. Creation is a single transaction; an unchanged edit creates no rows.
- The submitting rope's rowversion must still match, so an edit based on stale rope data cannot create a partial or outdated submission.
- The moderation screen shows every field in a submission, with current and proposed values, and one approve/reject action. Category changes and their new required measurements can now be submitted and approved together.
- Approval applies all proposed fields to a candidate before validation, category normalization and recalculation. Saving the rope/calculations and deleting the entire submission commit together. Approval retains backend password enforcement and the moderator's rowversion check.
- Rejection removes the entire submission atomically without altering the rope or unrelated submissions. Calling the service with any member ID resolves the complete submission, so there is no separate single-field approval path for grouped edits.
- Existing suggestions have a null submission ID and remain independent single-field submissions. Historical rows are not guessed into groups.

## Migration and deployment

`20260918143951_GroupedRopeEditSubmissions` adds nullable `SubmissionId` and replaces the rope-only suggestion index with `(RopeId, SubmissionId)`. It preserves all existing suggestions. Applied only to `localhost\SQLEXPRESS / RopeParison_Local`; Azure and the original review snapshot were not changed.

The [incremental SQL script](grouped-submissions-migration.sql) is idempotent for databases already migrated through `20260918090336_RopeNameUniquenessAndConcurrency`. It is not a bootstrap script. New databases should use the complete migration chain. Apply the schema change before deploying this application, with old app instances stopped so they cannot continue individual-field moderation. The existing EF design-time factory requires an explicit `ROPEPARISON_MIGRATION_CONNECTION`.

The down migration drops the grouping identifier. Do not downgrade with pending grouped submissions: their grouping would be lost, making each row an independent legacy suggestion. Back up the database and resolve/export pending submissions before a planned downgrade.

## Verification

- All **105 tests passed**: 84 unit cases and 21 SQL cases, no failures or skips with SQL enabled.
- New tests cover a complete Single-to-Half category change, one-call batch persistence, no-op submissions, rollback after SQL writes during creation and approval, invalid-group rejection, isolation of unrelated submissions during rejection, and stale submission prevention.
- Existing approval authentication, rowversion, uniqueness, legacy approval and migration tests remain passing. The migration-refusal test now checks the specific integrity migration remains pending rather than assuming no later migrations exist.
- Full Release build passed with 95 existing warnings and zero errors; no pending EF model changes.
- Browser verification covers the legacy suggestion review table and category selection. SQL integration tests perform actual approval/rejection in disposable test databases; the supplied local suggestion is retained.

## Remaining work

Individual sign-in and durable submission/decision history remain deferred. Submission rows are still deleted on approval/rejection; grouping is not an audit log. A submission does not preserve a historical before-image: moderators compare against the current rope, and the reviewed rowversion prevents changes between review and approval. Sustained concurrent-load/deadlock testing remains separate work.

The sheath-percentage interpretation and bulk correction of stored thicknesses remain unresolved. This change does not repair that dataset, introduce HTTP API contracts, Terraform or Angular.
