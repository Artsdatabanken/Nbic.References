// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Net.Http;
// using System.Text;
// using IdentityModel.Client;
// using Xunit;
//
// namespace Nbic.References.Tests
// {
//     public class AuthenticationTest
//     {
//         //[Fact]
//         //public async void CanNotDeleteNonExistingDoc()
//         //{
//         //    // discover endpoints from metadata
//         //    var client = new HttpClient();
//         //    var disco = await client.GetDiscoveryDocumentAsync("https://id.test.artsdatabanken.no");
//         //    Assert.Equal(disco.IsError, false);
//
//         //    if (disco.IsError)
//         //    {
//         //        Console.WriteLine(disco.Error);
//         //        return;
//         //    }
//
//         //    var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
//         //    {
//         //        Address = disco.TokenEndpoint,
//
//         //        ClientId = "ReferencesApiClient",
//         //        ClientSecret = "478f8f8d-b9b9-3bb1-219a-8bc2e3c2c949",
//         //        Scope = "references_api"
//         //    });
//
//         //    Assert.Equal(tokenResponse.IsError, false);
//
//         //    if (tokenResponse.IsError)
//         //    {
//         //        Console.WriteLine(tokenResponse.Error);
//         //        return;
//         //    }
//
//         //    Console.WriteLine(tokenResponse.Json);
//
//         //    // call api
//         //    var client2 = new HttpClient();
//         //    client2.SetBearerToken(tokenResponse.AccessToken);
//
//         //    var response = await client2.DeleteAsync("https://referenceapi.artsdatabanken.no/api/References/8041b97a-5d18-422b-904b-a4407cfd5a85");
//         //    Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
//         //    if (!response.IsSuccessStatusCode)
//         //    {
//         //        Console.WriteLine(response.StatusCode);
//         //    }
//         //    else
//         //    {
//         //        var content = await response.Content.ReadAsStringAsync();
//         //        Assert.True(content.Length > 100);
//         //        //Console.WriteLine(JArray.Parse(content));
//         //    }
//         //}
//     }
// }
