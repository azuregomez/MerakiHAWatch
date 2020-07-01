using System;
using System.Collections.Generic;
using System.Text;

namespace MerakiProbe
{
    public class LossLatency
    {
        public DateTime startTs { get; set; }
        public DateTime endTs { get; set; }
        public decimal lossPercent { get; set; }
        public decimal latencyMs { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
    }
}
