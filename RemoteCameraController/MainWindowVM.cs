
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using System.Windows.Input;
using Util;

namespace RemoteCameraController {
    public class MainWindowVM : BindableBase {

        protected EventAggregator _ea;

        public ModeColors modeColors { get; set; }

        public MainWindowVM (EventAggregator ea) {
            this._ea = ea;
        }

        private void changeModeShortCut(ModeColors modeColors) {
            _ea.GetEvent<ChangeModeShortCutEvent>().Publish(modeColors);
        }

        private void discoverShortCut(string discovery) {
            _ea.GetEvent<CameraDiscoverShortCutEvent>().Publish("discover");
        }

        ICommand modeCommand;
        public ICommand ModeCommand {
            get {
                if (modeCommand == null) {
                    modeCommand = new DelegateCommand<ModeColors>(
                        changeModeShortCut);
                }
                return modeCommand;
            }
        }

        ICommand discoverCommand;
        public ICommand DiscoverCommand {
            get {
                if (discoverCommand == null) {
                    discoverCommand = new DelegateCommand<string>(
                        discoverShortCut);
                }
                return discoverCommand;
            }
        }

    }
}
