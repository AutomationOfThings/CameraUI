using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util {
    public class CameraInfoVM {

        public ObservableCollection<CameraNameWrapper> CameraNameList { get; set; }

        public ModeColors modeColors { get; set; }

        protected readonly EventAggregator _ea;

        public CameraInfoVM(ObservableCollection<CameraNameWrapper> nameList) {
            CameraNameList = nameList;
            _ea = NotificationCenter.Notification.Instance;
            modeColors = ModeColors.Singleton(_ea);
        }
    }
}
