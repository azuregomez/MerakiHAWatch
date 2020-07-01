using System;
using System.Collections.Generic;
using System.Text;

namespace MerakiHAWatch.Az
{
    public class AadCredential
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
