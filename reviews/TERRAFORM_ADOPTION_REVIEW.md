# Terraform adoption review

Reviewed 12 October 2026 on `major-update`. Scope: `infra/`, both backends, helper scripts, saved adoption evidence, recovery instructions and the application deployment workflow's interaction with the preserved host settings.

The import baseline is sound: the recorded apply imported nine exact existing resources with zero additions, changes or deletions. Both subsequent remote plans reported no changes. There is one owner for the shared SQL resources; separate production/bootstrap state keys; Entra authorization; versioning and soft deletion; resource deletion safeguards; and ignored private state/plan artifacts. These are useful protections, but a no-change plan does not establish complete drift coverage or a tested recovery process.

This review repeated Terraform format and validation checks successfully for both configurations. It reproduced the input-overwrite and missing-inventory issues below with synthetic metadata and a stub CLI in an isolated scratch directory. It made no Azure requests, resource changes, state changes, application deployments or database operations. The live baseline evidence cited here is from the completed adoption earlier on the same date; it was not refreshed during this review.

## Resolution status

The fixes completed later on 12 October are recorded in [Terraform adoption fixes](TERRAFORM_ADOPTION_FIXES.md). R1, R3 and R4 are fixed and verified; R5's 90-day previous-version policy is applied. R2 now has an explicit deployment/runtime gate; the actual production .NET 10 release and federation/environment setup remain pending. The findings and initial next-stage sequence below preserve the reviewed baseline rather than current unresolved status.

## Findings and resolution plan

### R1 — P1: Routine inventory refresh can hide SQL configuration drift

Evidence: `infra/Write-SqlBaselineInputs.ps1:20–29`, `infra/README.md:30–32`, and `infra/STATE_OPERATIONS.md:25–27`.

The documented routine refreshes live metadata and unconditionally writes it over `private/sql-baseline.tfvars.json`. If someone widens a client firewall range or changes the Entra administrator through the portal while keeping expected names, the next routine overwrites the previously desired values with those changes before planning. The plan can then report no changes instead of proposing reconciliation. The existing check catches changed firewall names/counts, but not changed ranges or administrator identity. The prose warns against silently accepting drift, but the command sequence does exactly that for these fields.

Reproduction: start with a synthetic client IP of `192.0.2.1`, change the captured metadata to `192.0.2.3`, and rerun the writer. The existing desired input is replaced without confirmation or a reported difference.

Resolution: separate inventory capture from desired configuration. Make the adoption writer initialization-only, refusing an existing destination unless an explicit update mode is requested. Ordinary plans must consume stable reviewed inputs, with live inventory used for comparison rather than regeneration. Supply those inputs to trusted CI through a defined protected source; updating them must be a reviewable operation. Confirm that changing the live administrator/ranges leaves the desired inputs intact and produces drift in a normal plan. Do this before trusted plan/drift automation.

### R2 — P1 before deployment: The new application and preserved host runtime differ

Evidence: `infra/main.tf:41`, `RopeParison/RopeParison.csproj:4`, and `.github/workflows/main_ropeparison.yml:7–10,30–31,52–59`.

Terraform intentionally preserves .NET 7.0 for adoption. The branch targets .NET 10 and the existing workflow publishes a framework-dependent application, then deploys on pushes to `main` or a manual dispatch. There is no runtime compatibility check in that workflow. Merging this branch and allowing deployment before coordinating hosting can deploy an application that requires .NET 10 to a host configured for .NET 7. A later Terraform apply would also retain/reassert the configured 7.0 stack until the HCL is deliberately updated. This is a release blocker, not evidence that the import damaged the existing service.

Resolution: keep production deployment gated while preparing the release. Verify available Linux App Service runtimes, generate a reviewed hosting change, and coordinate it with the .NET 10 artifact, pending schema migrations, backup/recovery and smoke checks. A host stack change can restart the existing application, so do not apply it as an isolated early step. Define rollback for both runtime and artifact. Finish infrastructure CI first, then handle this as a separate production release. [Microsoft App Service .NET configuration](https://learn.microsoft.com/en-us/azure/app-service/configure-language-dotnetcore)

### R3 — P2: Dependency inspection cannot run on the intended Linux CI runner

Evidence: `infra/Inspect-RopeParisonDependencies.ps1:2`.

The helper invokes a fixed `C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd` path. It fails on an Ubuntu GitHub runner and Windows installations using another path even when `az` is available. `Inventory-Azure.ps1` already discovers `az` through PATH, so the two helpers have inconsistent portability.

Resolution: use the same CLI discovery or a shared helper, allow explicit executable injection for verification, and fail with a clear prerequisite message. Confirm that the scripts work with PowerShell and Azure CLI on the actual CI runner. This affects inventory automation, not Terraform's existing remote backend.

### R4 — P2: The documented routine cannot initialize a fresh checkout

Evidence: `infra/Inspect-RopeParisonDependencies.ps1:20`, `infra/README.md:25–32`, and `infra/STATE_OPERATIONS.md:20–27`.

Both command sequences call the dependency inspector without first generating `private/inventory/azure-inventory.json`. That ignored file exists only in the current local workspace. The inspector reaches `Get-Content` and stops before writing the SQL input metadata on a new checkout. Offline reproduction with a stub CLI confirmed the missing-file failure. The routine also needs stable desired inputs once R1 is addressed, rather than relying on a fresh live capture as their authoritative source.

Resolution: document and implement a clear first-use sequence distinct from ordinary planning. Validate inventory prerequisites before making CLI calls; generate the initial inventory when explicitly requested, or remove the inspector's dependency on that file. Establish a portable, protected source of reviewed SQL inputs. Verify the instructions from an isolated checkout without copying local ignored artifacts.

### R5 — P2: Old state versions have no automatic expiry policy

Evidence: `infra/state-bootstrap/main.tf:45–52`; no storage management policy exists in the module.

Thirty-day soft deletion governs recovery after deletion; it does not expire undeleted previous blob versions. The current account can retain old state versions indefinitely. As state can contain connection strings and app settings, this retains old secret-bearing snapshots and allows version storage to accumulate. No current cost or exposure incident was observed.

Resolution: choose a recovery retention period and add a narrowly scoped lifecycle policy for previous versions of the two state blobs. Preserve the current blobs. Explain that previous-version expiry and soft deletion are separate mechanisms, including their combined retention behavior. Review the deletion implications before applying a policy; this review did not delete versions or configure lifecycle rules. [Microsoft blob versioning and lifecycle guidance](https://learn.microsoft.com/en-us/azure/storage/blobs/versioning-overview)

## Intentional limits to carry forward

- App settings, connection strings, authentication settings, tags and two network defaults remain excluded from Terraform drift checking. Secrets may still be present in provider state. Establish an explicit owner and recovery source for those fields before claiming complete infrastructure recovery. [HashiCorp lifecycle behavior](https://developer.hashicorp.com/terraform/language/meta-arguments/lifecycle)
- SQL's preserved `0.0.0.0` Azure-services rule admits network connection attempts from Azure resources in other subscriptions, subject to SQL authentication. It is not confined to these two apps. Any tightening needs a connectivity plan for both consumers; removing it during adoption would have risked an outage. [Microsoft SQL firewall scope](https://learn.microsoft.com/en-us/azure/azure-sql/database/firewall-configure?view=azuresql)
- State access is currently a single owner's account-scoped grant. CI federation, additional authorized recovery access and production environment protections have not been configured or inspected. Do not infer required reviewers exist merely from the workflow's `Production` environment label.
- `prevent_destroy` is not an Azure deletion lock. State recovery and SQL backup recovery have not been rehearsed. LRS storage stays within one region; the existing SQL password is not recoverable from this configuration.
- The completed configuration and runbooks are recorded in local commits on `major-update`; private artifacts remain excluded. Publishing and protected CI setup are separate next steps.

## Next stage

The next stage is reproducible infrastructure automation, with the helper/input fixes as its first step.

1. Resolve R1, R3 and R4 locally: stable reviewed inputs, portable helpers, distinct first-use instructions. Validate the cold-start path and meaningful drift detection. Record R2 as a pre-deployment gate and choose the R5 retention policy separately.
2. Add credential-free PR checks for both configurations: formatting, backend-disabled initialization with the lock file, and validation. Untrusted PRs must not receive Azure credentials, remote state or protected inputs. Include an application build/test check on the working branch so failures are caught before the existing `main` deployment trigger.
3. Prepare GitHub OIDC access and trusted plan jobs. Restrict trust to the intended repository/ref/environment, grant only required resource-read and state-access permissions, and account for shared monitoring dependencies and secret-bearing App Service reads. Ordinary Reader permissions must not be assumed sufficient for every provider operation. Production apply requires a separate reviewed permission scope and real environment protection.
4. Add a reviewed saved-plan apply path and concurrency control. Apply the approved plan for the same commit and inputs; do not regenerate it after approval. Keep raw state and secret-bearing plan/JSON files away from public logs and untrusted artifacts. Keep automatic production apply disabled until the path is verified.
5. Confirm that the trusted production and bootstrap plans remain empty, then commit/review the infrastructure changes. Plan the coordinated .NET 10 production release next. Angular follows these infrastructure and release prerequisites.

Exit criteria: checks work from a fresh checkout; live SQL drift cannot rewrite desired inputs; trusted jobs use federation without long-lived credentials; reviewed plans control applies; shared SQL ownership is preserved; private artifacts remain excluded; and the .NET runtime mismatch cannot cause an unintended deployment.

Reference for the federation stage: [Microsoft GitHub-to-Azure OIDC guidance](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect).
