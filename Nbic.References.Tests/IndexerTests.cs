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
                index.AddOrUpdate(new Reference() { Id = newGuid, Title = "Creepy" });
                var result = index.SearchReference("Creepy", 0, 10);
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
        [Fact]
        public void CanFindReidarElvenReferanseDocument()
        {
            using (var index = new Nbic.Indexer.Index())
            {
                var referanse = new Reference()
                                    {
                                        Id = Guid.Parse("208daeb0-a917-45cd-9b0f-fa21f4300d01"),
                                        ApplicationId = 8,
                                        UserId = 123,
                                        Author = "Elven, R.",
                                        Year = "1980",
                                        Title = null,
                                        Summary = null,
                                        Journal = null,
                                        Volume = null,
                                        Pages = null,
                                        Bibliography =
                                            "Elven, R. 1980. Association analysis of moraine vegetation at the glacier Hardangerjökulen, Finse, South Norway. - Norw. J. Bot. 25: 171-191.",
                                        Lastname = null,
                                        Middlename = null,
                                        Firstname = null,
                                        Url = null,
                                        Keywords = "Fje Veg Ass Dyn NNNd#2",
                                        ImportXml = null,
                                        EditDate = DateTime.Parse("2010-04-26T08:09:18.613")
                                    };

                index.AddOrUpdate(referanse);
                var result = index.SearchReference("elven", 0, 10);
                Assert.Equal(result.Count(), 1);
                result = index.SearchReference("elven Association", 0, 10);
                Assert.Equal(result.Count(), 1);
                result = index.SearchReference("elven. R.", 0, 10);
                Assert.Equal(result.Count(), 1);

            }
        }

        [Fact]
        public void CanFindTheRightReidarElvenReferanseDocument()
        {
            using (var index = new Nbic.Indexer.Index())
            {
                var referanse = new Reference()
                {
                    Id = Guid.Parse("208daeb0-a917-45cd-9b0f-fa21f4300d01"),
                    ApplicationId = 8,
                    UserId = 123,
                    Author = "Elven, R.",
                    Year = "1980",
                    Title = null,
                    Summary = null,
                    Journal = null,
                    Volume = null,
                    Pages = null,
                    Bibliography =
                                            "Elven, R. 1980. Association analysis of moraine vegetation at the glacier Hardangerjökulen, Finse, South Norway. - Norw. J. Bot. 25: 171-191.",
                    Lastname = null,
                    Middlename = null,
                    Firstname = null,
                    Url = null,
                    Keywords = "Fje Veg Ass Dyn NNNd#2",
                    ImportXml = null,
                    EditDate = DateTime.Parse("2010-04-26T08:09:18.613")
                };
                var referans2 = new Reference()
                {
                    Id = Guid.Parse("208daeb0-a917-45cd-9b0f-fa21f4300d02"),
                    ApplicationId = 8,
                    UserId = 123,
                    Author = "Elven, R.",
                    Year = "1981",
                    Title = null,
                    Summary = null,
                    Journal = null,
                    Volume = null,
                    Pages = null,
                    Bibliography =
                        "Elven, R. 1981. Association analysis of moraine vegetation at the glacier Hardangerjökulen, Finse, South Norway. - Norw. J. Bot. 25: 171-191.",
                    Lastname = null,
                    Middlename = null,
                    Firstname = null,
                    Url = null,
                    Keywords = "Fje Veg Ass Dyn NNNd#2",
                    ImportXml = null,
                    EditDate = DateTime.Parse("2010-04-26T08:09:18.613")
                };

                index.AddOrUpdate(referanse);
                index.AddOrUpdate(referans2);

                var result = index.SearchReference("elven", 0, 10);
                Assert.Equal(result.Count(), 2);
                result = index.SearchReference("elven Association", 0, 10);
                Assert.Equal(result.Count(), 2);
                result = index.SearchReference("elven 1981", 0, 10);
                Assert.Equal(result.Count(), 1);

            }
        }
    }
}
