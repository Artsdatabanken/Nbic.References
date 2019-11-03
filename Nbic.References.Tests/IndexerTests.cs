using System;
using System.Collections.Generic;
using System.Text;
using Nbic.References.Public.Models;
using Xunit;

namespace Nbic.References.Tests
{
    public class IndexerTests
    {
        [Fact]
        public void CanIndexAnDocument()
        {
            using (var index = new Nbic.Indexer.Index())
            {
                var newGuid = Guid.Parse("b1df2d4b-e06a-421d-865b-03f3dfdf6913");
                index.AddOrUpdate(new Reference() {Id = newGuid, Title = "Creepy" });
                var result = index.SearchReference("Creepy");
                Assert.Equal(result, newGuid);
                index.AddOrUpdate(new Reference() { Id = newGuid, Title = "Snoopy Dog" });
                result = index.SearchReference("Creepy");
                Assert.Equal(result, Guid.Empty);
                result = index.SearchReference("Snoopy Dog");
                Assert.Equal(result, newGuid);
                index.Delete(newGuid);
                result = index.SearchReference("Snoopy Dog");
                Assert.Equal(result, Guid.Empty);
            }
        }
    }
}
