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

        Dictionary<string, CameraInfo> IP2CameraInfo;

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

        public OutputVM(Dictionary<string, CameraInfo> ip2CameraInfo) {
            isRunningProgram = false;
            outputCamera = null;
            IP2CameraInfo = ip2CameraInfo;
            Idle = Visibility.Visible;
            Active = Visibility.Hidden;
            _ea = Notification.Instance;
            _ea.GetEvent<CameraOutPutEvent>().Subscribe(outPutCameraFromCamlist);
            _ea.GetEvent<UpdateOutputCameraReceivedEvent>().Subscribe(outPutCameraFromRuntime);
            modeColors = ModeColors.Singleton(_ea);
        }

        private void outPutCameraFromRuntime(output_request_t res) {
            Idle = Visibility.Hidden;
            Active = Visibility.Visible;
            isRunningProgram = false;
            OutputCamera = null;
            if (res.ip_address == "") {
                var response = new stop_program_response_t() { status_code = status_codes_t.OK, response_message = "StopIndicatorFromUI" };
                _ea.GetEvent<ProgramStopResponseEvent>().Publish(response);
                return;
            }
            if (res.ip_address == "null") { return; }

            if (IP2CameraInfo.ContainsKey(res.ip_address)) {
                isRunningProgram = true;
                OutputCamera = IP2CameraInfo[res.ip_address];
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
