using System;
using SandBox.DbContextSource;

namespace SandBox
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;

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
            
            foreach (var item in db.RfReference)
            {
                Console.Write(item.PkReferenceId + ",");
                var reference = MapToReference(item);
                var post = api.PostAsync(reference);
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
                                      UserId = item.FkUserId,
                                      ImportXml = item.ImportXml,
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
                                     Year = item.Year
                                  };
            return reference;
        }

        private static void GetApiToken()
        {
            // discover endpoints from metadata
            using (var client = new HttpClient())
            {
                var discoveryDocumentAsync = client.GetDiscoveryDocumentAsync("https://demo.identityserver.io/");
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
                            Address = discoveryResponse.TokenEndpoint, ClientId = "client", ClientSecret = "secret", Scope = "api"
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
