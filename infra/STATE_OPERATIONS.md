# Terraform state operations

## Backend and ownership

The Azure Blob backend is in subscription `RopeParisonSubscription` (`ad052b7b-2015-4933-93eb-0b857cabccae`), resource group `RopeParisonTerraformState`, account `strpstatead052b7b2015`, private container `tfstate`.

| Configuration | Blob key | Managed resources |
| --- | --- | --- |
| `infra/` | `production.tfstate` | Four RopeParison app resources and five shared SQL resources |
| `infra/state-bootstrap/` | `bootstrap.tfstate` | State resource group, storage account, container, owner access grant and previous-version retention policy |

Use only the default Terraform workspace. Do not point both configurations at the same key or import the same Azure resource into another state. Future apps reference this shared SQL foundation rather than taking duplicate ownership.

Backend HCL contains IDs and names, not credentials. Both configurations use Azure CLI sign-in and Entra data-plane authorization. Shared Key and anonymous blob access are disabled. The existing owner has Storage Blob Data Contributor at the storage-account scope; Azure resource-management permissions are separate. A future CI identity will need its own reviewed permissions and workload identity federation. CI/OIDC is not configured yet.

## Routine changes

Sign in to the configured tenant and confirm the subscription. From `infra/`:

```powershell
az login --tenant a2155124-112f-4b17-a715-0ca032e18b28
terraform init '-backend-config=backend.azurerm.hcl'
terraform fmt -check -recursive
terraform validate
terraform plan '-input=false' '-var-file=private/sql-baseline.tfvars.json' '-out=private/change.tfplan'
```

On first use, restore the reviewed private SQL input file from protected storage, or run `Restore-SqlBaselineInputs.ps1` after backend initialization to recover the recorded production SQL settings. Review state provenance and recovered values. Existing desired inputs cannot be overwritten by either initialization helper. Ordinary plans consume stable inputs; do not run `Write-SqlBaselineInputs.ps1` to accept live settings before planning. Optional inventory refresh followed by `Test-SqlBaselineDrift.ps1` reports differences without changing those inputs. See the README for deliberate candidate-input changes. Review the actual plan before applying its saved file. SQL capacity, retention, firewall and administrator changes affect RopeParison and SimpleBugTracker and need coordination with both consumers.

For state-storage changes, use `state-bootstrap/` and its own `backend.azurerm.hcl`; no SQL inputs are needed. Do not apply the original bootstrap creation plan again. Import blocks in `infra/imports.tf` remain as an adoption record and are idempotent once state contains those addresses. `Test-AdoptionPlan.ps1` applies only to the initial nine-import plan, not an ordinary post-import plan.

The backend uses Azure Blob leases for locking. Keep locking enabled. If a lock remains after an interrupted run, first establish that no Terraform process or CI job is still operating. Only then use `terraform force-unlock <lock-id>` against the correct configuration/key. Do not break active leases or use `-lock=false` to bypass contention.

## Sensitive artifacts

State, saved binary plans, JSON exports and logs can contain app settings, connection strings and identity/IP metadata. Keep them in ignored `private/` folders, restrict their access and never attach them to public CI artifacts, issues or Git. Lifecycle exclusions preserve some settings but do not prevent the provider from recording their values in state. The backend's local `.terraform/` metadata is also ignored.

The private `state-bootstrap/private/bootstrap-before-migration.tfstate` recovery copy is retained from initial provisioning; it becomes stale after subsequent changes. Remote state is authoritative. Create a current protected recovery export when needed with `terraform state pull`, redirecting it to an ignored file rather than printing it. Do not commit credentials or use account keys to work around authorization errors.

## Recovery

Blob versioning and 30-day blob/container soft deletion are enabled. The previous-version policy declared in `state-bootstrap/retention.tf` was applied on 12 October 2026; see [verification evidence](../reviews/TERRAFORM_ADOPTION_FIXES.md). It only expires previous block-blob versions whose creation age exceeds 90 days, under prefixes `tfstate/production.tfstate` and `tfstate/bootstrap.tfstate`. Current blobs and snapshots have no deletion action. Reserve those prefixes for these state keys; prefix matching also covers names with those strings as a prefix. Age is from version creation, so a long-lived current version may become immediately eligible once superseded. Expiry is asynchronous; a deleted version is additionally protected by the existing 30-day soft-delete window from deletion. This is not a guaranteed 90-day window measured from when a version became previous. Recover state independently of the application's SQL backup/restore process. LRS stores replicas within one region; this is not a cross-region recovery arrangement. No destructive restore rehearsal has been performed.

1. Stop all infrastructure runs. Determine whether the fault is configuration, Azure resource drift, lost access or corrupt/missing state. Confirm the correct account, container and blob key.
2. Restore Entra/RBAC access if needed using an authorized subscription administrator. Role propagation can take time; keep Shared Key disabled. Loss of access alone does not justify recreating resources.
3. For corrupt or deleted state, inspect versions/deleted blobs in the Azure portal with an authorized account. Preserve the current state and download the selected previous version to protected storage. Inspect its lineage, serial, resource IDs and age privately, and compare it with current Azure resources before restoring it. For a deleted container, use Azure's container recovery facilities within the retention window.
4. Restore the selected blob version through Azure's documented recovery process while all Terraform runs remain stopped. If Terraform state commands are needed, use their normal lineage/serial protections; do not casually force an older state over a newer one. A restored state does not roll back Azure resources.
5. Reinitialize the appropriate configuration, run a refresh-backed plan and reconcile any differences deliberately before allowing an apply. Check both keys independently. Avoid replacing/deleting live resources to match stale state.
6. If the storage account itself is lost, first investigate Azure-supported account recovery and any protected exports. Rebootstrap state storage in a controlled procedure, preserving the identity and state mapping of surviving resources through import rather than recreating them. `prevent_destroy` protects resources only while their Terraform declarations remain present; it is not an Azure deletion lock.

The bootstrap module manages its own storage, so an ordinary `terraform destroy` is not an operational cleanup procedure. No state recovery path recreates SQL rows or supplies the unknown existing SQL administrator password. Application secrets and database data need separate recovery arrangements.

References: [HashiCorp Azure backend](https://developer.hashicorp.com/terraform/language/backend/azurerm), [backend initialization and migration](https://developer.hashicorp.com/terraform/cli/commands/init), [Azure blob versions](https://learn.microsoft.com/en-us/azure/storage/blobs/versioning-overview), [Azure blob soft deletion](https://learn.microsoft.com/en-us/azure/storage/blobs/soft-delete-blob-overview).
