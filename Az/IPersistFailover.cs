using MerakiHAWatch.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MerakiHAWatch.Az
{
    public interface IPersistFailover
    {
        VmxHaPair GetNodes();
        void SaveNodes(VmxHaPair record);
    }
}
