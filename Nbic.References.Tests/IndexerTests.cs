using System;
using System.Collections.Generic;
using System.Linq;
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
                var result = index.SearchReference("Creepy",0, 10);
                Assert.Equal(result.Count(), 1);
                index.AddOrUpdate(new Reference() { Id = newGuid, Title = "Snoopy Dog" });
                result = index.SearchReference("Creepy", 0, 10);
                Assert.Equal(result.Count(), 0);
                result = index.SearchReference("Snoopy Dog", 0, 10);
                Assert.Equal(result.Count(), 1);
                index.Delete(newGuid);
                result = index.SearchReference("Snoopy Dog", 0, 10);
                Assert.Equal(result.Count(), 0);
            }
        }
    }
}
