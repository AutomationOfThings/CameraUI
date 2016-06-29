using Microsoft.Practices.Prism.PubSubEvents;
using NotificationCenter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;

namespace Camera {
    public class CameraListVM {

        EventAggregator _ea;

        List<CameraInfo> camInfoList;

        public ObservableCollection<CameraVM> CamList { get; set; }

        public ModeColors modeColors { get; set; }

        public CameraListVM(List<CameraInfo> camInfoList) {
            _ea = Notification.Instance;
            modeColors = ModeColors.Singleton(_ea);
            this.camInfoList = camInfoList;
            CamList = new ObservableCollection<CameraVM>();

            for (int i = 0; i < camInfoList.Count; i++) {
                CameraVM vm = new CameraVM(camInfoList[i], modeColors, _ea);
                CamList.Add(vm);
            }

            _ea.GetEvent<CameraDiscoverEvent>().Subscribe(discoverCameras);
            _ea.GetEvent<CameraDiscoverShortCutEvent>().Subscribe(discoverCameras);
        }

        private void discoverCameras(string input) {
            // send reqeust to discover cameras asynchronously
        }

        public void updateCamList() {
            for (int i = 0; i < camInfoList.Count; i++) {
                CameraVM vm = new CameraVM(camInfoList[i], modeColors, _ea);
                CamList.Add(vm);
            }
        }
    }
}
