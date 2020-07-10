using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MerakiProbe;
using System.Configuration;
using MerakiHAWatch.Az;
using System.Collections.Generic;
using MerakiHAWatch.Model;
using Newtonsoft.Json;


namespace MerakiHAWatch
{
    public static class MerakiHA
    {
        [FunctionName("MerakiHA")]
        public static void Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            // Meraki dashboard parameters
            var apiKey = Environment.GetEnvironmentVariable("MerakiApiKey");                
            var networkId = Environment.GetEnvironmentVariable("MerakiNetworkId");
            var targetIp = Environment.GetEnvironmentVariable("MerakiTargetIp");
            var timespan = Environment.GetEnvironmentVariable("MerakiProbeTimespan");
            // UDR parameters
            AadCredential cred = new AadCredential()
            {
                 TenantId = Environment.GetEnvironmentVariable("AadTenantId"),
                 ClientId = Environment.GetEnvironmentVariable("AadClientId"),
                 ClientSecret = Environment.GetEnvironmentVariable("AadClientSecret")
            };
            var subscription = Environment.GetEnvironmentVariable("AzSubscriptionId");
            // route table parameters
            var udrrg = Environment.GetEnvironmentVariable("AzRouteTableResourceGroup");
            var routeTableName = Environment.GetEnvironmentVariable("AzRouteTableName");
            var routeNames = JsonConvert.DeserializeObject<List<object>>(Environment.GetEnvironmentVariable("AzRouteNames"));
            // Blob Storage Parameters
            var blobCnString = Environment.GetEnvironmentVariable("BlobCnString");
            var container = Environment.GetEnvironmentVariable("BlobContainerName");
            var blobname = Environment.GetEnvironmentVariable("BlobName");
            // Log Analytics parameters            
            var workspaceId = Environment.GetEnvironmentVariable("LogAnalyticsWorkspaceId");
            var workspaceKey = Environment.GetEnvironmentVariable("LogAnalyticsWorkspaceKey");
            // max loss percent
            var maxLossPercent = Environment.GetEnvironmentVariable("MaxLossPercent");
            // create all dependency injection objects
            IGetLossLatency probe = new ProbeAgent(apiKey,targetIp);
            IDefineRoute router = new RouteAgent(cred, subscription);
            IPersistFailover blob = new BlobAgent(blobCnString, container, blobname);
            ILog logger = new AzMonLogger(workspaceId, workspaceKey);
            // constructor
            List<RouteInfo> routes = new List<RouteInfo>();
            foreach (var routeName in routeNames)
            {
                routes.Add(new RouteInfo() {  
                    ResourceGroup= udrrg,
                    RouteTableName = routeTableName,
                    RouteName = routeName.ToString()
                });
            }
            MerakiHaAgent agent = new MerakiHaAgent(probe, router, blob, logger, routes);
            // probe and failover if needed
            var result = agent.ProbeAndFailover(int.Parse(maxLossPercent));
            log.LogInformation("Loss={0}, Failover={1}, Message={2}", result.lossPercent, result.Failover, result.Message ?? "OK");
        }
    }
}
