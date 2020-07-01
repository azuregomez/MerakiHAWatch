using MerakiHAWatch.Az;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Text;

namespace MerakiProbe
{
    public class ProbeAgent: IGetLossLatency
    {        
        private long _timespan;
        private long _resolution;
        private string _ip;
        private string _apikey;

        /// <summary>
        /// Probe constructor that will take common params for a pair of Merakis
        /// </summary>
        /// <param name="networkId">NetworkId of the vMX</param>
        /// <param name="timespan">The timespan for which the information will be fetched.
        ///     If specifying timespan, do not specify parameters t0 and t1.
        ///     The value must be in seconds and be less than or equal to 31 days.The Cisco default is 1 day.
        ///     This implementation default is 120 seconds because the probe will run every 2 minutes</param>
        /// <param name="resolution">The time resolution in seconds for returned data. 
        ///         The valid resolutions are: 60, 600, 3600, 86400. The default is 60.</param>
        /// <param name="ip">IP to probe</param>
        public ProbeAgent(string apikey, string ip, long timespan=120, long resolution=60)
        {
            if(String.IsNullOrEmpty(apikey) || String.IsNullOrEmpty(ip))
            {
                throw new Exception("API Key and target IP are needed to probe vMX");
            }
            _apikey = apikey;            
            _timespan = timespan;
            _resolution = resolution;
            _ip = ip;            
        }
        /// <summary>
        /// Gets Loss and Latency for a particular device
        /// </summary>
        /// <param name="deviceSerial"></param>
        /// <returns></returns>
        public LossLatency GetLossLatency(string networkId, string deviceSerial)
        {            
            // Make URL            
            var merakiDashboardUrl = String.Format("https://api.meraki.com/api/v0/networks/N_{0}/devices/{1}/lossAndLatencyHistory?uplink=wan1&ip={2}&timespan={3}&resolution={4}", networkId, deviceSerial, _ip, _timespan, _resolution);
            RestClient restClient = new RestClient(merakiDashboardUrl);
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-Cisco-Meraki-API-Key", _apikey);
            request.AddHeader("Content-Type", "application/json");
            var jsonresponse = restClient.Execute(request);
            if (jsonresponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                // not OK then return error
                return new LossLatency()
                {
                    startTs = DateTime.Now,
                    endTs = DateTime.Now,
                    lossPercent = 100,
                    Error = true,
                    ErrorMessage = jsonresponse.ErrorMessage
                };
            }
            // expecting a json array
            // https://dashboard.meraki.com/api_docs/v0#get-the-uplink-loss-percentage-and-latency-in-milliseconds-for-a-wired-network-device
            var lossLatencyArray = JsonConvert.DeserializeObject<LossLatency[]>(jsonresponse.Content);
            if (lossLatencyArray.Length == 0)
            {
                return new LossLatency()
                {
                    startTs = DateTime.Now,
                    endTs = DateTime.Now,
                    lossPercent = 100,
                    Error = true,
                    ErrorMessage = "No data from probe. Device likely down"
                };
            }
            return lossLatencyArray[lossLatencyArray.Length-1];
            
        }
    }
}
