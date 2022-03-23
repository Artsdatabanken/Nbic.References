using System;
using SandBox.DbContextSource;
using System.Linq;
namespace SandBox
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json.Linq;

    using SandBox.ApiContext;

    using RfReference = SandBox.DbContextSource.RfReference;

    class Program
    {
        private static HttpClient _apiClient = new HttpClient();

        
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "migrate":
                    //migrate
                    using (var db = new ArtsdatabankenSIContext(args[1]))
                    {
                        var apiEndPoint = args[2];

                        MigrateFromSqlToApi(db, apiEndPoint);
                    }

                    break;
            }
        }

        private static void MigrateFromSqlToApi(ArtsdatabankenSIContext db, string apiEndPoint)
        {
            GetApiToken();

            //migrate references
            var api = new ApiContext.Client(apiEndPoint, _apiClient);
            var batch = new List<Reference>();
            foreach (var item in db.RfReference.Include(Reference => Reference.RfReferenceUsage))
            {
                Console.Write(item.PkReferenceId + ",");
                var reference = MapToReference(item);
                batch.Add(reference);
                if (batch.Count() > 50)
                {
                    var post = api.BulkAsync(batch);
                    post.Wait();
                    batch = new List<Reference>();
                }
            }
            if (batch.Count() > 0)
                {
                    var post = api.BulkAsync(batch);
                    post.Wait();
                }
        }

        private static ApiContext.Reference MapToReference(RfReference item)
        {
            var reference = new ApiContext.Reference
                                  {
                                      ApplicationId = item.ApplicationId,
                                      Author = item.Author,
                                      Bibliography = item.Bibliography,
                                      EditDate = item.EditDate,
                                      Firstname = item.Firstname,
                                      UserId = CreateGuid(item.FkUserId),
                                    //   ImportXml = item.ImportXml,
                                      Journal = item.Journal,
                                      Keywords = item.Keywords,
                                      Lastname = item.Lastname,
                                      Middlename = item.Middlename,
                                      Pages = item.Pages,
                                      Id = item.PkReferenceId,
                                     Summary = item.Summary,
                                     Title = item.Title,
                                     Url = item.Url,
                                     Volume = item.Volume,
                                     Year = item.Year,
                                     ReferenceUsage = item.RfReferenceUsage.Select(ru => 
                                     new ReferenceUsage(){
                                         ReferenceId = ru.FkReferenceId, ApplicationId = ru.FkApplicationId, UserId = CreateGuid(ru.FkUserId)
                                         }).ToArray()
                                  };
            return reference;
        }

        private static Guid CreateGuid(int? fkUserId)
        {
            if(fkUserId.HasValue == false) return new Guid("00000000-0000-0000-0000-000000000000");
            return new Guid("00000000-0000-0000-0000-000000000000".Substring(0, 36 - fkUserId.ToString().Length) + fkUserId.ToString() );
        }

        private static void GetApiToken()
        {
            // discover endpoints from metadata
            using (var client = new HttpClient())
            {
                var discoveryDocumentAsync = client.GetDiscoveryDocumentAsync("https://id.artsdatabanken.no/");
                discoveryDocumentAsync.Wait();
                var discoveryResponse = discoveryDocumentAsync.Result;

                if (discoveryResponse.IsError)
                {
                    Console.WriteLine(discoveryResponse.Error);
                    throw new Exception(discoveryResponse.Error);
                }

                // request token
                //var clientSecret = "secret".ToSha256();
                var requestClientCredentialsTokenAsync = client.RequestClientCredentialsTokenAsync(
                    new ClientCredentialsTokenRequest
                        {
                            Address = discoveryResponse.TokenEndpoint, ClientId = "references_sandbox_client", ClientSecret = "*", Scope = "references_api"
                        });
                requestClientCredentialsTokenAsync.Wait();
                var tokenResponse = requestClientCredentialsTokenAsync.Result;

                if (tokenResponse.IsError)
                {
                    Console.WriteLine(tokenResponse.Error);
                    throw new Exception(tokenResponse.Error);
                }

                Console.WriteLine(tokenResponse.Json);
                Console.WriteLine("\n\n");

                _apiClient.SetBearerToken(tokenResponse.AccessToken);
            }
        }
    }
}
