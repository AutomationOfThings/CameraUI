
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using NotificationCenter;
using ptz_camera;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
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

        Visibility runningProgramRedVisible;
        public Visibility RunningProgramRedVisible {
            get { return runningProgramRedVisible; }
            set { SetProperty(ref runningProgramRedVisible, value); }
        }

        int selectedIndex;
        public int SelectedIndex {
            get { return selectedIndex; }
            set { SetProperty(ref selectedIndex, value); }
        }

        string runningProgramString;
        string runningProgramStatusString;
        string RunningProgramPlaceHolder = "No running program";

        string programString;
        public string ProgramString {
            get { return programString; }
            set { SetProperty(ref programString, value); }
        }
        public ProgramRunBarVM(ObservableCollection<ProgramInfo> programList, Dictionary<string, PresetParams> presetName2Preset, Dictionary<string, string> cameraName2IP) {
            ProgramList = programList;
            this.presetName2Preset = presetName2Preset;
            this.cameraName2IP = cameraName2IP;
            _ea = Notification.Instance;
            modeColors = ModeColors.Singleton(_ea);
            runningProgramString = RunningProgramPlaceHolder;
            runningProgramStatusString = "";
            SelectedIndex = -1;
            RunningProgramRedVisible = Visibility.Hidden;
            _ea.GetEvent<ProgramRunEvent>().Subscribe(run);
            _ea.GetEvent<ProgramStartResponseEvent>().Subscribe(startProgramResponse);
            _ea.GetEvent<ProgramStopResponseEvent>().Subscribe(stopProgramResponse);
            _ea.GetEvent<ProgramEndMessageReceivedEvent>().Subscribe(endProgram);
            _ea.GetEvent<ProgramStatusMessageReceivedEvent>().Subscribe(updateProgramRunningStatus);
        }

        private void updateProgramString() {
            ProgramString = runningProgramString + runningProgramStatusString;
        }

        private void run(int? index) {
            if (runningProgram != null) {
                MessageBox.Show("A program is currently running.", "Error");
            } else {
                if (SelectedIndex < ProgramList.Count && SelectedIndex >= 0) {
                    run(ProgramList[SelectedIndex]);
                }
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
                    runningProgramString = program.ProgramName;
                    updateProgramString();
                    RunningProgramRedVisible = Visibility.Visible;
                    _ea.GetEvent<PreviewPauseEvent>().Publish(true);
                }
            }
        }

        private void stop(ProgramInfo program) {
            if (program != null) {
                CameraConnnector.requestProgramStop();
            }
        }

        private void startProgramResponse(start_program_response_t res) {
            if (res.status_code == status_codes_t.ERR) {
                stop();
                MessageBox.Show("Message: " + res.response_message, "Error");
            }  
        }

        private void stopProgramResponse(stop_program_response_t res) {
            if (res.status_code == status_codes_t.OK) {
                stop();
            }
        }

        private void endProgram(end_program_message_t res) {
            stop();
        }

        private void stop() {
            runningProgram = null;
            runningProgramString = RunningProgramPlaceHolder;
            runningProgramStatusString = "";
            updateProgramString();
            RunningProgramRedVisible = Visibility.Hidden;
            _ea.GetEvent<PreviewResumeEvent>().Publish(true);
        }


        private void updateProgramRunningStatus(program_status_message_t res) {
            if (runningProgram != null) {
                int line = res.line_num - 1;
                if (line >= 0 && line < runningProgram.commandList.Count) {
                    CameraCommand item = runningProgram.commandList[line];
                    string cmd = item.Command.ToString();
                    string param = item.Parameter.ToString();
                    runningProgramStatusString = "  Line " + line + ": " + cmd + " " + param;
                } else {
                    runningProgramStatusString = "";
                }
                updateProgramString();
            }
            
        }

        private string formatProgramString(ProgramInfo program) {
            string output = "";
            int i = 1;
            foreach (CameraCommand item in program.commandList) {
                switch (item.Command) {
                    case Cmd.WAIT:
                        output += ("WAIT=" + int.Parse(item.Parameter) * 1000);
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
