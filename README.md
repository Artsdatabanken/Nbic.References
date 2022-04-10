# Nbic.References
Reference database that handles literature references, person and url references. Also tracks usage by userid and applicationid if needed.

Why: Converted an old reference database from class library to REST API

API documented with Swagger at root URL


## Test with docker
```<language>
docker run -d -p 8080:8000 --name nbicreferences artsdatabanken/nbicreferences
```

Default configuration:
```<language>
ENV AuthAuthority=https://demo.identityserver.com
ENV AuthAuthorityEndPoint=https://demo.identityserver.com/connect/authorize
ENV ApiName=api
ENV WriteAccessRole="my_write_access_role"
ENV SwaggerClientId="implicit"
ENV DbProvider=Sqlite
ENV DbConnectionString="Data Source=referencesdb.db"
```

This configuration not recommended for production - alter as needed.

ENV DbProvider=Sqlite or SqlServer
Schema will be created on first run.

If using SQLite - map volume /app/Data/ for persistence and backup 

Feel free to fork!

Landingpages for api (Swagger):

- https://referenceapi.artsdatabanken.no/ (production)
- https://referenceapi.test.artsdatabanken.no/ (test/staging)

