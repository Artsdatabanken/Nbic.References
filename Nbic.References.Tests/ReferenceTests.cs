using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Nbic.References.Public.Models;
using Xunit;

namespace Nbic.References.Tests
{
    public class ReferenceTests
    {
        [Fact]
        public void FixParantesesAroundYearInReference()
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
        public void NotFixParantesesIfOkInReference()
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

        [Fact]
        public void UrlShouldBeIncludedInReference()
        {
            var reference = new Reference()
            {
                ApplicationId = 25,
                Id = new Guid("b0b008c0-a6df-4b9a-a7a4-7360976b659c"),
                ReferenceString =
                    "Roth S og Coulianos C-C (2014) A survey of aquatic and terrestrial Heteroptera in northern Europe with special regard to Finnmark, Norway (and adjacent regions). Norwegian Journal of Entomology 61 (1): 99–116.",
                Url = "http://www.entomologi.no/journals/nje/2014-1/pdf/nje-vol61-no1-roth.pdf"

            };
            Assert.Equal("Roth S og Coulianos C-C (2014) A survey of aquatic and terrestrial Heteroptera in northern Europe with special regard to Finnmark, Norway (and adjacent regions). Norwegian Journal of Entomology 61 (1): 99–116. http://www.entomologi.no/journals/nje/2014-1/pdf/nje-vol61-no1-roth.pdf", reference.ReferencePresentation);
        }
    }
}
