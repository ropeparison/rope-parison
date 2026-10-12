# Nonsecret backend settings for the adopted application and shared SQL resources.
resource_group_name  = "RopeParisonTerraformState"
storage_account_name = "strpstatead052b7b2015"
container_name       = "tfstate"
key                  = "production.tfstate"
use_azuread_auth     = true
use_cli              = true
subscription_id      = "ad052b7b-2015-4933-93eb-0b857cabccae"
tenant_id            = "a2155124-112f-4b17-a715-0ca032e18b28"
