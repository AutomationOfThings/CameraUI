
using System.Collections.Generic;
using Microsoft.Practices.Prism.PubSubEvents;
using Util;
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
            _ea.GetEvent<ProgramEndMessageReceivedEvent>().Subscribe(endOutput);
            modeColors = ModeColors.Singleton(_ea);
        }

        private void outPutCameraFromRuntime(output_request_t res) {
            Idle = Visibility.Hidden;
            Active = Visibility.Visible;

            if (IP2CameraInfo.ContainsKey(res.ip_address)) {
                isRunningProgram = true;
                OutputCamera = IP2CameraInfo[res.ip_address];
            }

        }

        private void endOutput(end_program_message_t res) {
            isRunningProgram = false;
            OutputCamera = null;
            Idle = Visibility.Visible;
            Active = Visibility.Hidden;
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
