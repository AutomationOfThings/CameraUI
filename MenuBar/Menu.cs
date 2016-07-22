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
using System.Collections.ObjectModel;

namespace MenuBar
{
    public class MenuVM: BindableBase
    {
        public string Discover { get; set; }

        string mode;
        public string Mode {
            get { return mode; }
            set { SetProperty(ref mode, value); }
        }

        public ModeColors modeColors { get; set; }

        protected readonly EventAggregator _ea;
        List<CameraInfo> camList;
        public ObservableCollection<CameraNameWrapper> CameraNameList;

        public MenuVM(List<CameraInfo> camList, ObservableCollection<CameraNameWrapper> cameraNameList) {
            _ea = Notification.Instance;
            this.camList = camList;
            CameraNameList = cameraNameList;
            modeColors = ModeColors.Singleton(_ea);
            Mode = "Dark Mode";
            Discover = "Discover";
            _ea.GetEvent<ChangeModeShortCutEvent>().Subscribe(changeMode);
            _ea.GetEvent<ShowCameraInfoShortCutEvent>().Subscribe(showCameraList);
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
            _ea.GetEvent<RelaunchRuntimeEvent>().Publish("relaunchRuntime");
        }

        private void showCameraList(string obj) {
            CameraInfoVM vm = new CameraInfoVM(CameraNameList);
            CameraInfoForm form = new CameraInfoForm(vm);
            form.Show();
        }

        private void sendEventToPreview(string command) {
            _ea.GetEvent<MenuBarToPreviewEvent>().Publish(command);
        }

        private void saveSetting(string obj) {
            sendEventToPreview("SaveSetting");
        }

        private void saveSettingAsNew(string obj) {
            sendEventToPreview("SaveSettingAsNew");
        }

        private void clearPreview(string obj) {
            sendEventToPreview("ClearPreview");
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
                    discoverCommand = new DelegateCommand<string>(discover);
                }
                return discoverCommand;
            }
        }

        ICommand modeCommand;
        public ICommand ModeCommand {
            get {
                if (modeCommand == null) {
                    modeCommand = new DelegateCommand<string>(changeMode);
                }
                return modeCommand;
            }
        }

        ICommand checkCameraInfoCommand;
        public ICommand CheckCameraInfoCommand {
            get {
                if (checkCameraInfoCommand == null) {
                    checkCameraInfoCommand = new DelegateCommand<string>(showCameraList);
                }
                return checkCameraInfoCommand;
            }
        }

        ICommand saveSettingCommand;
        public ICommand SaveSettingCommand {
            get {
                if (saveSettingCommand == null) {
                    saveSettingCommand = new DelegateCommand<string>(saveSetting);
                }
                return saveSettingCommand;
            }
        }
        ICommand saveAsNewSettingCommand;
        public ICommand SaveAsNewSettingCommand {
            get {
                if (saveAsNewSettingCommand == null) {
                    saveAsNewSettingCommand = new DelegateCommand<string>(saveSettingAsNew);
                }
                return saveAsNewSettingCommand;
            }
        }
        ICommand clearPreviewCommand;
        public ICommand ClearPreviewCommand {
            get {
                if (clearPreviewCommand == null) {
                    clearPreviewCommand = new DelegateCommand<string>(clearPreview);
                }
                return clearPreviewCommand;
            }
        }

    }
}
