using System;
using Nbic.References.Core.Models;
using Xunit;

namespace Nbic.References.Tests;

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
        var reference = new Reference
        {
            ApplicationId = 25,
            Id = new Guid("b0b008c0-a6df-4b9a-a7a4-7360976b659c"),
            ReferenceString =
                "Roth S og Coulianos C-C (2014) A survey of aquatic and terrestrial Heteroptera in northern Europe with special regard to Finnmark, Norway (and adjacent regions). Norwegian Journal of Entomology 61 (1): 99–116.",
            Url = "http://www.entomologi.no/journals/nje/2014-1/pdf/nje-vol61-no1-roth.pdf"

        };
        Assert.Equal("Roth S og Coulianos C-C (2014) A survey of aquatic and terrestrial Heteroptera in northern Europe with special regard to Finnmark, Norway (and adjacent regions). Norwegian Journal of Entomology 61 (1): 99–116. http://www.entomologi.no/journals/nje/2014-1/pdf/nje-vol61-no1-roth.pdf", reference.ReferencePresentation);
    }

    [Fact]
    public void ReferenceType_ShouldReturnCorrectType()
    {
        // Test case 1: ReferenceString is set
        var reference1 = new Reference
        {
            ReferenceString = "Some reference string"
        };
        Assert.Equal("Reference", reference1.ReferenceType);

        // Test case 2: Author is set
        var reference2 = new Reference
        {
            Author = "Some Author"
        };
        Assert.Equal("Publication", reference2.ReferenceType);

        // Test case 3: Firstname is set
        var reference3 = new Reference
        {
            Firstname = "John"
        };
        Assert.Equal("Person", reference3.ReferenceType);

        // Test case 4: Url is set
        var reference4 = new Reference
        {
            Url = "http://example.com"
        };
        Assert.Equal("Url", reference4.ReferenceType);

        // Test case 5: None of the above properties are set
        var reference5 = new Reference();
        Assert.Equal("All", reference5.ReferenceType);
    }
}
