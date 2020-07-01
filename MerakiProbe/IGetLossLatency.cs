using System;
using System.Collections.Generic;
using System.Text;

namespace MerakiProbe
{
    public interface IGetLossLatency
    {
        LossLatency GetLossLatency(string networkId, string deviceSerial);
    }
}
