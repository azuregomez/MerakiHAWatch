using System;
using System.Collections.Generic;
using System.Text;

namespace MerakiHAWatch.Model
{
    public class VmxNode { 
        public string resourceGroup { get; set; }
        public string vmName { get; set; }
        public string networkId { get; set; }
        public string deviceSerial { get; set; }
        public string privateIp { get; set; }
    }

    public class VmxHaPair
    {
        public VmxNode activeNode { get; set; }
        public VmxNode standByNode { get; set; }
    }
    
}
