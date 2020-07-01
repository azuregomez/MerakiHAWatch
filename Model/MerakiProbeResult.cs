using System;
using System.Collections.Generic;
using System.Text;

namespace MerakiHAWatch.Model
{
    public class MerakiProbeResult
    {
        public decimal lossPercent { get; set; }
        public decimal latencyMs { get; set; }
        public string ActiveVmName { get; set; }
        public bool Failover { get; set; }        

        public string Message { get; set; }
        
    }
}
