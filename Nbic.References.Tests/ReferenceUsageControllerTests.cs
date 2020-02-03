using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nbic.References.Controllers;
using Nbic.References.EFCore;
using Nbic.References.Public.Models;
using Nbic.Indexer;
using Xunit;

namespace Nbic.References.Tests
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Mvc;
    using Index = Indexer.Index;

    public class ReferenceUsageControllerTests
    {
        [Fact]
        public async Task CanPostReferenceAndGetReferenceUsages()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);
            using var index = new Index();

            try
            {
                var id = Guid.NewGuid();
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context, index);
                    await service.Post(new Reference() {Id = id, ReferenceUsage = new List<ReferenceUsage>() { new ReferenceUsage() { ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") } } }).ConfigureAwait(false);
                }

                // Use a separate instance of the context to verify correct data was saved to database
                using (var context = new ReferencesDbContext(options))
                {
                    var usageService = new ReferenceUsageController(context);
                    
                    var count = (await usageService.GetCount().ConfigureAwait(false)).Value;
                    Assert.Equal(1, count);
                    var all = await usageService.GetAll(0, 10).ConfigureAwait(false);
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
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);
            using var index = new Index();

            try
            {
                var id = Guid.NewGuid();
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context, index);
                    await service.Post(new Reference() { Id = id, ReferenceUsage = new List<ReferenceUsage>() { new ReferenceUsage() { ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") } } }).ConfigureAwait(false);
                }

                // Use a separate instance of the context to verify correct data was saved to database
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context, index);
                    var usageService = new ReferenceUsageController(context);
                    usageService.DeleteAllUsages(id);
                    var all = await usageService.GetAll(0, 10).ConfigureAwait(false);
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
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);
            using var index = new Index();

            try
            {
                var id = Guid.NewGuid();
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context, index);
                    await service.Post(new Reference() { Id = id, ReferenceUsage = new List<ReferenceUsage>() { new ReferenceUsage() { ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") }, new ReferenceUsage() { ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") } } }).ConfigureAwait(false);
                }

                // Use a separate instance of the context to verify correct data was saved to database
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context, index);
                    var usageService = new ReferenceUsageController(context);
                    usageService.DeleteUsage(id, 1, new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"));
                    var all = await usageService.GetAll(0, 10).ConfigureAwait(false);
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
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            using var index = new Index();
            try
            {
                var id = Guid.NewGuid();
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context, index);
                    await service.Post(new Reference() { Id = id, ReferenceUsage = new List<ReferenceUsage>() { new ReferenceUsage() { ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") }, new ReferenceUsage() { ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") } } }).ConfigureAwait(false);
                }

                // Use a separate instance of the context to verify correct data was saved to database
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context, index);
                    var usageService = new ReferenceUsageController(context);
                    await usageService.Post(new ReferenceUsage() { ApplicationId = 3, ReferenceId = id, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") }).ConfigureAwait(false);
                    var all = await usageService.GetAll(0, 10).ConfigureAwait(false);
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
        public async Task CanAddSingleDuplicateReferenceUsage()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);
            using var index = new Index();

            try
            {
                var id = Guid.NewGuid();
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context, index);
                    await service.Post(new Reference() { Id = id, ReferenceUsage = new List<ReferenceUsage>() { new ReferenceUsage() { ApplicationId = 1, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") }, new ReferenceUsage() { ApplicationId = 2, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") } } }).ConfigureAwait(false);
                }

                // Use a separate instance of the context to verify correct data was saved to database
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context, index);
                    var usageService = new ReferenceUsageController(context);
                    await usageService.Post(new ReferenceUsage() { ApplicationId = 1, ReferenceId = id, UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3") }).ConfigureAwait(false);
                    var all = await usageService.GetAll(0, 10).ConfigureAwait(false);
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
            using (var context = new ReferencesDbContext(options))
            {
                context.Database.EnsureCreated();
            }
        }
    }
}
