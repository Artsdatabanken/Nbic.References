using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nbic.References.Controllers;
using Nbic.References.Core.Models;
using Nbic.References.EFCore;
using Nbic.References.Infrastructure.Repositories;
using Nbic.References.Infrastructure.Repositories.DbContext;
using Xunit;
using Index = Nbic.References.Infrastructure.Services.Indexing.Index;

namespace Nbic.References.Tests;

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

public class ReferenceUsageControllerTests
{
    private static ReferencesController GetReferencesController(ReferencesDbContext context, Index index)
    {
        return new ReferencesController(new ReferenceRepository(context, index));
    }
    private static ReferenceUsageController GetReferencesUsageController(ReferencesDbContext context)
    {
        return new ReferenceUsageController(new ReferenceUsageRepository(context));
    }

    [Fact]
    public async Task CanPostReferenceAndGetReferenceUsages()
    {
        GetInMemoryDb(out var connection, out var options);
        using var index = new Index(true, true);
        try
        {
            var id = Guid.NewGuid();
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(new Reference
                {
                    Id = id,
                    ReferenceUsage = new List<ReferenceUsage>
                    {
                        new() {ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")}
                    }
                });
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var usageService = GetReferencesUsageController(context);

                var response = await usageService.GetCount();
                if (response.Result is OkObjectResult okResult)
                {
                    var count1 = okResult.Value as int?;
                    Assert.Equal(1, count1);
                }

                var allResult = await usageService.GetAll();
                if (allResult.Result is OkObjectResult allOkResult)
                {
                    var all = allOkResult.Value as List<ReferenceUsage>;
                    Assert.Single(all);
                }

                var themResult = await usageService.Get(id);
                if (themResult.Result is OkObjectResult themOkResult)
                {
                    var them = themOkResult.Value as List<ReferenceUsage>;
                    Assert.Single(them);
                }
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanDeleteReferenceIfUsageReferencesIsDeletedFirst()
    {
        GetInMemoryDb(out var connection, out var options);
        using var index = new Index(true, true);
        try
        {
            var id = Guid.NewGuid();
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(new Reference
                {
                    Id = id,
                    ReferenceUsage = new List<ReferenceUsage>
                    {
                        new() {ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")}
                    }
                });
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                usageService.DeleteAllUsages(id);

                var allResult = await usageService.GetAll();
                if (allResult.Result is OkObjectResult allOkResult)
                {
                    var all = allOkResult.Value as List<ReferenceUsage>;
                    Assert.Empty(all);
                }

                var response = await service.Get(id);
                if (response.Result is OkObjectResult okObjectResult)
                {
                    Assert.Equal(id, ((Reference)okObjectResult.Value).Id);
                    Assert.Empty(((Reference)okObjectResult.Value).ReferenceUsage);
                }

                // now delete reference
                service.Delete(id);
                // and should not be found
                var result2 = await service.Get(id);
                Assert.IsType<NotFoundResult>(result2.Result);
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanDeleteSingleReferenceUsage()
    {
        GetInMemoryDb(out var connection, out var options);
        using var index = new Index(true, true);
        try
        {
            var id = Guid.NewGuid();
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(new Reference
                {
                    Id = id,
                    ReferenceUsage = new List<ReferenceUsage>
                    {
                        new() {ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")},
                        new() {ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")}
                    }
                });
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                usageService.DeleteUsage(id, 1, new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"));

                var allResult = await usageService.GetAll();
                if (allResult.Result is OkObjectResult allOkResult)
                {
                    var all = allOkResult.Value as List<ReferenceUsage>;
                    Assert.Single(all);
                }

                var response = await service.Get(id);
                if (response.Result is OkObjectResult okObjectResult)
                {
                    Assert.Equal(id, ((Reference)okObjectResult.Value).Id);
                    Assert.Single(((Reference)okObjectResult.Value).ReferenceUsage);
                }

                // and should not be found
                var response1 = await service.Get(id);
                if (response1.Result is OkObjectResult okObjectResult1)
                {
                    Assert.Equal(id, ((Reference)okObjectResult1.Value).Id);
                    Assert.Single(((Reference)okObjectResult1.Value).ReferenceUsage);
                }
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanAddSingleReferenceUsage()
    {
        GetInMemoryDb(out var connection, out var options);

        using var index = new Index(true, true);
        try
        {
            var id = Guid.NewGuid();
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(new Reference { Id = id, ReferenceUsage = new List<ReferenceUsage> { new() { ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") }, new() { ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") } } });
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                await usageService.Post(new ReferenceUsage { ApplicationId = 3, ReferenceId = id, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") });

                var allResult = await usageService.GetAll();
                if (allResult.Result is OkObjectResult allOkResult)
                {
                    var all = allOkResult.Value as List<ReferenceUsage>;
                    Assert.Equal(3, all.Count);
                }

                var response = await service.Get(id);
                if (response.Result is OkObjectResult okObjectResult)
                {
                    Assert.Equal(id, ((Reference)okObjectResult.Value).Id);
                    Assert.Equal(3, ((Reference)okObjectResult.Value).ReferenceUsage.Count);
                }
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanAddDoubleReferenceUsage()
    {
        GetInMemoryDb(out var connection, out var options);

        using var index = new Index(true, true);
        try
        {
            var id = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(new Reference
                {
                    Id = id,
                    ReferenceUsage = new List<ReferenceUsage>
                    {
                        new() {ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")},
                        new() {ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")}
                    }
                });
                await service.Post(new Reference
                {
                    Id = id2,
                    ReferenceUsage = new List<ReferenceUsage>
                    {
                        new() {ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")},
                        new() {ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")}
                    }
                });
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                await usageService.Post(new[]
                {
                    new ReferenceUsage
                    {
                        ApplicationId = 3, ReferenceId = id,
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")
                    },
                    new ReferenceUsage
                    {
                        ApplicationId = 3, ReferenceId = id2,
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")
                    }
                });

                var allResult = await usageService.GetAll();
                if (allResult.Result is OkObjectResult allOkResult)
                {
                    var all = allOkResult.Value as List<ReferenceUsage>;
                    Assert.Equal(6, all.Count);
                }

                var response = await service.Get(id);
                if (response.Result is OkObjectResult okObjectResult)
                {
                    Assert.Equal(id, ((Reference)okObjectResult.Value).Id);
                    Assert.Equal(3, ((Reference)okObjectResult.Value).ReferenceUsage.Count);
                }

                var response1 = await service.Get(id2);
                if (response1.Result is OkObjectResult okResult)
                {
                    Assert.Equal(id2, ((Reference)okResult.Value).Id);
                    Assert.Equal(3, ((Reference)okResult.Value).ReferenceUsage.Count);
                }
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task AddReferenceUsageToNotExistingReferenceShouldNotFail()
    {
        GetInMemoryDb(out var connection, out var options);

        using var index = new Index(true, true);
        try
        {
            var id = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(new Reference
                {
                    Id = id,
                    ReferenceUsage = new List<ReferenceUsage>
                    {
                        new() {ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")},
                        new() {ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")}
                    }
                });
                await service.Post(new Reference
                {
                    Id = id2,
                    ReferenceUsage = new List<ReferenceUsage>
                    {
                        new() {ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")},
                        new() {ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")}
                    }
                });
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                await usageService.Post(new[]
                {
                    new ReferenceUsage
                    {
                        ApplicationId = 3, ReferenceId = id,
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")
                    },
                    new ReferenceUsage
                    {
                        ApplicationId = 3, ReferenceId = id2,
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")
                    },
                    new ReferenceUsage
                    {
                        ApplicationId = 3, ReferenceId = id3,
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-87f7fcac67a3")
                    }
                });

                var allResult = await usageService.GetAll();
                if (allResult.Result is OkObjectResult allOkResult)
                {
                    var all = allOkResult.Value as List<ReferenceUsage>;
                    Assert.Equal(6, all.Count);
                }

                var response = await service.Get(id);
                if (response.Result is OkObjectResult okObjectResult)
                {
                    Assert.Equal(id, ((Reference)okObjectResult.Value).Id);
                    Assert.Equal(3, ((Reference)okObjectResult.Value).ReferenceUsage.Count);
                }

                var response2 = await service.Get(id2);
                if (response2.Result is OkObjectResult okResult)
                {
                    Assert.Equal(id2, ((Reference)okResult.Value).Id);
                    Assert.Equal(3, ((Reference)okResult.Value).ReferenceUsage.Count);
                }
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanAddSingleDuplicateReferenceUsage()
    {
        GetInMemoryDb(out var connection, out var options);
        using var index = new Index(true, true);
        try
        {
            var id = Guid.NewGuid();
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(new Reference
                {
                    Id = id,
                    ReferenceUsage = new List<ReferenceUsage>
                    {
                        new() {ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")},
                        new() {ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")}
                    }
                });
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                await usageService.Post(new ReferenceUsage
                {
                    ApplicationId = 1,
                    ReferenceId = id,
                    UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")
                });

                var allResult = await usageService.GetAll();
                if (allResult.Result is OkObjectResult allOkResult)
                {
                    var all = allOkResult.Value as List<ReferenceUsage>;
                    Assert.Equal(2, all.Count);
                }

                var response = await service.Get(id);
                if (response.Result is OkObjectResult okObjectResult)
                {
                    Assert.Equal(id, ((Reference)okObjectResult.Value).Id);
                    Assert.Equal(2, ((Reference)okObjectResult.Value).ReferenceUsage.Count);
                }
            }
        }
        finally
        {
            connection.Close();
        }
    }

    private static void GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options)
    {
        // In-memory database only exists while the connection is open
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        options = new DbContextOptionsBuilder<ReferencesDbContext>()
            .UseSqlite(connection)
            .Options;

        // Create the schema in the database
        using var context = new ReferencesDbContext(options);
        context.Database.EnsureCreated();
    }
}
