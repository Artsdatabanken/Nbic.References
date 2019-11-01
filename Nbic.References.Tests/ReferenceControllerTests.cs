using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nbic.References.Controllers;
using Nbic.References.EFCore;
using Nbic.References.Public.Models;
using Xunit;

namespace Nbic.References.Tests
{
    public class ReferenceControllerTests
    {
        [Fact]
        public async Task CanPostReference()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            try
            {
                
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context);
                    var result = service.Post(new Reference() {});
                }

                // Use a separate instance of the context to verify correct data was saved to database
                using (var context = new ReferencesDbContext(options))
                {
                    Assert.Equal(1, context.Reference.Count());
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public void CanPostAndReadCompleteReference()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);
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
                    ImportXml = "no",
                    Journal = "the",
                    Keywords = "natur,nett",
                    Lastname = "hoem",
                    Middlename = "Ari",
                    Pages = "1-3",
                    ReferenceUsage = new[] { new ReferenceUsage() { UserId = 1, ApplicationId = 1 } },
                    Summary = "Sum",
                    Title = "Tiii",
                    Url = "http://vg.no",
                    UserId = 1,
                    Volume = "1",
                    Year = "1901"
                };

                // Run the test against one instance of the context
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context);
                    var result = await service.PostAsync(new Reference() {}).ConfigureAwait(false);
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
                    Assert.Equal(it.ImportXml, reference.ImportXml);
                    Assert.Equal(it.Journal, reference.Journal);
                    Assert.Equal(it.Lastname, reference.Lastname);
                    Assert.Equal(it.Middlename, reference.Middlename);
                    Assert.Equal(it.Pages, reference.Pages);
                    Assert.Single(it.ReferenceUsage);
                    Assert.Equal(it.ReferenceUsage.First().ApplicationId, reference.ReferenceUsage.First().ApplicationId);
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

        [Fact]
        public void CanPostBulkReferences()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            try
            {

                // Run the test against one instance of the context
                using (var context = new ReferencesDbContext(options))
                {
                    var service = new ReferencesController(context);
                    var result = service.PostMany(new[] {new Reference() {Summary = "Ref1"}, new Reference() { Summary = "Ref2" } });
                }

                // Use a separate instance of the context to verify correct data was saved to database
                using (var context = new ReferencesDbContext(options))
                {
                    Assert.Equal(1, context.Reference.Where(x=>x.Summary == "Ref1").Count());
                    Assert.Equal(1, context.Reference.Where(x => x.Summary == "Ref2").Count());
                }
            }
            finally
            {
                connection.Close();
            }
        }
        [Fact]
        public void CanNotBulkPostReferencesWithIdenticalId()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            try
            {
                using (var context = new ReferencesDbContext(options))
                {
                    var id = Guid.NewGuid();
                    var service = new ReferencesController(context);
                    Assert.Throws<System.InvalidOperationException>(() => service.PostMany(new[] { new Reference() { Id = id, Summary = "Ref1" }, new Reference() { Id = id, Summary = "Ref2" } }));
                }
            }
            finally
            {
                connection.Close();
            }
        }
        [Fact]
        public void CanNotPostReferencesWithIdenticalId()
        {
            GetInMemoryDb(out SqliteConnection connection, out DbContextOptions<ReferencesDbContext> options);

            try
            {
                using (var context = new ReferencesDbContext(options))
                {
                    var id = Guid.NewGuid();
                    var service = new ReferencesController(context);
                    var result = service.Post(new Reference() {Id = id });
                    Assert.Throws<System.InvalidOperationException>(() => service.Post( new Reference() { Id = id, Summary = "Ref1" }));
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
