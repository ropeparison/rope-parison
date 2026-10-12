# Azure baseline inventory

Captured 12 October 2026 and updated after adoption. Azure Storage was registered and the four state-bootstrap resources were created. Nine existing app/shared SQL resources were imported with no additions, changes or deletions; fresh plans for both remote states show no changes.

Subscription: `RopeParisonSubscription` (`ad052b7b-2015-4933-93eb-0b857cabccae`). Resource group: `RopeParison`. Main resources are in UK South; the static-site entry is in West Europe.

## Ownership and import scope

The live group contains 16 resource entries. The owner confirmed that SimpleBugTracker uses the `ropeparison` SQL server and `RopeParisonData` database, and requested Terraform ownership of this shared SQL foundation for current and future apps. Deploy-tester telemetry remains outside scope because its purpose is unclear. The inventory reports presence, not current business use.

| Resource(s) | Treatment |
| --- | --- |
| `ropeparison` Linux Web App | Import as `azurerm_linux_web_app.ropeparison` |
| `ASP-RopeParison-99cf` Linux hosting plan | Import as `azurerm_service_plan.ropeparison` |
| `ropeparison` Application Insights | Import as `azurerm_application_insights.ropeparison` |
| `Failure Anomalies - ropeparison` | Import as `azurerm_monitor_smart_detector_alert_rule.ropeparison` |
| `ropeparison` SQL server and `RopeParisonData` database | Import as `azurerm_mssql_server.shared` and `azurerm_mssql_database.shared`; shared ownership across consumers |
| Three existing SQL firewall rules | Import as `azurerm_mssql_firewall_rule.azure_services`, `.legacy_client` and `.query_editor` |
| SQL `master` database | System database, excluded |
| `Application Insights Smart Detection` action group | Shared data source; alerts for the other components use it too |
| `RopeParisonDeployTester` Insights and its failure-anomaly alert | Retained, outside initial import scope |
| `SimpleBugTrackerBackEnd` Web App and Insights; `ASP-RopeParison-bb7e`; its failure-anomaly alert; `SimpleBugTracker-id-b014`; `SimpleBugTracker` static site | Present in Azure; excluded from RopeParison management |
| Existing `RopeParison` resource group | Data source because it contains excluded/shared resources |
| Default Log Analytics workspace in `DefaultResourceGroup-SUK` | Shared data source; linked from all three Insights components |

The nine managed resources have `prevent_destroy`. The firewall rules are child resources and are not additional entries in the 16-resource top-level listing. Backup retention is declared inside the database resource, so it does not need separate import addresses. This is a Terraform safeguard while declarations remain present, not an Azure lock or a guarantee against deletion through other tools.

## Deployed baseline

- Web App: running, Linux, HTTPS only, .NET **7.0**, startup `dotnet RopeParison.dll`; no assigned identity or custom hostname recorded.
- Hosting plan: **F1 / Free**, one worker. Always On is false. WebSockets and HTTP/2 settings are false. These observations are not an application-health test.
- No deployment slots were returned. A separate runtime/deployment change is needed for the locally upgraded .NET 10 application; initial adoption deliberately retains the deployed stack.
- SQL database: **Basic, 5 DTUs, 2 GiB maximum**, requested Zone backup storage redundancy. SQL server/database are managed as shared infrastructure in the adopted configuration. Their current settings were preserved during import.
- Short-term SQL backup retention: **7 days**, differential interval 24 hours. Long-term weekly/monthly/yearly retention is zero. No restore rehearsal was performed.
- Three SQL firewall rules, including Azure-services access. Existing client IP addresses remain in private inventory and are omitted here.
- Shared Log Analytics workspace: `PerGB2018`, 30-day retention. Insights declares 90-day retention; effective retention/charges require separate operational review.
- App configuration includes three operation-password setting names, Insights setting names, and `dataConnection`/`securityConnection` SQL connection-string names. Values were excluded from the CLI inventory. Terraform's private preview may still contain secret values returned by its provider; protect those files as state.
- The initial inventory found no storage accounts. `Microsoft.Storage` is now registered and `strpstatead052b7b2015` was created for Terraform state in the separate `RopeParisonTerraformState` group.

## Validation evidence

- Terraform **1.16.5**, signed HashiCorp AzureRM provider **4.81.0**, version lock files prepared.
- Curated configuration passes `terraform fmt -check` and `terraform validate`.
- Live adoption preview: **9 imports, 0 additions, 0 changes, 0 deletions**. A JSON guard checks the exact nine addresses/IDs and rejects any infrastructure changes.
- Azure omits two network default-action values; AzureRM would otherwise set them to `Allow` during import. Narrow lifecycle exclusions preserve their existing values. App settings, connection strings, authentication blocks, tags and Azure's `SystemAssigned` Key Vault reference sentinel are also preserved during baseline adoption.
- These exclusions limit Terraform's management/drift coverage. They are not a complete secret-management or infrastructure recovery solution. Do not recreate the app from this draft and expect secrets to be supplied automatically.
- Automatic configuration generation was inspected privately, then replaced by curated code to remove invalid generated fields and exclude secrets. App Service IDs must retain the canonical `serverFarms` segment for the provider parser.
- Expanded SQL scope includes the current Entra administrator, existing SQL login, Basic capacity, collation, TLS/network settings, encryption, three firewall rules, seven-day/24-hour short-term backup policy and disabled long-term retention. No password is read, supplied or rotated. Administrator/client-IP metadata is stored in ignored private inputs. The same state must be the sole owner of this shared SQL infrastructure; other apps reference it.
- Separate state bootstrap passes format/validation and previews **4 additions, 0 changes, 0 deletions**: resource group, storage account, private container and user role assignment. It was applied successfully; its subsequent remote plan shows no changes.

## Completed state bootstrap

Created group: `RopeParisonTerraformState`; storage: `strpstatead052b7b2015`; private container: `tfstate`. Standard StorageV2, LRS, UK South, TLS 1.2 and HTTPS, anonymous blob access disabled, Shared Key access disabled, Entra authentication, blob versioning and 30-day blob/container soft deletion. Data access is granted to the existing signed-in owner at this storage account's scope, not to a new application identity. The authenticated storage endpoint remains accessible over the public network; anonymous access is disabled.

The owner approved creation, provider registration, scoped data access and imports. The account name was confirmed available before creation. The four-resource bootstrap apply created only the reviewed resources. Storage and operations are billable; no monetary estimate or new staging environment is included.

Both configurations now use Entra-authenticated Azure Blob state: `bootstrap.tfstate` and `production.tfstate` in container `tfstate`. Bootstrap migration preserved lineage and all resource contents, with serial advancing from 5 to 6; a private recovery copy was retained. The final guarded adoption plan and apply both reported nine imports, zero additions, changes or deletions. All nine remote-state resource IDs were verified, and subsequent ordinary plans for both configurations reported no changes. See [state operations and recovery](STATE_OPERATIONS.md). Infrastructure CI/OIDC and intentional .NET runtime improvements follow separately.

References: [AzureRM resource docs](https://registry.terraform.io/providers/hashicorp/azurerm/4.81.0/docs), [Azure Blob backend and Entra authentication](https://developer.hashicorp.com/terraform/language/backend/azurerm).
