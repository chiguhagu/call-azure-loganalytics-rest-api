using System;
using System.IO;
using System.Net.Http;
using Microsoft.Azure.OperationalInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest.Azure.Authentication;

namespace call_azure_loganalytics_rest_api
{
    class Program
    {
        HttpClient _httpClient = new HttpClient();

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var tenantId        = configuration["tenantId"];
            var clientId        = configuration["clientId"];
            var secret          = configuration["secret"];
            var workspaceId     = configuration["workspaceId"];

            var adSettingsForLoganalytics = new ActiveDirectoryServiceSettings
            {
                AuthenticationEndpoint = new Uri("https://login.microsoftonline.com"),
                TokenAudience = new Uri("https://api.loganalytics.io/"),
                ValidateAuthority = true
            };

            // Build the service credentials and Monitor client
            var serviceCreds = ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, secret, adSettingsForLoganalytics).Result;

            using (var logAnalyticsClient = new OperationalInsightsDataClient(serviceCreds))
            {
                logAnalyticsClient.WorkspaceId = workspaceId;

                var queryResult = logAnalyticsClient.Query("KubePodInventory");
                foreach(var result in queryResult.Results){
                    Console.WriteLine(result.Count + ": {" + result.Keys.ToString() + ", " + result.Values.ToString() + "}");
                }
            }

            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
