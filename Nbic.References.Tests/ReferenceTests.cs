using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nbic.References.Public.Models;
using Xunit;

namespace Nbic.References.Tests
{
    public class ReferenceTests
    {
        [Fact]
        public async Task FixParantesesAroundYearInReference()
        {
            var reference = new Reference
            {
                Author = "Hoem",
                Year = "2020", // uten parantes
                Title = "A title", // uten punktum
                Journal = "Blang yournal",
                Volume = "1a",
                Pages = "1-12" // uten punktum
            };
            Assert.Equal("Hoem (2020). A title. Blang yournal 1a: 1-12.", reference.ReferencePresentation);
        }

        [Fact]
        public async Task NotFixParantesesIfOkInReference()
        {
            var reference = new Reference
            {
                Author = "Hoem",
                Year = "(2020)", // med parantes
                Title = "A title.", // med punktum
                Journal = "Blang yournal",
                Volume = "1a",
                Pages = "1-12." // med punktum
            };
            Assert.Equal("Hoem (2020). A title. Blang yournal 1a: 1-12.", reference.ReferencePresentation);
        }
    }
}
