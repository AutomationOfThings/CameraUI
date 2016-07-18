using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using Util;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Prism.Mvvm;
using NotificationCenter;
using System.IO;
using System.Windows.Forms;

namespace MenuBar
{
    public class MenuVM: BindableBase
    {
        public string Discover { get; set; }
        private Process runtime;
        string mode;
        public string Mode {
            get { return mode; }
            set { SetProperty(ref mode, value); }
        }

        public ModeColors modeColors { get; set; }

        protected readonly EventAggregator _ea;
        List<CameraInfo> camList;

        public MenuVM(List<CameraInfo> camList, Process runtime) {
            _ea = Notification.Instance;
            this.camList = camList;
            modeColors = ModeColors.Singleton(_ea);
            Mode = "Dark Mode";
            Discover = "Discover";
            this.runtime = runtime;
            _ea.GetEvent<ChangeModeShortCutEvent>().Subscribe(changeMode);
            _ea.GetEvent<RelaunchRuntimeShortCutEvent>().Subscribe(relaunchRuntime);
        }


        public void discover(string input) {
            _ea.GetEvent<CameraDiscoverEvent>().Publish(input);
        }

        public void changeMode(string mode) {
            _ea.GetEvent<ChangeModeEvent>().Publish(mode);
            if (Mode == "Dark Mode") {
                Mode = "Light Mode";
            } else if (Mode == "Light Mode") {
                Mode = "Dark Mode";
            }

        }

        public void changeMode(ModeColors modeColor) {
            changeMode(Mode);
        }

        private void relaunchRuntime(string obj) {
            try {
                runtime.Kill();
                runtime.Start();

            } catch (Exception ex) {
                Console.WriteLine("An error occurred in starting runtime!!!: " + ex.Message);
                MessageBox.Show("Meet an error in launching the runtime.", "Attention", MessageBoxButtons.OK);
                return;
            }
        }

        // ICommands:

        ICommand relaunchRuntimeCommand;
        public ICommand RelaunchRuntimeCommand {
            get {
                return relaunchRuntimeCommand ?? new DelegateCommand<string>(relaunchRuntime);
            }
        }

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
