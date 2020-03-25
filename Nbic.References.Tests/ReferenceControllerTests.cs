using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nbic.Indexer;
using Nbic.References.Controllers;
using Nbic.References.EFCore;
using Nbic.References.Public.Models;
using Xunit;
using Index = Nbic.Indexer.Index;

namespace Nbic.References.Tests
{
    using System.Collections.Generic;

    public class ReferenceControllerTests
    {
        [Fact]
        public async Task CanPostReference()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);
            using (var index = new Index())
            {
                try
                {
                    var id = Guid.NewGuid();
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Post(new Reference() {Id = id}).ConfigureAwait(false);
                    }

                    // Use a separate instance of the context to verify correct data was saved to database
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Get(id).ConfigureAwait(false);
                        Assert.Equal(id, result.Value.Id);
                        var count = (await service.GetCount().ConfigureAwait(false)).Value;
                        Assert.Equal(1, count);
                        var all = await service.GetAll(0, 10).ConfigureAwait(false);
                        Assert.Single(all.ToArray());
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [Fact]
        public async Task CanPostReindexAndGetReference()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);
            using (var index = new Index())
            {
                try
                {
                    var id = Guid.NewGuid();
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Post(new Reference() {Id = id}).ConfigureAwait(false);
                    }

                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = service.DoReindex();
                    }

                    // Use a separate instance of the context to verify correct data was saved to database
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Get(id).ConfigureAwait(false);
                        Assert.Equal(id, result.Value.Id);
                        var count = (await service.GetCount().ConfigureAwait(false)).Value;
                        Assert.Equal(1, count);
                        var all = await service.GetAll(0, 10).ConfigureAwait(false);
                        Assert.Single(all.ToArray());
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [Fact]
        public async Task CanDeleteReference()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);
            using (var index = new Index())
            {
                try
                {
                    var id = Guid.NewGuid();
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Post(new Reference() {Id = id}).ConfigureAwait(false);
                    }

                    // Use a separate instance of the context to verify correct data was saved to database
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Get(id).ConfigureAwait(false);
                        Assert.Equal(id, result.Value.Id);
                        service.Delete(id);

                        var all = await service.GetAll(0, 10).ConfigureAwait(false);
                        Assert.Empty(all.ToArray());
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [Fact]
        public async Task CanNotDeleteReferenceIfUsed()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            using (var index = new Index())
            {
                try
                {
                    var id = Guid.NewGuid();
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Post(new Reference()
                        {
                            Id = id,
                            ReferenceUsage = new List<ReferenceUsage>()
                                {new ReferenceUsage() {ApplicationId = 1, UserId = new Guid()}}
                        }).ConfigureAwait(false);
                    }

                    // Use a separate instance of the context to verify correct data was saved to database
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Get(id).ConfigureAwait(false);
                        Assert.Equal(id, result.Value.Id);

                        Assert.Throws<System.InvalidOperationException>(() => service.Delete(id));

                        var all = await service.GetAll(0, 10).ConfigureAwait(false);
                        Assert.Single(all.ToArray());
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [Fact]
        public async Task CanDeleteReferenceAfterDeletingUsages()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            using (var index = new Index())
            {
                try
                {
                    var id = Guid.NewGuid();
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Post(new Reference()
                        {
                            Id = id,
                            ReferenceUsage = new List<ReferenceUsage>()
                                {new ReferenceUsage() {ApplicationId = 1, UserId = new Guid()}}
                        }).ConfigureAwait(false);
                    }

                    // Use a separate instance of the context to verify correct data was saved to database
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Get(id).ConfigureAwait(false);
                        Assert.Equal(id, result.Value.Id);

                        Assert.Throws<System.InvalidOperationException>(() => service.Delete(id));

                        var all = await service.GetAll(0, 10).ConfigureAwait(false);
                        Assert.Single(all.ToArray());
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [Fact]
        public async Task CanPostAndReadCompleteReference()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            using (var index = new Index())
            {
                try
                {


                    Reference reference = new Reference()
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
                            new ReferenceUsage()
                                {UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"), ApplicationId = 1}
                        },
                        Summary = "Sum",
                        Title = "Tiii",
                        Url = "http://vg.no",
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"),
                        Volume = "1",
                        Year = "1901"
                    };

                    // Run the test against one instance of the context
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = await service.Post(reference).ConfigureAwait(false);
                    }

                    // Use a separate instance of the context to verify correct data was saved to database
                    using (var context = new ReferencesDbContext(options))
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
        }

        [Fact]
        public async Task CanPostUpdateAndDeleteCompleteReference()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            using (var index = new Index())
            {
                try
                {


                    Reference reference = new Reference()
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
                            new ReferenceUsage()
                                {UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"), ApplicationId = 1}
                        },
                        Summary = "Sum",
                        Title = "Tiii",
                        Url = "http://vg.no",
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a3"),
                        Volume = "1",
                        Year = "1901"
                    };

                    // Run the test against one instance of the context
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        await service.Post(reference).ConfigureAwait(false);
                    }

                    var replacementReference = new Reference()
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
                            new ReferenceUsage()
                                {UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a2"), ApplicationId = 2}
                        },
                        Summary = "Sum2",
                        Title = "Tiii2",
                        Url = "http://vg.no2",
                        UserId = new Guid("3ed89222-de9a-4df3-9e95-67f7fcac67a2"),
                        Volume = "2",
                        Year = "1902"
                    };
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        await service.Put(reference.Id, replacementReference).ConfigureAwait(false);
                    }

                    // Use a separate instance of the context to verify correct data was saved to database
                    using (var context = new ReferencesDbContext(options))
                    {
                        Assert.Equal(1, context.Reference.Count());
                        var service = new ReferencesController(context, index);
                        var getit = await service.Get(reference.Id).ConfigureAwait(false);
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
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [Fact]
        public void CanPostBulkReferences()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            using (var index = new Index())
            {
                try
                {

                    // Run the test against one instance of the context
                    using (var context = new ReferencesDbContext(options))
                    {
                        var service = new ReferencesController(context, index);
                        var result = service.PostMany(new[]
                            {new Reference() {Summary = "Ref1"}, new Reference() {Summary = "Ref2"}});
                    }

                    // Use a separate instance of the context to verify correct data was saved to database
                    using (var context = new ReferencesDbContext(options))
                    {
                        Assert.Equal(1, context.Reference.Where(x => x.Summary == "Ref1").Count());
                        Assert.Equal(1, context.Reference.Where(x => x.Summary == "Ref2").Count());
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [Fact]
        public void CanNotBulkPostReferencesWithIdenticalId()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            using (var index = new Index())
            {
                try
                {
                    using (var context = new ReferencesDbContext(options))
                    {
                        var id = Guid.NewGuid();
                        var service = new ReferencesController(context, index);
                        Assert.Throws<System.InvalidOperationException>(() => service.PostMany(new[]
                        {
                            new Reference() {Id = id, Summary = "Ref1"}, new Reference() {Id = id, Summary = "Ref2"}
                        }));
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [Fact]
        public async void CanNotPostReferencesWithIdenticalId()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            using (var index = new Index())
            {
                try
                {
                    using (var context = new ReferencesDbContext(options))
                    {
                        var id = Guid.NewGuid();
                        var service = new ReferencesController(context, index);
                        await service.Post(new Reference() {Id = id}).ConfigureAwait(false);
                        await Assert
                            .ThrowsAsync<InvalidOperationException>(() =>
                                service.Post(new Reference() {Id = id, Summary = "Ref1"})).ConfigureAwait(false);
                    }
                }
                finally
                {
                    connection.Close();
                }
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
