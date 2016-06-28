using Microsoft.Practices.Prism.PubSubEvents;
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

        public ObservableCollection<CameraVM> CamList { get; set; }

        public ModeColors modeColors { get; set; }

        public CameraListVM(List<CameraInfo> camInfoList, ModeColors modeColors, EventAggregator ea) {
            _ea = ea;
            this.modeColors = modeColors;
            CamList = new ObservableCollection<CameraVM>();

            for (int i = 0; i < camInfoList.Count; i++) {
                CameraVM vm = new CameraVM(camInfoList[i], modeColors, _ea);
                CamList.Add(vm);
            }
        }
    }
}
