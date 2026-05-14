var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("ReferencesDb").PublishAsConnectionString();

var api = builder.AddProject<Projects.Nbic_References>("ReferencesApi")
    .WaitFor(sqlServer)
    .WithReference(sqlServer)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
