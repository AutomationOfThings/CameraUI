
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using NotificationCenter;
using System.Threading.Tasks;
using Util;

namespace MenuBar {
    public class StatusBarVM: BindableBase {

        protected readonly EventAggregator _ea;
        public ModeColors modeColors { get; set; }

        string status;
        public string Status {
            get {return status;}
            set { SetProperty(ref status, value);}
        }

        protected void updateStatus(string sts) {
            Status = sts;
            Task.Delay(5000).ContinueWith(_ => {
                Status = "Ready";
            });
        }

        public StatusBarVM() {
            _ea = Notification.Instance;
            Status = "Ready";
            modeColors = ModeColors.Singleton(_ea);
            _ea.GetEvent<StatusUpdateEvent>().Subscribe(updateStatus);
        }
    }
}
