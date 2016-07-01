using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using Util;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Prism.Mvvm;
using NotificationCenter;

namespace MenuBar
{
    public class MenuVM: BindableBase
    {
        public string Discover { get; set; } = "Discover";

        string mode;
        public string Mode {
            get { return mode; }
            set { SetProperty(ref mode, value); }
        }

        public ModeColors modeColors { get; set; }

        protected readonly EventAggregator _ea;
        List<CameraInfo> camList;

        public MenuVM(List<CameraInfo> camList) {
            _ea = Notification.Instance;
            this.camList = camList;
            modeColors = ModeColors.Singleton(_ea);
            Mode = "Dark Mode";

            _ea.GetEvent<ChangeModeShortCutEvent>().Subscribe(changeMode);
        }


        public void discover(string input) {
            _ea.GetEvent<CameraDiscoverEvent>().Publish(input);
        }

        public void changeMode(string mode) {
            _ea.GetEvent<ChangeModeEvent>().Publish(mode);
            if (this.Mode == "Dark Mode") {
                this.Mode = "Light Mode";
            } else if (this.Mode == "Light Mode") {
                this.Mode = "Dark Mode";
            }

        }

        public void changeMode(ModeColors modeColor) {
            changeMode(Mode);
        }

        // ICommands:
        ICommand discoverCommand;
        public ICommand DiscoverCommand {
            get {
                if (discoverCommand == null) {
                    discoverCommand = new DelegateCommand<string>(
                        discover);
                }
                return discoverCommand;
            }
        }

        ICommand modeCommand;
        public ICommand ModeCommand {
            get {
                if (modeCommand == null) {
                    modeCommand = new DelegateCommand<string>(
                        changeMode);
                }
                return modeCommand;
            }
        }


    }
}
