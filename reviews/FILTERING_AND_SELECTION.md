# Filtering and selection fixes

Implemented 18 September 2026; browser verification and callback correction completed 3 October 2026 on `major-update`.

## Legacy filtering (F13)

Both public filter services exclude unverified ropes unless `IncludeUnverified` is explicitly true. The legacy `/ropeList` page has an inclusion checkbox, category dropdown and working Apply/Reset buttons. Reset restores verified-only defaults.

The legacy category string is parsed before building the SQL predicate, replacing an untranslatable enum `ToString()` call. Matching accepts trimmed, case-insensitive enum names. Blank means unrestricted; an unknown or numeric category string matches nothing. All measurement predicates continue to execute on SQL Server.

## Selection (F18)

For Filter A, `RopeIds` and `Categorys` now distinguish an omitted restriction (`null`) from an explicitly empty selection (zero matches). Nonempty lists restrict results to the chosen values. Empty brands retain their existing meaning of all brands.

The selector supplies an explicit category list in category mode and an explicit rope list in individual mode. Clearing either selection therefore shows no ropes. Individual mode includes explanatory text. Mode changes refresh available ropes, prune unavailable IDs and immediately publish the new results. Existing choices are retained per mode when still valid.

Changing unverified inclusion refreshes the chooser in either mode and removes unavailable selected IDs. Category buttons, checkboxes and multiselects refresh after their bound values update. Browser verification caught the multiselect's old `ValueChange` callback running before its bound selection was updated; it now uses `@bind-Value:after`. Selection callbacks and initialization return awaited tasks.

## Verification

- The regression suite passed all **110 cases**, including **26 SQL cases**, with zero failures/skips when SQL tests were enabled. Five added SQL cases cover the filter/selection contract and real SQL query execution. The saved full-suite result is from 18 September; the final callback correction was built and browser-tested on 3 October.
- Full Release build after the callback correction passed with 95 existing warnings and zero errors.
- Legacy browser checks showed 10 Half ropes after applying the category filter, plus explicit inclusion of the supplied unverified records.
- Main selector browser checks: initial Single selection shows 99 ropes; clearing categories shows zero; switching to empty individual selection shows zero; selecting unverified Genesis shows one; disabling unverified inclusion removes Genesis and shows zero; returning to category mode restores the saved Single selection and 99 results.
- Checks use the existing local dataset read-only. No schema migration was required.

These changes resolve F13 and F18. Other async/chart refresh work under F14 remains separate. Terraform inventory is the next planned stage, followed by Angular. Individual sign-in and durable moderation history remain deferred; stored thickness repair awaits confirmation of the sheath-percentage interpretation.
