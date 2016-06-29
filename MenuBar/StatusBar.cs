
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
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

        public StatusBarVM(ModeColors mode, EventAggregator ea) {
            _ea = ea;
            Status = "Ready";
            modeColors = mode;
            _ea.GetEvent<StatusUpdateEvent>().Subscribe(updateStatus);
        }
    }
}
