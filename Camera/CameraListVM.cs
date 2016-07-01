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
            _ea.GetEvent<CameraDiscoveredEvent>().Subscribe(updateCamList);

            modeColors = ModeColors.Singleton(_ea);
            this.camInfoList = camInfoList;
            CamList = new ObservableCollection<CameraVM>();

            updateCamList(camInfoList);
        }



        public void updateCamList(List<CameraInfo> list) {
            CamList.Clear();
            for (int i = 0; i < camInfoList.Count; i++) {
                CameraVM vm = new CameraVM(camInfoList[i], modeColors, _ea);
                CamList.Add(vm);
            }
        }
    }
}
