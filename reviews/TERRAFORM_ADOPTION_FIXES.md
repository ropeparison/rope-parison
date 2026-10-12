# Terraform adoption review fixes

Completed 12 October 2026 on `major-update`, following [the adoption review](TERRAFORM_ADOPTION_REVIEW.md).

## Changes

| Finding | Resolution | Remaining work |
| --- | --- | --- |
| R1: live inventory overwrites desired SQL inputs | Initialization refuses an existing destination; exclusive file creation also prevents concurrent overwrite. Normal plans use stable reviewed inputs. Separate comparison reports drift without modifying inputs. | Trusted CI needs a protected source for reviewed inputs. |
| R2: .NET 10 artifact can deploy to the .NET 7 host | Deployment is explicitly gated to enabled production runs on `main`; artifact, Terraform and live Azure runtime must all agree before deployment. Wrong subscription, missing stack or mismatched runtime fails closed. | Coordinate production runtime, artifact, migrations and rollback; configure federation and environment protections before enabling deployment. |
| R3: fixed Windows Azure CLI path | Both inventory helpers share PATH discovery, Windows install fallbacks, and optional executable injection. | Actual Linux execution will be verified when the workflow runs; no remote CI run was dispatched here. |
| R4: missing inventory on fresh checkout | The dependency inspector reads its own resource list and creates its output folder. A new restore helper can recover recorded SQL inputs from initialized production state without capturing live values. Instructions distinguish first use, ordinary plans, and deliberate input updates. | Review restored state provenance; prefer an existing protected desired-input copy when available. |
| R5: unbounded previous state versions | A reviewed, narrowly scoped Azure lifecycle policy was applied: only previous block-blob versions older than 90 days from creation under the two reserved state prefixes. | Monitor asynchronous expiry and retain an agreed recovery process. No destructive recovery rehearsal was performed. |

## Stable inputs and drift

`Write-SqlBaselineInputs.ps1` is now exclusively an initializer for adoption. It rejects an existing desired-input file before reading inventory, and writes with `FileMode.CreateNew` to prevent a race from overwriting another initializer's result. It has no overwrite switch. Intentional changes use a separate private candidate input, a reviewed Terraform plan, and deliberate promotion after apply; normal inventory capture is never the configuration source for subsequent plans.

`Test-SqlBaselineDrift.ps1` compares the observed administrator and client IP fields with reviewed desired inputs. It emits changed field paths rather than values. Unexpected rule names/counts, an altered Azure-services rule and mismatched server/tenant metadata are rejected for review. None of these operations changes SQL infrastructure or data.

`Restore-SqlBaselineInputs.ps1` reads the initialized production backend, validates expected SQL resources/server identity, and extracts only the SQL input metadata. It neither queries live SQL nor writes/prints the full state. It refuses an existing destination. This restores recorded state, not an independently versioned configuration history; another operator's refresh/apply may have changed that state. The runbook therefore requires provenance/value review and prioritizes a protected reviewed input copy. Future trusted CI should receive approved inputs, not restore or regenerate them before every plan.

## Release guard

The application workflow continues building and testing. It also runs the offline infrastructure helper and runtime regressions on its Ubuntu runner. Deployment now requires `main` and repository variable `ROPEPARISON_PRODUCTION_DEPLOYMENT_ENABLED=true`. Leave that variable unset/false until the production release is ready.

The workflow replaces its publish-profile path with an OIDC login using protected `AZURE_CLIENT_ID`, `AZURE_TENANT_ID` and `AZURE_SUBSCRIPTION_ID` inputs. Only the deploy job receives `id-token: write`. The deploy job checks the signed-in subscription and retrieves the live host stack before the runtime guard. There is no publish-profile fallback. The guard checks the framework-dependent artifact's ASP.NET/Core target, the declared Terraform stack and Azure's live stack. A runtime upgrade in HCL without a corresponding live change cannot pass the guard.

No GitHub settings, Azure federated identities, new access grants or application deployments were created by these fixes. OIDC/environment setup is still the next stage. Existing production hosting remains .NET 7.0; the application targets .NET 10. The unsafe deployment path is now blocked in the changed workflow, but the actual runtime upgrade remains a coordinated release task. These locally committed workflow changes take effect in GitHub only after they are pushed and used by the workflow. After validating the OIDC replacement, revoke the former publish-profile credential separately; it was not accessed or revoked here. [Azure Login documentation](https://github.com/Azure/login)

## Applied retention policy

The reviewed plan added exactly `azurerm_storage_management_policy.state_versions`, with **1 addition, 0 changes, 0 deletions**. Applying the saved plan produced the same counts. The existing four state-bootstrap resources remained unchanged; bootstrap state now manages five resources. The subsequent normal plan reported no changes.

The policy matches block-blob prefixes `tfstate/production.tfstate` and `tfstate/bootstrap.tfstate` and declares only a `version` deletion action after 90 days from creation. No `base_blob` or snapshot action is present. Azure prefix matching also covers any future names beginning with those strings, so the runbook reserves those prefixes for these backend keys. Before activation, 24 current/previous versions were recorded and none of the previous versions was older than 90 days.

Current state is preserved. Previous-version age is measured from creation, so a long-lived current version can become immediately eligible once superseded; this is not 90 days of guaranteed retention after supersession. Expiry is asynchronous, and the existing 30-day soft-delete protection applies from deletion of a version. No manual version deletion or retention-window reduction was performed. [Microsoft lifecycle policy behavior](https://learn.microsoft.com/en-us/azure/storage/blobs/lifecycle-management-policy-delete), [Microsoft versioning and soft deletion](https://learn.microsoft.com/en-us/azure/storage/blobs/versioning-overview)

## Verification

- Fifteen offline checks passed for cold inspection without prior inventory, CLI injection, refused overwrites, unchanged desired-input bytes after drift checks, changed firewall/administrator detection, restoration from recorded state, rejected mismatched state, and runtime mismatch/missing/matching scenarios.
- The same suite passed from the actual repository. PowerShell sources parsed successfully; workflow YAML and deployment gate/authentication/step-order checks passed. The latter used a scratch-only YAML parser, not a new project dependency.
- Real production state restoration to an ignored verification file matched live SQL metadata. Existing canonical desired inputs were compared with live metadata and remained byte-for-byte unchanged.
- The application/shared SQL Terraform plan reported **no changes**. No app/SQL apply was required or executed for these fixes.
- Both configurations passed format/validation checks. The state-storage plan reported **no changes** after the single retention-policy apply.
- No SQL mutation, EF migration, host runtime change, GitHub workflow dispatch or application deployment was performed. Existing application tests were not repeated because no application source changed.

## Next stage

Add credential-free infrastructure PR checks, then configure restricted GitHub-to-Azure federation, protected reviewed SQL inputs, trusted plan jobs and a reviewed apply path. Keep production deployment disabled until the coordinated .NET 10 runtime/schema/application release is ready. Shared SQL remains owned by this single Terraform configuration. Angular follows those infrastructure and release prerequisites.

Changes are recorded in local commits on `major-update`; private input files, plans, JSON/state exports and verification metadata remain ignored.
