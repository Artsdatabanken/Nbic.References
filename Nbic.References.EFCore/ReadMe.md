Aktiver default constructors på 
ReferencesDbContext.cs

- Add-Migration Initial -Context SqlServerReferencesDbContext -Project Nbic.References.EFCore -StartupProject Nbic.References -OutputDir Migrations\SqlServerMigrations
- Update-Database -Context SqlServerReferencesDbContext -Project Nbic.References.EFCore -StartupProject Nbic.References

- Add-Migration Initial -Context SqliteReferencesDbContext -Project Nbic.References.EFCore -StartupProject Nbic.References -OutputDir Migrations\SqliteMigrations
