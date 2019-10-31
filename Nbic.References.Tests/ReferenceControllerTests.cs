using System;
using System.Linq;
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
        public void CanPostReference()
        {
            // In-memory database only exists while the connection is open
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
               var  options = new DbContextOptionsBuilder<ReferencesDbContext>()
                    .UseSqlite(connection)
                    .Options;

                // Create the schema in the database
                using (var context = new ReferencesDbContext(options))
                {
                    context.Database.EnsureCreated();
                }

                // Run the test against one instance of the context
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
        public void CanPostBulkReferences()
        {
            // In-memory database only exists while the connection is open
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ReferencesDbContext>()
                    .UseSqlite(connection)
                    .Options;

                // Create the schema in the database
                using (var context = new ReferencesDbContext(options))
                {
                    context.Database.EnsureCreated();
                }

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
    }
}
