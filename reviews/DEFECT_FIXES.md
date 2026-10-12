# Defect fixes — 18 September 2026

Branch: `major-update`. Builds on the .NET 10 upgrade. No Azure deployment or production data changes.

## Changes

- **F04:** the validator rejects invalid optional sheath percentages/slippage, non-finite measurements, invalid category/brand selections, and missing or nonpositive required drop counts. Add, update, and edit-suggestion commands validate before calling persistence. Approval validates the resulting rope before committing. UI write failures display feedback instead of escaping the handler.
- **F06:** validated writes normalize category-dependent measurements before calculating. Approval uses that same path.
- **F07:** edit comparison uses category values, not DTO reference identity.
- **F08:** radial sheath thickness uses half the outer/core diameter difference. The calculated-values mapper now returns stored thickness (it previously omitted it). This retains the existing area-fraction assumption; it does not establish whether manufacturer sheath percentages are mass fractions.
- **F05:** approval verifies that the suggestion exists and belongs to the selected rope, loads that rope, applies the proposed change, validates/normalizes/recalculates, then persists the rope and deletes the suggestion within one serializable transaction. Exceptions dispose the uncommitted transaction. Repeated approval is an explicit failure. Approval no longer commits first and recalculates through a second command.
- **F09:** DTO mapping preserves the rope's verified flag; integration tests also verify approval leaves the persisted flag intact.
- The workflow runs all ordinary regression cases without the former known-defect filter. SQL integration tests require explicit opt-in on a host with SQL Express.

## Evidence

- Regression suite: 40 unit tests and 7 SQL Server tests pass, 47 total with zero failures/skips when SQL tests are enabled.
- SQL tests use generated disposable databases only. Fault injection after SQL execution verifies rollback of the edit, calculations, and suggestion deletion together.
- Additional SQL assertions cover mismatched and missing suggestions, repeat approval, invalid candidate rejection, brand edits, category normalization, and DTO mapping.

## Remaining work

Stored thickness values have not been backfilled; the 105 affected rows identified in the supplied snapshot still need an explicit repair once the sheath-percentage interpretation is settled. Updated/new ropes receive the corrected formula.

The single-property suggestion design cannot atomically approve a category change plus all newly required measurements. Unsafe intermediate candidates are now rejected instead of saved. A grouped suggestion model or complete validated administrative update is needed for those cases.

General update concurrency tokens, database uniqueness, backend authorization, missing-ID reads, other structural/UI issues, and durable moderation history remain in the original review plan. Serializing approval is not a replacement for these. Simultaneous conflicting approvals can return a database contention error and roll back; a user can refresh before retrying. No concurrent-load test was performed here.

The completed work is recorded in local commits on `major-update`; existing user files remain in place.
