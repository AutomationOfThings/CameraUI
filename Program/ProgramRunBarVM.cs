
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using NotificationCenter;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using Util;

namespace Program {
    public class ProgramRunBarVM: BindableBase {

        public ObservableCollection<ProgramInfo> ProgramList { get; set; }
        Dictionary<string, PresetParams> presetName2Preset;
        Dictionary<string, string> cameraName2IP;
        protected readonly EventAggregator _ea;
        public ModeColors modeColors { get; set; }

        ProgramInfo runningProgram;

        int selectedIndex;
        public int SelectedIndex {
            get { return selectedIndex; }
            set { SetProperty(ref selectedIndex, value); }
        }

        string RunningProgramPlaceHolder = "No running program";
        string runningProgramString;
        public string RunningProgramString {
            get { return runningProgramString; }
            set { SetProperty(ref runningProgramString, value); }
        }
        public ProgramRunBarVM(ObservableCollection<ProgramInfo> programList, Dictionary<string, PresetParams> presetName2Preset, Dictionary<string, string> cameraName2IP) {
            ProgramList = programList;
            this.presetName2Preset = presetName2Preset;
            this.cameraName2IP = cameraName2IP;
            _ea = Notification.Instance;
            modeColors = ModeColors.Singleton(_ea);
            RunningProgramString = RunningProgramPlaceHolder;
            SelectedIndex = -1;
            _ea.GetEvent<ProgramRunEvent>().Subscribe(run);
        }

        private void run(int? index) {
            if (SelectedIndex < ProgramList.Count && SelectedIndex >= 0) {
                run(ProgramList[SelectedIndex]);
            }
        }


        private void stop(object obj) {
            if (runningProgram != null) {
                stop(runningProgram);
            }
        }

        private void run(ProgramInfo program) {
            if (program != null) {
                string pgm = formatProgramString(program);
                if (pgm != null) {
                    CameraConnnector.requestProgramRun(formatProgramString(program));
                    runningProgram = ProgramList[SelectedIndex];
                    RunningProgramString = program.ProgramName;
                }
                
            }
        }

        private void stop(ProgramInfo program) {
            if (program != null) {
                string pgm = formatProgramString(program);
                if (pgm != null) {
                    CameraConnnector.requestProgramStop(formatProgramString(program));
                }
            }
        }

        private void runProgramResponse() {
            stop();
        }

        private void stopProgramResponse() {
            stop();
        }

        private void stop() {
            runningProgram = null;
            RunningProgramString = RunningProgramPlaceHolder;
        }

        private string formatProgramString(ProgramInfo program) {
            string output = "";
            int i = 1;
            foreach (CameraCommand item in program.commandList) {
                switch (item.Command) {
                    case Cmd.WAIT:
                        output += ("WAIT=" + item.Parameter);
                        break;
                    case Cmd.OUTPUT:
                        string ip1 = "";
                        if (cameraName2IP.ContainsKey(item.Parameter)) {
                            ip1 = cameraName2IP[item.Parameter];
                        } else {
                            string err = string.Format( "Line {0}: Output camera \"{1}\" is not associated with a valid IP.", i, item.Parameter);
                            MessageBox.Show(err, "Error");
                            return null;
                        }
                        output += ("OUTPUT=" + ip1);
                        break;
                    case Cmd.PRESET:
                        PresetParams preset = presetName2Preset[item.Parameter];
                        string ip2 = "";
                        if (cameraName2IP.ContainsKey(preset.CameraName)) {
                            ip2 = cameraName2IP[preset.CameraName];
                        } else {
                            string err = string.Format("Line {0}: Output camera \"{1}\" in Preset \"{2}\" is not associated with a valid IP.", i, preset.CameraName, preset.presettingId);
                            MessageBox.Show(err, "Error");
                            return null;
                        }
                        string ptz = preset.pan + "," + preset.tilt + "," + preset.zoom;
                        output += ("PRESET=" + ip2 + "," + ptz);
                        break;
                    default:
                        break;
                }
                output += "\n";
                i ++;
            }
            return output;
        }

        ICommand runCommand;
        public ICommand RunCommand {
            get {
                if (runCommand == null) { runCommand = new DelegateCommand<int?>(run); }
                return runCommand;
            }
        }

        ICommand stopCommand;
        public ICommand StopCommand {
            get {
                if (stopCommand == null) { stopCommand = new DelegateCommand<object>(stop); }
                return stopCommand;
            }
        }
    }
}
