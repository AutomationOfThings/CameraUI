
using LCM.LCM;
using Microsoft.Practices.Prism.PubSubEvents;
using NotificationCenter;
using ptz_camera;
using System;
using System.Collections.ObjectModel;


namespace Util {
    class CameraExplorer {

        private readonly LCM.LCM.LCM _lcm;
        private EventAggregator _ea;

        ObservableCollection<CameraInfo> camList;

        public CameraExplorer(ObservableCollection<CameraInfo> camList) {
            this.camList = camList;
            _lcm = LCM.LCM.LCM.Singleton;
            _ea = Notification.Instance;
            _ea.GetEvent<DiscoveryResponseReceivedEvent>().Subscribe(OnGetDiscoveryResponse);
        }

        public void discover() {
            discovery_request_t discoveryRequest = new discovery_request_t();
            _lcm.Publish(Channels.DiscoveryReqChannel, discoveryRequest);
            _lcm.Subscribe(Channels.DiscoveryReqChannel, new DiscoveryResponseHandler());
        }

        private void OnGetDiscoveryResponse(discovery_response_t res) {
            foreach (string cam in res.camera_names) {
                // add cam into camlist
            }
        }

    }

    public class DiscoveryResponseHandler: LCMSubscriber {
        public void MessageReceived(LCM.LCM.LCM lcm, string channel, LCMDataInputStream data_stream) {
            if (channel == Channels.DiscoveryResChannel) {
                discovery_response_t response = new discovery_response_t(data_stream);
                Notification.Instance.GetEvent<DiscoveryResponseReceivedEvent>().Publish(response);
            }
        }
    }
}
