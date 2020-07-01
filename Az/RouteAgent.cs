using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MerakiHAWatch.Az
{
    public class RouteAgent: IDefineRoute
    {
        private IAzure _azure;
        private string _subscription;
        public RouteAgent(AadCredential cred, string subscription)
        {
            if (null == cred || String.IsNullOrEmpty(cred.ClientId) || String.IsNullOrEmpty(cred.ClientSecret) || String.IsNullOrEmpty(subscription))
            {
                throw new Exception("Cannot Update UDR without AAD credential and subscription");
            }
            _subscription = subscription;            
            // Login to Az in constructor
            var creds = SdkContext.AzureCredentialsFactory.FromServicePrincipal(cred.ClientId, cred.ClientSecret, cred.TenantId, AzureEnvironment.AzureGlobalCloud);
            _azure = Microsoft.Azure.Management.Fluent.Azure.Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(creds)
                .WithSubscription(subscription);                
        }

        public void UpdateRoute(string resourceGroup, string routeTableName, string routeName, string nextHop)
        {
            string routeTableId = String.Format("/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Network/routeTables/{2}",_subscription,resourceGroup,routeTableName);
            _azure.RouteTables.GetById(routeTableId).Update()
                .UpdateRoute(routeName)
                .WithNextHopToVirtualAppliance(nextHop)
                .Parent()
                .Apply();
        }

        public IRoute GetRoute(string resourceGroup, string routeTableName, string routeName)
        {
            string routeTableId = String.Format("/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Network/routeTables/{2}", _subscription, resourceGroup, routeTableName);
            return _azure.RouteTables.GetById(routeTableId).Routes[routeName];
        }

    }
}
