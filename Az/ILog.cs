using System;
using System.Collections.Generic;
using System.Text;

namespace MerakiHAWatch.Az
{
    public interface ILog
    {
        void Log(string logName, string json);
    }
}
