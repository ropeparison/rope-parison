output "web_app_url" {
  value = "https://${azurerm_linux_web_app.ropeparison.default_hostname}"
}

output "shared_database_id" {
  value = azurerm_mssql_database.shared.id
}

output "shared_sql_server_id" {
  value = azurerm_mssql_server.shared.id
}
