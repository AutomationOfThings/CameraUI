using Microsoft.Practices.Prism.Mvvm;
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
    public class CameraListVM: BindableBase {

        EventAggregator _ea;

        List<CameraInfo> camInfoList;
        ObservableCollection<CameraNameWrapper> cameraNameList;
        ObservableCollection<CameraVM> camList;
        public ObservableCollection<CameraVM> CamList {
            get { return camList; }
            set { SetProperty(ref camList,value); }
        }

        public ModeColors modeColors { get; set; }

        public CameraListVM(List<CameraInfo> camInfoList, ObservableCollection<CameraNameWrapper> CameraNameList) {
            _ea = Notification.Instance;
            _ea.GetEvent<CameraDiscoveredEvent>().Subscribe(updateCamList, ThreadOption.UIThread);
            cameraNameList = CameraNameList;
            modeColors = ModeColors.Singleton(_ea);
            this.camInfoList = camInfoList;
            CamList = new ObservableCollection<CameraVM>();
            updateCamList(camInfoList);
        }

        public void updateCamList(List<CameraInfo> list) {
            foreach (CameraVM item in CamList) {
                item.mjpegDecoder.StopStream();
                item.unSbscribe();
                item.mjpegDecoder = null;
            }
            CamList.Clear();
            for (int i = 0; i < camInfoList.Count; i++) {
                CameraVM vm = new CameraVM(camInfoList[i], cameraNameList, modeColors, _ea);
                if (camInfoList[i].IP != "" && camInfoList[i].IP != null 
                    && camInfoList[i].UserName!=null && camInfoList[i].Password != null
                    && camInfoList[i].UserName != "" && camInfoList[i].Password != "") {
                    vm.connect();
                }
                CamList.Add(vm);
            }
        }
    }
}
