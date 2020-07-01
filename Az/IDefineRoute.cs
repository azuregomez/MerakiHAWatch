using Microsoft.Azure.Management.Network.Fluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace MerakiHAWatch.Az
{
    public interface IDefineRoute
    {
        public void UpdateRoute(string resourceGroup, string routeTableName, string routeName, string nextHop);
        public IRoute GetRoute(string resourceGroup, string routeTableName, string routeName);

    }
}
