Migrations.
Package Manager Console
Select "Default project:" RopeParison.Data
add-migration MigrationName -Context DataContext
update-database -Context DataContext

Script-Migration -Context DataContext <FromMigration> <ToMigration>
E.g. <Script-Migration -Context DataContext Mig8 Mig9> would produce a script which applied Mig9.