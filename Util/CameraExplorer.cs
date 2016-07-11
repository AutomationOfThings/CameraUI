
using LCM.LCM;
using Microsoft.Practices.Prism.PubSubEvents;
using NotificationCenter;
using ptz_camera;

using System.Collections.Generic;


namespace Util {
    public class CameraExplorer {

        private readonly LCM.LCM.LCM _lcm;
        private EventAggregator _ea;

        List<CameraInfo> camList;

        public CameraExplorer(List<CameraInfo> camList) {
            this.camList = camList;
            _lcm = LCM.LCM.LCM.Singleton;
            _ea = Notification.Instance;
            _ea.GetEvent<CameraDiscoverEvent>().Subscribe(discover);
            _ea.GetEvent<CameraDiscoverShortCutEvent>().Subscribe(discover);
            _ea.GetEvent<DiscoveryResponseReceivedEvent>().Subscribe(OnGetDiscoveryResponse);
            
        }

        public void discover(string input) {
            _ea.GetEvent<StatusUpdateEvent>().Publish("Discovering...");
            discovery_request_t discoveryRequest = new discovery_request_t();
            _lcm.Publish(Channels.discovery_req_channel, discoveryRequest);
            _lcm.Subscribe(Channels.discovery_res_channel, new DiscoveryResponseHandler());
        }

        private void OnGetDiscoveryResponse(discovery_response_t res) {
            camList.Clear();
            List<string> temp = new List<string>();
            foreach (string ip in res.camera_names) {
                if (!temp.Contains(ip)) {
                    temp.Add(ip);
                    CameraInfo cam = new CameraInfo(ip, ip, 0, 0, 1);
                    camList.Add(cam);
                }
            }
            
            _ea.GetEvent<StatusUpdateEvent>().Publish("Camera discovery finished");
            _ea.GetEvent<CameraDiscoveredEvent>().Publish(camList);
        }

    }

    public class DiscoveryResponseHandler: LCMSubscriber {
        public void MessageReceived(LCM.LCM.LCM lcm, string channel, LCMDataInputStream data_stream) {
            if (channel == Channels.discovery_res_channel) {
                discovery_response_t response = new discovery_response_t(data_stream);
                Notification.Instance.GetEvent<DiscoveryResponseReceivedEvent>().Publish(response);
            }
        }
    }
}
