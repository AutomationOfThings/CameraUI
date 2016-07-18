using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;
using Util;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Microsoft.Practices.Prism.Mvvm;
using NotificationCenter;
using ptz_camera;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace Output {
    public class OutputVM: BindableBase {

        bool isRunningProgram;

        public ModeColors modeColors { get; set; }
        List<CameraInfo> camInfoList;

        Visibility idle;
        public Visibility Idle {
            get { return idle; }
            set { SetProperty(ref idle, value); }
        }

        Visibility active;
        public Visibility Active {
            get { return active; }
            set { SetProperty(ref active, value); }
        }

        protected readonly EventAggregator _ea;

        private CameraInfo outputCamera;
        public CameraInfo OutputCamera {
            get { return outputCamera; }
            set { SetProperty(ref outputCamera, value); }
        }

        public OutputVM(List<CameraInfo> camInfoList) {
            isRunningProgram = false;
            outputCamera = null;
            this.camInfoList = camInfoList;
            Idle = Visibility.Visible;
            Active = Visibility.Hidden;
            _ea = Notification.Instance;
            _ea.GetEvent<CameraOutPutEvent>().Subscribe(outPutCameraFromCamlist);
            _ea.GetEvent<UpdateOutputCameraReceivedEvent>().Subscribe(outPutCameraFromRuntime);
            modeColors = ModeColors.Singleton(_ea);
        }

        private void outPutCameraFromRuntime(end_session_response_t res) {
            Idle = Visibility.Hidden;
            Active = Visibility.Visible;
            if (res.ip_address == "") {
                isRunningProgram = false;
                OutputCamera = null;
                Idle = Visibility.Visible;
                Active = Visibility.Hidden;
            }
            foreach (CameraInfo item in camInfoList) {
                if (item.IP == res.ip_address) {
                    isRunningProgram = true;
                    OutputCamera = item;
                }
            }
        }

        private void outPutCameraFromCamlist(CameraInfo cam) {
            if (!isRunningProgram) {
                OutputCamera = cam;
                Idle = Visibility.Hidden;
                Active = Visibility.Visible;
            } 
        }

        private void clear(CameraInfo cam) {
            _ea.GetEvent<ClearCameraOutputEvent>().Publish(OutputCamera);
            OutputCamera = null;
            Idle = Visibility.Visible;
            Active = Visibility.Hidden;
        }

        ICommand clearCommand;
        public ICommand ClearCommand {
            get {
                if (clearCommand == null) { clearCommand = new DelegateCommand<CameraInfo>(clear); }
                return clearCommand;
            }
        }

    }

}
