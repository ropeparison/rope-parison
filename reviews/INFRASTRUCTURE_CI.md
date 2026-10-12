# Infrastructure CI stage

Completed locally on 12 October 2026 on `major-update`.

## Implemented

`.github/workflows/infrastructure-checks.yml` adds one Ubuntu job, **Validate infrastructure**, for every pull request, pushes to `main` and `major-update`, and manual dispatch. There are no path filters, so unrelated PRs can still complete the check if it is later required by branch protection. New runs cancel superseded validation runs; this workflow performs no applies.

The job reads the exact Terraform version from `infra/.terraform-version` (currently 1.16.5), validates its format, and installs it with HashiCorp's setup action. The wrapper is disabled so command exit codes are preserved. It runs the same `infra/Test-Infrastructure.ps1` entry point available locally.

The script checks formatting throughout `infra/`, initializes both the application/shared SQL and state-bootstrap configurations with `-backend=false` and `-lockfile=readonly`, validates both, then runs the fifteen offline helper/deployment regressions. Each initialization uses a separate fresh temporary `TF_DATA_DIR`, even in a checkout already connected to production. The script restores any previous `TF_DATA_DIR` and cleans up only its checked temporary directory on success or failure.

Permissions are limited to `contents: read`; checkout does not persist credentials. The workflow references no Azure login, secrets, OIDC permission, production environment, SQL input file, remote state, plan/apply step or uploaded artifact. It downloads the pinned Terraform tool and locked provider from public distribution services. This is configuration and regression validation; it does not inspect live Azure drift or prove production runtime/schema compatibility.

## Verification

- The complete check script passed against a fresh copy containing only public source/configuration, with Azure CLI authentication disabled and an empty Azure CLI configuration directory. Both modules initialized using AzureRM 4.81.0 from their existing lock files and validated successfully; all fifteen helper/deployment checks passed.
- Workflow YAML, event/branch triggers, permission limits, pinned tool setup and entry-point wiring passed local checks. The embedded version step parsed and produced exactly `version=1.16.5`.
- A failed Terraform validation was injected to verify the check stops immediately, retains existing metadata and restores the caller's `TF_DATA_DIR` on failure.
- The entry point was also verified in the actual checkout, with production backend metadata and provider lock files checked for unchanged contents.
- Verification was local Windows/PowerShell 7 execution. GitHub's Ubuntu runner execution remains pending until the branch is pushed; no workflow run was dispatched here.

No Azure resource/state changes, SQL migrations or application deployments are part of this stage. Existing application tests were not repeated because application code did not change. The stage is committed locally after verification; unrelated `CLAUDE.md` and private artifacts remain excluded.

## Local use

From the repository root, with the pinned Terraform CLI available on PATH:

```powershell
./infra/Test-Infrastructure.ps1
```

An explicit executable can be supplied with `-TerraformPath`. Providers may need public registry access on the first run. The check deliberately does not use `backend.azurerm.hcl` or private SQL inputs. For an actual Azure plan, use the separate instructions in `infra/README.md` with reviewed desired inputs and authorized state access.

## Following stage and release policy

Configure narrowly scoped GitHub-to-Azure federation, a protected source for reviewed SQL inputs, trusted infrastructure plans and a reviewed apply path. No CI identity or new access grant has been provisioned yet. Keep untrusted PR validation separate from credentialed operations.

Each completed stage is validated and committed locally on `major-update`. The agreed application, runtime, infrastructure and front-end updates form one coordinated major production release. Preparing a workflow or making a stage commit does not trigger an early application deployment. Hosting, database migrations, artifact compatibility and rollback must be coordinated before production deployment is enabled.

References: [HashiCorp setup action](https://github.com/hashicorp/setup-terraform), [Terraform backend-disabled initialization and readonly lock file](https://developer.hashicorp.com/terraform/cli/commands/init), [GitHub workflow syntax](https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax).
