terraform {
  required_version = "~> 1.16.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id                 = "ad052b7b-2015-4933-93eb-0b857cabccae"
  tenant_id                       = "a2155124-112f-4b17-a715-0ca032e18b28"
  resource_provider_registrations = "none"
}
