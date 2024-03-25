using System;
using System.Linq;
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

public class ReferenceControllerTests
{
    [Fact]
    public async Task CanPostReference()
    {
        GetInMemoryDb(out var connection, out var options);
        using var index = new Index(true, true);
        try
        {
            var id = Guid.NewGuid();
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(new Reference {Id = id});
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var result = await service.Get(id);
                Assert.Equal(id, result.Value.Id);
                var count = (await service.GetCount()).Value;
                Assert.Equal(1, count);
                var all = await service.GetAll();
                Assert.Single(all.ToArray());
            }
        }
        finally
        {
            connection.Close();
        }
    }

    private static ReferencesController GetReferencesController(ReferencesDbContext context, Index index)
    {
        return new ReferencesController(new ReferenceRepository(context, index));
    }

    [Fact]
    public async Task CanPostReindexAndGetReference()
    {
        GetInMemoryDb(out var connection, out var options);
        using var index = new Index(true, true);
        try
        {
            var id = Guid.NewGuid();
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(new Reference {Id = id});
            }

            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                service.DoReindex();
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var result = await service.Get(id);
                Assert.Equal(id, result.Value.Id);
                var count = (await service.GetCount()).Value;
                Assert.Equal(1, count);
                var all = await service.GetAll();
                Assert.Single(all.ToArray());
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanDeleteReference()
    {
        GetInMemoryDb(out var connection, out var options);
        using var index = new Index(true, true);
        try
        {
            var id = Guid.NewGuid();
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(new Reference {Id = id});
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var result = await service.Get(id);
                Assert.Equal(id, result.Value.Id);
                service.Delete(id);

                var all = await service.GetAll();
                Assert.Empty(all.ToArray());
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanNotDeleteReferenceIfUsed()
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
                    ReferenceUsage = new List<ReferenceUsage> {new() {ApplicationId = 1, UserId = new Guid()}}
                });
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var result = await service.Get(id);
                Assert.Equal(id, result.Value.Id);

                Assert.Throws<InvalidOperationException>(() => service.Delete(id));

                var all = await service.GetAll();
                Assert.Single(all.ToArray());
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanDeleteReferenceAfterDeletingUsages()
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
                    ReferenceUsage = new List<ReferenceUsage> {new() {ApplicationId = 1, UserId = new Guid()}}
                });
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                var result = await service.Get(id);
                Assert.Equal(id, result.Value.Id);

                Assert.Throws<InvalidOperationException>(() => service.Delete(id));

                var all = await service.GetAll();
                Assert.Single(all.ToArray());
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanPostAndReadCompleteReference()
    {
        GetInMemoryDb(out var connection, out var options);

        using var index = new Index(true, true);
        try
        {


            var reference = new Reference
            {
                Id = Guid.NewGuid(),
                ApplicationId = 1,
                Author = "Theps",
                Bibliography = "tri",
                EditDate = DateTime.Now,
                Firstname = "stein",
                //ImportXml = "no",
                Journal = "the",
                Keywords = "natur,nett",
                Lastname = "hoem",
                Middlename = "Ari",
                Pages = "1-3",
                ReferenceUsage = new[]
                {
                    new ReferenceUsage {UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"), ApplicationId = 1}
                },
                Summary = "Sum",
                Title = "Tiii",
                Url = "http://vg.no",
                UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"),
                Volume = "1",
                Year = "1901"
            };

            // Run the test against one instance of the context
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(reference);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                Assert.Equal(1, context.Reference.Count());
                var it = context.Reference.Include(x => x.ReferenceUsage).First();
                Assert.Equal(it.Year, reference.Year);
                Assert.Equal(it.Volume, reference.Volume);
                Assert.Equal(it.ApplicationId, reference.ApplicationId);
                Assert.Equal(it.Author, reference.Author);
                Assert.Equal(it.Bibliography, reference.Bibliography);
                Assert.Equal(it.EditDate, reference.EditDate);
                Assert.Equal(it.Firstname, reference.Firstname);
                Assert.Equal(it.Id, reference.Id);
                //Assert.Equal(it.ImportXml, reference.ImportXml);
                Assert.Equal(it.Journal, reference.Journal);
                Assert.Equal(it.Lastname, reference.Lastname);
                Assert.Equal(it.Middlename, reference.Middlename);
                Assert.Equal(it.Pages, reference.Pages);
                Assert.Single(it.ReferenceUsage.ToArray());
                Assert.Equal(it.ReferenceUsage.First().ApplicationId,
                    reference.ReferenceUsage.First().ApplicationId);
                Assert.Equal(it.Summary, reference.Summary);
                Assert.Equal(it.Title, reference.Title);
                Assert.Equal(it.Url, reference.Url);
                Assert.Equal(it.UserId, reference.UserId);
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanPostUpdateAndDeleteCompleteReference()
    {
        GetInMemoryDb(out var connection, out var options);

        using var index = new Index(true, true);
        try
        {
            var reference = new Reference
            {
                Id = Guid.NewGuid(),
                ApplicationId = 1,
                Author = "Theps",
                Bibliography = "tri",
                EditDate = DateTime.Now,
                Firstname = "stein",
                //ImportXml = "no",
                Journal = "the",
                Keywords = "natur,nett",
                Lastname = "hoem",
                Middlename = "Ari",
                Pages = "1-3",
                ReferenceUsage = new[]
                {
                    new ReferenceUsage {UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"), ApplicationId = 1}
                },
                Summary = "Sum",
                Title = "Tiii",
                Url = "http://vg.no",
                UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"),
                Volume = "1",
                Year = "1901"
            };

            // Run the test against one instance of the context
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Post(reference);
            }

            var replacementReference = new Reference
            {
                ApplicationId = 2,
                Author = "Theps2",
                Bibliography = "tri2",
                EditDate = DateTime.Now,
                Firstname = "stein2",
                ReferenceString = "no2",
                Journal = "the2",
                Keywords = "natur,nett2",
                Lastname = "hoem2",
                Middlename = "Ari2",
                Pages = "1-32",
                ReferenceUsage = new[]
                {
                    new ReferenceUsage {UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a2"), ApplicationId = 2}
                },
                Summary = "Sum2",
                Title = "Tiii2",
                Url = "http://vg.no2",
                UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a2"),
                Volume = "2",
                Year = "1902"
            };
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Put(reference.Id, replacementReference);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                Assert.Equal(1, context.Reference.Count());
                var service = GetReferencesController(context, index);
                var getit = await service.Get(reference.Id);
                var it = getit.Value;

                Assert.Equal(it.Year, replacementReference.Year);
                Assert.Equal(it.Volume, replacementReference.Volume);
                //Assert.Equal(it.ApplicationId, replacementReference.ApplicationId);
                Assert.Equal(it.Author, replacementReference.Author);
                Assert.Equal(it.Bibliography, replacementReference.Bibliography);
                //Assert.Equal(it.EditDate, replacementReference.EditDate);
                Assert.Equal(it.Firstname, replacementReference.Firstname);
                //Assert.Equal(it.Id, replacementReference.Id);
                Assert.Equal(it.ReferenceString, replacementReference.ReferenceString);
                Assert.Equal(it.Journal, replacementReference.Journal);
                Assert.Equal(it.Lastname, replacementReference.Lastname);
                Assert.Equal(it.Middlename, replacementReference.Middlename);
                Assert.Equal(it.Pages, replacementReference.Pages);
                Assert.Single(it.ReferenceUsage.ToArray());
                Assert.Equal(it.ReferenceUsage.First().ApplicationId,
                    replacementReference.ReferenceUsage.First().ApplicationId);
                Assert.Equal(it.Summary, replacementReference.Summary);
                Assert.Equal(it.Title, replacementReference.Title);
                Assert.Equal(it.Url, replacementReference.Url);
                Assert.Equal(it.UserId, replacementReference.UserId);
            }

            var replacementReference2 = new Reference
            {
                ApplicationId = 2,
                Author = "Theps2",
                Bibliography = "tri2",
                EditDate = DateTime.Now,
                Firstname = "stein3",
                ReferenceString = "no2",
                Journal = "the2",
                Keywords = "natur,nett2",
                Lastname = "hoem2",
                Middlename = "Ari2",
                Pages = "1-32",
                Summary = "Sum2",
                Title = "Tiii2",
                Url = "http://vg.no2",
                UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a2"),
                Volume = "2",
                Year = "1902"
            };
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.Put(reference.Id, replacementReference2);
            }

            await using (var context = new ReferencesDbContext(options))
            {
                Assert.Equal(1, context.Reference.Count());
                var service = GetReferencesController(context, index);
                var getit = await service.Get(reference.Id);
                var it = getit.Value;

                Assert.Equal(it.Year, replacementReference2.Year);
                Assert.Equal(it.Volume, replacementReference2.Volume);
                //Assert.Equal(it.ApplicationId, replacementReference.ApplicationId);
                Assert.Equal(it.Author, replacementReference2.Author);
                Assert.Equal(it.Bibliography, replacementReference2.Bibliography);
                //Assert.Equal(it.EditDate, replacementReference.EditDate);
                Assert.Equal(it.Firstname, replacementReference2.Firstname);
                //Assert.Equal(it.Id, replacementReference.Id);
                Assert.Equal(it.ReferenceString, replacementReference2.ReferenceString);
                Assert.Equal(it.Journal, replacementReference2.Journal);
                Assert.Equal(it.Lastname, replacementReference2.Lastname);
                Assert.Equal(it.Middlename, replacementReference2.Middlename);
                Assert.Equal(it.Pages, replacementReference2.Pages);
                Assert.Single(it.ReferenceUsage.ToArray());
                Assert.Equal(it.ReferenceUsage.First().ApplicationId,
                    replacementReference.ReferenceUsage.First().ApplicationId);
                Assert.Equal(it.Summary, replacementReference2.Summary);
                Assert.Equal(it.Title, replacementReference2.Title);
                Assert.Equal(it.Url, replacementReference2.Url);
                Assert.Equal(it.UserId, replacementReference2.UserId);
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async void CanPostBulkReferences()
    {
        GetInMemoryDb(out var connection, out var options);

        using var index = new Index(true, true);
        try
        {

            // Run the test against one instance of the context
            await using (var context = new ReferencesDbContext(options))
            {
                var service = GetReferencesController(context, index);
                await service.PostMany([new Reference {Summary = "Ref1"}, new Reference {Summary = "Ref2"}]);
            }

            // Use a separate instance of the context to verify correct data was saved to database
            await using (var context = new ReferencesDbContext(options))
            {
                Assert.Equal(1, context.Reference.Count(x => x.Summary == "Ref1"));
                Assert.Equal(1, context.Reference.Count(x => x.Summary == "Ref2"));
            }
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async Task CanNotBulkPostReferencesWithIdenticalId()
    {
        GetInMemoryDb(out var connection, out var options);

        using var index = new Index(true, true);
        try
        {
            await using var context = new ReferencesDbContext(options);
            var id = Guid.NewGuid();
            var service = GetReferencesController(context, index);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.PostMany([
                new Reference {Id = id, Summary = "Ref1"}, new Reference {Id = id, Summary = "Ref2"}
            ]));
        }
        finally
        {
            connection.Close();
        }
    }

    [Fact]
    public async void CanNotPostReferencesWithIdenticalId()
    {
        GetInMemoryDb(out var connection, out var options);

        using var index = new Index(true, true);
        try
        {
            await using var context = new ReferencesDbContext(options);
            var id = Guid.NewGuid();
            var service = GetReferencesController(context, index);
            await service.Post(new Reference {Id = id});
            await Assert
                .ThrowsAsync<InvalidOperationException>(() =>
                    service.Post(new Reference {Id = id, Summary = "Ref1"}));
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