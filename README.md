# Nbic.References
Reference database that handles literature references, person and url references. Also tracks usage by userid and applicationid if needed.

Why: Converted an old reference database from class library to REST API

API documented with Swagger at root URL


## Test with docker
```<language>
docker run -d -p 8080:80 -p 8082:443 --name nbicreferences -v /app/Data steinho/nbicreferences
```

Default configuration:
```<language>
ENV AuthAuthority=https://demo.identityserver.io
ENV AuthAuthorityEndPoint=https://demo.identityserver.io/connect/authorize
ENV AuthApiSecret="test-secret"
ENV ApiName=api
ENV DbProvider=Sqlite
ENV DbConnectionString="Data Source=referencesdb.db"
```

This configuration not recommended for production - alter as needed.

ENV DbProvider=Sqlite or SqlServer
Schema will be created on first run.

If using SQLite - map volume /app/Data/ for persistence and backup 

Feel free to fork!

