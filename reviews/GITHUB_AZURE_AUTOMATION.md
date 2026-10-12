# GitHub and Azure automation

## Purpose and release boundary

Prepare authenticated production infrastructure plans, a reviewed apply path, and application deployment authentication. All work remains on `major-update` for one coordinated major production release. Infrastructure apply and application deployment opt-ins remain `false`; preparing identities does not upgrade hosting, migrate SQL or deploy an app. Commits are local stage checkpoints.

The owner account `ropeparison` authenticated through Git Credential Manager's device flow. `LukeBounds` is a collaborator with push access; this personal-account repository cannot grant that collaborator the organization-style Admin role. Both accounts can review environment jobs. Passwords, GitHub tokens and recovery codes are not stored in source or chat.

## GitHub controls

| Environment | Branches | Identity | Required approval |
| --- | --- | --- | --- |
| Terraform-Plan | `main`, `major-update` | `ropeparison-github-plan` | `ropeparison` or `LukeBounds` |
| Terraform-Apply | `main` | `ropeparison-github-apply` | `ropeparison` or `LukeBounds` |
| Production | `main` | `ropeparison-github-deploy` | `ropeparison` or `LukeBounds` |

All three environments disallow administrator bypass. Self-review is allowed so a single operator can manually dispatch and approve their own work. This is an explicit operator checkpoint, not independent two-person review. Branch policies allow branches only, not tags. These settings live in GitHub repository settings and must be reviewed if the repository is transferred or renamed; Terraform owns the Azure side only.

Environment variables `AZURE_CLIENT_ID`, `AZURE_TENANT_ID` and `AZURE_SUBSCRIPTION_ID` contain nonsecret identifiers. Only `Terraform-Plan` receives `ROPEPARISON_SQL_INPUTS_JSON`, encrypted using GitHub's environment public key from the existing reviewed private desired inputs. Do not regenerate this input from live Azure on every plan; doing so would silently adopt drift. Deliberate SQL administrator/firewall changes require reviewing both desired inputs and the declarations before updating the protected secret.

Repository variables `ROPEPARISON_INFRASTRUCTURE_APPLY_ENABLED` and `ROPEPARISON_PRODUCTION_DEPLOYMENT_ENABLED` are `false`. Enabling either is a separate release decision. Untrusted pull requests retain the credential-free infrastructure checks; they cannot invoke this manual credentialed workflow.

## Azure ownership and permissions

The owner-operated `infra/state-bootstrap` module manages the identities, federation, access grants, storage and lifecycle rules. It is excluded from automated applies. Federation uses issuer `https://token.actions.githubusercontent.com`, audience `api://AzureADTokenExchange`, and the exact `repo:ropeparison/rope-parison:environment:<environment>` subject for each identity.

- The planner has Reader on the existing RopeParison resource group and shared Log Analytics workspace, plus `Microsoft.Web/sites/config/list/action` scoped to the RopeParison web app. It can lease/write application state and upload private plan bundles. It has no ARM Contributor grant.
- The applier has Contributor separately on the existing RopeParison hosting plan, web app, Insights component, detector alert and shared SQL server. SQL scope covers the database/firewalls used by both RopeParison and SimpleBugTracker. It has Reader for shared references, application-state data access and read access to private plans. It has no role-assignment write permission or write grant on the whole application group/subscription.
- The app deployer has Website Contributor on the RopeParison web app only. It has no Terraform-state or SQL role grant.

These are scoped ARM permissions, not a guarantee that the planner cannot affect the app indirectly: provider refresh reads app settings, connection strings and publishing credentials, and Terraform state/plan files can retain them. Existing publishing basic authentication and shared-password authentication remain as adopted. Until the coordinated release removes obsolete publishing credentials and rotates existing secrets, anyone who can read state may obtain credentials capable of application/database access. Planning therefore requires the same environment approval protection as the write paths. Keep all credentialed workflow branches trusted; a workflow-authoring collaborator can change scripts on an allowed branch, so approval must include reviewing that source commit.

The planner and applier need Storage Blob Data Contributor for state leasing; it also permits replacing/deleting application state. Protect trusted workflow source and retain storage versioning. Neither identity has access to bootstrap state or permission to alter its own Azure roles/federation. Owner-operated bootstrap changes still need careful review; Terraform `prevent_destroy` is a configuration safeguard, not an Azure deny assignment.

## State and private plan storage

| Container/key | Use | CI access |
| --- | --- | --- |
| `tfproduction/production.tfstate` | Authoritative application/shared-SQL state | Planner and applier |
| `tfstate/bootstrap.tfstate` | Bootstrap/access state | None |
| `tfstate/production.tfstate` | Retained pre-separation copy | None; no longer authoritative |
| `tfplans/runs/<run>/<attempt>/<commit>/` | Private saved plan, JSON, diagnostic log and manifest | Planner writes; applier reads |

Application state is migrated with Terraform's backend migration, retaining lineage and resource identities. The old production key is a historical copy, not a second active owner: do not initialize new checkouts against it. Fresh checkouts use the committed `infra/backend.azurerm.hcl`; existing checkouts must reinitialize/migrate their backend rather than continuing from old cached metadata. Bootstrap continues using its unchanged backend configuration.

Current state blobs are never targeted for deletion. Previous versions for both original state keys and the new production key retain the existing 90-day creation-age policy. Plan bundles are eligible for deletion after seven days since their last modification, with previous plan versions eligible after seven days since creation. Account-wide 30-day soft deletion and asynchronous lifecycle execution mean this is not a strict seven-day erasure guarantee. A workflow's approval window is only 24 hours, independently of storage retention.

## Review and apply flow

`.github/workflows/infrastructure-production.yml` accepts manual dispatch only. The default is plan-only. It authenticates through `azure/login@v3` using federation, then Terraform uses that job's Azure CLI session for both provider and Azure AD backend access. The saved plan has no hardcoded planner client ID in backend configuration, allowing the separate applier session to consume it. No client secret or storage access key is supplied.

The planner reads pinned Terraform and locked providers, uses the protected desired SQL inputs, locks state and saves a binary plan. Terraform output/JSON are kept private, not uploaded as public GitHub artifacts or printed to Actions logs. The public summary lists only managed resource addresses/actions, source commit, private storage location and SHA256 manifest digest. Imports, moves, deletion, replacement, incomplete plans and changes outside the nine adopted managed resources are rejected; those operations require a separate owner-reviewed procedure.

Before approving `Terraform-Apply`, download and inspect the private plan with an authorized owner account. For example, substituting the exact run prefix from the job summary:

```powershell
az storage blob download-batch --account-name strpstatead052b7b2015 --auth-mode login --source tfplans --destination ./infra/private/review --pattern 'runs/<run>/<attempt>/<commit>/*' --output none
terraform show -no-color ./infra/private/review/runs/<run>/<attempt>/<commit>/production.tfplan > ./infra/private/review.txt
Get-FileHash ./infra/private/review/runs/<run>/<attempt>/<commit>/manifest.json -Algorithm SHA256
```

Compare the manifest digest with the workflow summary and review the actual proposed values in the private plan, including shared SQL impact. The public action-count summary alone is insufficient for approval. Keep downloaded artifacts out of Git, public logs and shared chat.

An apply job is created only for `main`, with dispatch input `apply=true` and the repository apply opt-in explicitly `true`. Approval happens after the plan job has completed. Apply downloads the same run/attempt/commit's bundle and verifies its manifest against the trusted preceding job output, every file digest, source commit, run/attempt, CLI version, provider lock hash, backend and 24-hour expiry. It applies that exact binary plan without replanning, targeting or variable replacement. Terraform's state lineage/serial checks reject a stale plan. Failures stop; no fallback apply is inferred. Both jobs share a non-cancelling concurrency group and native state leases.

Failure diagnostics remain private on the runner. Successful plan diagnostics are persisted with the private bundle; an unsuccessful plan/apply does not automatically publish diagnostic files. Investigate with an authorized owner instead of rerunning an unreviewed apply. No workflow run is dispatched or branch pushed by this stage.

## Verification and remaining release work

Local checks include both Terraform configurations, the existing fifteen helper/deployment checks and twenty-five saved-plan review/provenance/tampering checks. Workflow YAML checks cover manual-only triggers, branch/apply opt-ins, per-job identities/permissions, no public plan artifacts and trusted digest wiring. State migration, protected GitHub settings, encrypted input metadata, scoped Azure grants and no-change application/bootstrap plans are verified separately during setup.

Setup applied **24 additions, one storage-policy update and no deletions**, leaving 29 managed bootstrap resources. Production-state migration preserved lineage and all resource contents (nine managed resources); serial advanced from 2 to 3. Fresh application and bootstrap plans both report **no changes**, and the saved-plan guard accepted the real production plan. Azure CLI upload/download syntax and byte preservation were checked with a small synthetic blob in the private plans container using the owner's account. This checks storage transport, not the untested GitHub identities' token exchange. The fixture is covered by the private-plan expiry rule. No application tests were repeated for this infrastructure-only stage.

Credentialed GitHub runner execution and OIDC token exchange remain pending. Publish the branch only with user authorization and run credential-free checks first. GitHub requires a manually dispatched workflow to exist on the default branch: this new file cannot be dispatched on `major-update` before it is available on `main`. After the coordinated merge, run plan-only with both write opt-ins still false and approve only its planning environment; expect zero production changes. Thereafter manual dispatch may also select `major-update` for planning. Inspect the private plan and identity access before enabling either write opt-in. Do not remove the older publishing credentials until the replacement deployment path is verified and the coordinated release/rollback procedure is ready. Application hosting is still .NET 7 while the prepared application targets .NET 10; the runtime guard continues to block that mismatch.

Primary references: [GitHub environments and approvals](https://docs.github.com/en/actions/reference/workflows-and-actions/deployments-and-environments), [manual workflow/default-branch requirement](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow), [Azure GitHub federation](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect), [AzureRM backend authentication and container-scoped data access](https://developer.hashicorp.com/terraform/language/backend/azurerm), [AzureRM CLI support for service principals](https://github.com/hashicorp/terraform-provider-azurerm/blob/v4.81.0/website/docs/guides/azure_cli.html.markdown), [GitHub encrypted environment secrets](https://docs.github.com/en/rest/actions/secrets).
