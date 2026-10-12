# Backend authorization and deferred sign-in

Decision recorded: 18 September 2026. Branch: `major-update`.

## Implemented now

The existing shared-password mechanism remains, but checks are enforced at the application command boundary in `RopeService`. Each protected command requires an explicit password argument and checks it before validation, database reads or writes. There is no password-free overload for these commands.

| Command | Required existing configuration |
| --- | --- |
| Verify a rope | `Security:Password:Verify` |
| Remove a rope | `Security:Password:Delete` |
| Approve or reject an edit suggestion | `Security:Password:ApproveEdit` |
| Directly update a rope | `Security:Password:ApproveEdit` |

Empty, missing, whitespace-only or incorrect passwords are denied with `UnauthorizedAccessException`. Empty/missing configured passwords cannot authorize anything. Password values are not included in rejection messages or logged by this change.

Blazor handlers now submit the entered password to these commands and display the existing incorrect-password feedback on rejection. The UI no longer owns the permission decision. Read operations, unverified additions and proposed edits remain public.

The existing `IPasswordService` registration supplies the required backend dependency. Repository/data services remain trusted persistence implementation details: future HTTP endpoints and other frontends must call the application commands, not expose those lower-level services directly.

## Required improvement — explicitly deferred

Replace shared passwords with individual sign-in and backend authorization policies later. The user has deferred implementing sign-in; no provider is chosen or configured by this change.

Google accounts are a supported option, alongside Microsoft accounts/Entra ID or application-managed accounts. See [Microsoft's external-provider guidance](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/social-without-identity?view=aspnetcore-10.0).

Acceptance criteria for that future work:

1. Select an identity provider and restrict administration to explicitly approved users/roles. Signing in with an arbitrary Google or Microsoft account must not itself grant administration rights.
2. Replace command password parameters with authorization based on the trusted authenticated caller and named permissions; enforce the same rules for Blazor and the future Angular/API entry points.
3. Keep public browsing and decide separately whether submissions should require an account.
4. Remove shared-password handling and checked-in fallback credentials, configure secrets outside source control, and record who approved or deleted records.
5. Test anonymous, signed-in but unauthorized, and authorized callers, including session expiry and revocation.

This interim move does not introduce individual accountability, account recovery, MFA, session management or rate limiting. The previously documented shared-password/configuration limitations remain; F03 is partially addressed, not closed. Authentication modernization is required before exposing administrative functionality through a new public API.

## Verification

Authorization regression cases invoke `IRopeService` directly with missing, empty, incorrect, correct and other-operation passwords. Denied commands make no persistence calls. The tests exercise the real configuration-backed password service with synthetic in-memory credentials. They also verify that unconfigured admin credentials do not prevent public submissions. Existing SQL approval/rollback tests continue to supply explicit synthetic edit credentials.

Release build passed. All 89 cases passed with SQL tests enabled (82 unit cases, 7 SQL cases; zero failures/skips). A browser check of the existing local pending suggestion with no password showed the incorrect-password toast and left the proposal pending.
