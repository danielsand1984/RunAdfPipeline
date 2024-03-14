using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RunAdfPipeline
{
    public static class StartURL
    {
        [FunctionName("StartRun")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string tenant           = req.Headers["tenant"];
            string subscription     = req.Headers["subscription"];
            string resourceGroup    = req.Headers["resourcegroup"];
            string adf              = req.Headers["adf"];
            string pipeline         = req.Headers["pipeline"];
            string client           = req.Headers["client"];
            string secret           = req.Headers["secret"];

            var resourceUrl = "https://management.azure.com/";
            var requestUrl = $"https://login.microsoftonline.com/{tenant}/oauth2/token";

            // in real world application, please use Typed HttpClient from ASP.NET Core DI
            var httpClient = new HttpClient();

            var dict = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", client },
                    { "client_secret", secret },
                    { "resource", resourceUrl },
                    { "scope", resourceUrl + "user_impersonation"}
                };

            var requestBody = new FormUrlEncodedContent(dict);
            var response = await httpClient.PostAsync(requestUrl, requestBody);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var aadToken = JsonSerializer.Deserialize<AzureADToken>(responseContent);

            string token = aadToken?.AccessToken;

            httpClient = new HttpClient();
            requestUrl = $"https://management.azure.com/subscriptions/{subscription}/resourcegroups/{resourceGroup}/providers/microsoft.datafactory/factories/{adf}/pipelines/{pipeline}/createrun?api-version=2018-06-01";
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            httpRequest.Headers.Add("Authorization", $"bearer {token}");
            var test = await httpClient.SendAsync(httpRequest);

            return new OkObjectResult(test.ReasonPhrase);
        }
    }
    public static class StartRunURL
    {
        [FunctionName("StartRunURL")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string tenant           = req.Query["tenant"];
            string subscription     = req.Query["subscription"];
            string resourceGroup    = req.Query["resourcegroup"];
            string adf              = req.Query["adf"];
            string pipeline         = req.Query["pipeline"];
            string client           = req.Query["client"];
            string secret           = req.Query["secret"];

            var resourceUrl = "https://management.azure.com/";
            var requestUrl = $"https://login.microsoftonline.com/{tenant}/oauth2/token";

            // in real world application, please use Typed HttpClient from ASP.NET Core DI
            var httpClient = new HttpClient();

            var dict = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", client },
                    { "client_secret", secret },
                    { "resource", resourceUrl },
                    { "scope", resourceUrl + "user_impersonation"}
                };

            var requestBody = new FormUrlEncodedContent(dict);
            var response = await httpClient.PostAsync(requestUrl, requestBody);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var aadToken = JsonSerializer.Deserialize<AzureADToken>(responseContent);

            string token = aadToken?.AccessToken;

            httpClient = new HttpClient();
            requestUrl = $"https://management.azure.com/subscriptions/{subscription}/resourcegroups/{resourceGroup}/providers/microsoft.datafactory/factories/{adf}/pipelines/{pipeline}/createrun?api-version=2018-06-01";
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            httpRequest.Headers.Add("Authorization", $"bearer {token}");
            var test = await httpClient.SendAsync(httpRequest);

            return new OkObjectResult(test.ReasonPhrase);
        }
    }
    public class AzureADToken
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public string ExpiresIn { get; set; }

        [JsonPropertyName("ext_expires_in")]
        public string ExtExpiresIn
        {
            get; set;
        }

        [JsonPropertyName("expires_on")]
        public string ExpiresOn { get; set; }

        [JsonPropertyName("not_before")]
        public string NotBefore { get; set; }

        public string Resource { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }
}
