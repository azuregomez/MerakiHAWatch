using MerakiHAWatch.Az;
using MerakiHAWatch.Model;
using MerakiProbe;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MerakiHAWatch
{
    public class MerakiHaAgent
    {
        private ILog _log;
        private IDefineRoute _router;
        private IGetLossLatency _probe;
        private IPersistFailover _blob;
        private List<RouteInfo> _routes;

        const string LogName = "MerakiProbe";        

        public MerakiHaAgent(IGetLossLatency probe, IDefineRoute router, IPersistFailover blob, ILog log, List<RouteInfo> routes)
        {
            _log = log;
            _router = router;
            _probe = probe;
            _blob = blob;
            _routes = routes;
        }

        public bool CanProbeActive(int maxLossPercent)
        {
            // Get HA Pair from blob storage
            var hapair = _blob.GetNodes();

            // probe standby node
            var probe = _probe.GetLossLatency(hapair.activeNode.networkId, hapair.activeNode.deviceSerial);

            return ((probe.lossPercent > maxLossPercent) ? false : true);
        }

        public bool CanProbeStandby(int maxLossPercent)
        {
            // Get HA Pair from blob storage
            var hapair = _blob.GetNodes();

            // probe standby node
            var probe = _probe.GetLossLatency(hapair.standByNode.networkId, hapair.standByNode.deviceSerial);

            return ((probe.lossPercent > maxLossPercent) ? false : true);
        }

        public MerakiProbeResult Failover(int maxLossPercent)
        {
            // Get HA Pair from blob storage
            var hapair = _blob.GetNodes();

            // probe active node
            var probe = _probe.GetLossLatency(hapair.activeNode.networkId, hapair.activeNode.deviceSerial);
            
            var logentry = new MerakiProbeResult()
            {
                latencyMs = probe.latencyMs,
                lossPercent = probe.lossPercent,
                ActiveVmName = hapair.activeNode.vmName,
                Failover=false
            };

            if (probe.lossPercent > maxLossPercent)
            {
                // failover updating all route tables
                foreach(RouteInfo route in _routes)
                {
                    _router.UpdateRoute(route.ResourceGroup, route.RouteTableName, route.RouteName, hapair.standByNode.privateIp);                    
                }                
                var newhapair = new VmxHaPair()
                {
                    activeNode = hapair.standByNode,
                    standByNode = hapair.activeNode
                };
                _blob.SaveNodes(newhapair);
                logentry.Failover = true;
                logentry.Message = probe.ErrorMessage;
            }
            var json = JsonConvert.SerializeObject(logentry);
            _log.Log(LogName, json);
            return logentry;
        }

    }
}
