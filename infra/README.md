# Azure inventory and Terraform adoption

## Completed baseline

On 12 October 2026, the existing resources in subscription `RopeParisonSubscription` (`ad052b7b-2015-4933-93eb-0b857cabccae`) and resource group `RopeParison` were adopted into Terraform. Terraform 1.16.5 and the signed HashiCorp AzureRM 4.81.0 provider were used; provider lock files are included and automatic provider registration remains disabled.

- State setup created four resources: resource group `RopeParisonTerraformState`, Standard LRS account `strpstatead052b7b2015`, private container `tfstate`, and an account-scoped Storage Blob Data Contributor grant for the existing owner. `Microsoft.Storage` was registered explicitly.
- Both configurations now use the Azure Blob backend with Entra/CLI authentication. Bootstrap state is in `bootstrap.tfstate`; application/shared SQL state is in `production.tfstate`. Bootstrap migration preserved lineage and all resource contents; its serial advanced from 5 to 6 during migration. A private local recovery copy was retained.
- The guarded adoption plan contained **9 imports, 0 additions, 0 changes, 0 deletions**. Applying that saved plan imported all nine existing resources with the same counts.
- Exact resource IDs were verified in remote production state. Fresh normal plans for both configurations reported **no changes**.
- No application deployment, SQL data mutation, EF migration, SQL password rotation or hosting runtime upgrade was performed during adoption.

See [the inventory and ownership map](AZURE_INVENTORY.md) and [state operations and recovery](STATE_OPERATIONS.md). Saved plans, logs, metadata inputs and state exports remain under ignored `private/` folders. Changes are recorded in local commits on `major-update`.

## Ownership

`main.tf` manages the RopeParison Web App, its hosting plan, Application Insights component and app-specific failure alert. `shared-sql.tf` manages the shared SQL server, existing database and three firewall rules. SimpleBugTracker uses that same server and database; future apps must reference this foundation rather than declare the same Azure resources in another state.

The existing resource group, default Log Analytics workspace and shared action group are data sources. SimpleBugTracker's application resources, the SQL `master` database and deploy-tester telemetry remain outside this configuration. All nine adopted resources have `prevent_destroy`, which protects against Terraform deletion while their declarations remain present; it is not an Azure lock.

## First use from a fresh checkout

Install Terraform, Azure CLI and PowerShell 7. Both inventory helpers resolve `az` from PATH (with Windows install fallbacks), and accept `-AzureCliPath` for an explicit executable. From this folder:

```powershell
az login --tenant a2155124-112f-4b17-a715-0ca032e18b28
terraform init '-backend-config=backend.azurerm.hcl'
terraform fmt -check -recursive
terraform validate
./Restore-SqlBaselineInputs.ps1
```

Prefer restoring an existing protected, reviewed `private/sql-baseline.tfvars.json` copy when available. Otherwise the restore helper extracts only the SQL inputs from the initialized production state's recorded settings; it does not query live SQL, print/save the full state, or overwrite an existing input file. Review the restored values and the state's provenance before using them, especially if another operator has refreshed or changed state. Do not regenerate desired inputs from current Azure settings during normal planning or CI. Trusted CI must receive a stable reviewed input file from a protected source; that integration is the next stage.

## Routine plan and drift inspection

Keep the desired SQL input file unchanged unless an intentional update is reviewed. From an initialized checkout:

```powershell
terraform plan '-input=false' '-var-file=private/sql-baseline.tfvars.json' '-out=private/change.tfplan'
```

Optional read-only inventory and comparison:

```powershell
./Inventory-Azure.ps1
./Inspect-RopeParisonDependencies.ps1
./Test-SqlBaselineDrift.ps1
```

The dependency inspector independently reads its resource list and creates its output folder; it does not depend on a previous `azure-inventory.json`. Drift comparison reports changed field names without printing administrator identities or IP ranges and never edits desired inputs. Added/removed firewall names and a changed Azure-services rule are rejected for explicit review.

`Write-SqlBaselineInputs.ps1` is exclusively for initializing a new adoption from captured live metadata. It refuses to overwrite an existing file, including concurrent creation. It is not a routine drift or CI step. To propose changes after adoption, create a separate ignored candidate input file, edit/review it deliberately, and generate a plan with that candidate. Apply the reviewed saved plan before promoting the candidate to the canonical desired input. Preserve the previous reviewed input privately for rollback review. Avoid `terraform refresh` or refresh-only applies merely to make drift disappear.

The input metadata includes the current Entra administrator and two client IP ranges. It is private, but not marked sensitive in HCL because those annotations introduced state-only updates during adoption. No SQL password is retrieved or supplied. Coordinate changes with all shared SQL consumers. Saved plans, JSON and state may contain secret values; do not publish them as CI artifacts. `imports.tf` remains an idempotent adoption record; `Test-AdoptionPlan.ps1` is exclusively for the original nine-import plan, not ordinary plans.

Inventory scripts exclude app-setting values, connection-string values, keys and publishing profiles; Terraform's provider can still record secret-bearing settings in private state and plans.

## Verification and deployment gate

Run the offline helper/deployment regressions with PowerShell 7:

```powershell
./tests/Test-InfraHelpers.ps1
```

The application workflow's deploy job requires the repository variable `ROPEPARISON_PRODUCTION_DEPLOYMENT_ENABLED` to equal `true` and the `main` ref. Leave the variable unset/false during release preparation. The job now uses OIDC, with `AZURE_CLIENT_ID`, `AZURE_TENANT_ID` and `AZURE_SUBSCRIPTION_ID` provided through protected GitHub secrets, plus an Azure federated identity and actual `Production` environment protections. No federation or GitHub settings were created by these fixes; deployment remains gated until the next stage configures them.

Before deployment, the workflow verifies the signed-in subscription and compares the published artifact's framework versions with both the Terraform stack and Azure's live `linuxFxVersion`. Changing HCL alone cannot bypass the live-runtime check. These checks do not prove schema compatibility or application health. Coordinate the runtime, artifact, migrations and rollback before enabling deployment. The former publish-profile path is removed from this workflow; revoke its unused credential after verifying the OIDC replacement, as a separate operational step.

## Preserved settings and limits

SQL retains its existing Basic tier, 2 GiB capacity, Zone backup storage, collation, TLS/network settings, encryption, Entra administrator, SQL login, firewall rules, seven-day short-term retention and disabled long-term retention. The existing SQL password is unknown and excluded from management. This configuration does not provide credentials for rebuilding a deleted server. SQL tables, rows and schema migrations remain application-owned.

App settings, connection strings, authentication settings and tags are preserved through lifecycle exclusions. Narrow exclusions also preserve Azure's unset network defaults and Key Vault identity sentinel. These areas are not fully managed or drift-checked yet, and the configuration alone cannot recover app secrets. Hosting remains .NET 7.0 even though the local application is upgraded to .NET 10; runtime and deployment changes need a separate reviewed plan. The deployment gate prevents accidental release while these stacks differ.

State storage uses Entra authentication, HTTPS/TLS 1.2, no anonymous or Shared Key access, blob versioning, and 30-day blob/container soft deletion. The applied `state-bootstrap/retention.tf` policy expires only previous versions older than 90 days from creation under the two reserved state prefixes; current state is preserved. The authenticated endpoint is accessible over the public network. Storage and operations are billable. LRS is within one region; no destructive recovery rehearsal or cost estimate was performed.

The follow-up review and verified resolutions are recorded in [Terraform adoption review](../reviews/TERRAFORM_ADOPTION_REVIEW.md) and [Terraform adoption fixes](../reviews/TERRAFORM_ADOPTION_FIXES.md).

## Next work

1. Add infrastructure CI for format/validation and trusted plans, followed by GitHub-to-Azure workload identity federation and a reviewed production apply gate. No CI identity or extra access grant has been created yet.
2. Plan the production .NET 10 runtime/deployment change separately, coordinating database migrations and application configuration.
3. Improve secret management and drift coverage, and rehearse state/database recovery with an agreed scope.

Angular work follows application fixes and infrastructure adoption. New paid hosting, capacity changes and additional applications are separate changes.
