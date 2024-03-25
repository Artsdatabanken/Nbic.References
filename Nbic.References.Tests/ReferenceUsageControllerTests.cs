using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nbic.References.Controllers;
using Nbic.References.Core.Models;
using Nbic.References.EFCore;
using Nbic.References.Infrastructure.Repositories;
using Xunit;
using Index = Nbic.References.Infrastructure.Services.Indexing.Index;

namespace Nbic.References.Tests;

using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Index = Index;

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
                }).ConfigureAwait(false);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var usageService = GetReferencesUsageController(context);

                var count = (await usageService.GetCount().ConfigureAwait(false)).Value;
                Assert.Equal(1, count);
                var all = await usageService.GetAll().ConfigureAwait(false);
                Assert.Single(all);
                var them = await usageService.Get(id).ConfigureAwait(false);
                Assert.Single(them);
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
                }).ConfigureAwait(false);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                usageService.DeleteAllUsages(id);
                var all = await usageService.GetAll().ConfigureAwait(false);
                Assert.Empty(all);
                var result = await service.Get(id).ConfigureAwait(false);
                Assert.Equal(id, result.Value.Id);

                // now delete reference
                service.Delete(id);
                // and should not be found
                var result2 = await service.Get(id).ConfigureAwait(false);
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
                }).ConfigureAwait(false);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                usageService.DeleteUsage(id, 1, new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"));
                var all = await usageService.GetAll().ConfigureAwait(false);
                Assert.Single(all);
                var result = await service.Get(id).ConfigureAwait(false);
                Assert.Equal(id, result.Value.Id);

                // and should not be found
                var result2 = await service.Get(id).ConfigureAwait(false);
                Assert.Single(result2.Value.ReferenceUsage);
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
                await service.Post(new Reference { Id = id, ReferenceUsage = new List<ReferenceUsage> { new() { ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") }, new() { ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") } } }).ConfigureAwait(false);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                await usageService.Post(new ReferenceUsage { ApplicationId = 3, ReferenceId = id, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") }).ConfigureAwait(false);
                var all = await usageService.GetAll().ConfigureAwait(false);
                Assert.Equal(3, all.Count);
                var result = await service.Get(id).ConfigureAwait(false);
                Assert.Equal(id, result.Value.Id);
                Assert.Equal(3, result.Value.ReferenceUsage.Count);
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
                }).ConfigureAwait(false);
                await service.Post(new Reference
                {
                    Id = id2,
                    ReferenceUsage = new List<ReferenceUsage>
                    {
                        new() {ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")},
                        new() {ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")}
                    }
                }).ConfigureAwait(false);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                await usageService.Post([
                    new()
                    {
                        ApplicationId = 3, ReferenceId = id,
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")
                    },
                    new()
                    {
                        ApplicationId = 3, ReferenceId = id2,
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")
                    }
                ]).ConfigureAwait(false);
                var all = await usageService.GetAll().ConfigureAwait(false);
                Assert.Equal(6, all.Count);
                var result = await service.Get(id).ConfigureAwait(false);
                Assert.Equal(id, result.Value.Id);
                Assert.Equal(3, result.Value.ReferenceUsage.Count);
                var result2 = await service.Get(id2).ConfigureAwait(false);
                Assert.Equal(id2, result2.Value.Id);
                Assert.Equal(3, result2.Value.ReferenceUsage.Count);
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
                }).ConfigureAwait(false);
                await service.Post(new Reference
                {
                    Id = id2,
                    ReferenceUsage = new List<ReferenceUsage>
                    {
                        new() {ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")},
                        new() {ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")}
                    }
                }).ConfigureAwait(false);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var usageService = GetReferencesUsageController(context);
                await usageService.Post([
                    new()
                    {
                        ApplicationId = 3, ReferenceId = id,
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")
                    },
                    new()
                    {
                        ApplicationId = 3, ReferenceId = id2,
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3")
                    },
                    new()
                    {
                        ApplicationId = 3, ReferenceId = id3,
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-87f7fcac67a3")
                    }
                ]).ConfigureAwait(false);
                var all = await usageService.GetAll().ConfigureAwait(false);
                Assert.Equal(6, all.Count);
                var result = await service.Get(id).ConfigureAwait(false);
                Assert.Equal(id, result.Value.Id);
                Assert.Equal(3, result.Value.ReferenceUsage.Count);
                var result2 = await service.Get(id2).ConfigureAwait(false);
                Assert.Equal(id2, result2.Value.Id);
                Assert.Equal(3, result2.Value.ReferenceUsage.Count);
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
                }).ConfigureAwait(false);
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
                }).ConfigureAwait(false);
                var all = await usageService.GetAll();
                Assert.Equal(2, all.Count);
                var result = await service.Get(id).ConfigureAwait(false);
                Assert.Equal(id, result.Value.Id);
                Assert.Equal(2, result.Value.ReferenceUsage.Count);
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
