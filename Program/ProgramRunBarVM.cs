
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using NotificationCenter;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Util;

namespace Program {
    public class ProgramRunBarVM: BindableBase {

        public ObservableCollection<ProgramInfo> ProgramList { get; set; }
        protected readonly EventAggregator _ea;
        public ModeColors modeColors { get; set; }

        ProgramInfo program;
        public ProgramInfo Program {
            get { return program; }
            set { SetProperty(ref program, value); }
        }

        string RunningProgramPlaceHolder = "No running program";
        string runningProgramString;
        public string RunningProgramString {
            get { return runningProgramString; }
            set { SetProperty(ref runningProgramString, value); }
        }
        public ProgramRunBarVM(ObservableCollection<ProgramInfo> programList) {
            this.ProgramList = programList;
            _ea = Notification.Instance;
            modeColors = ModeColors.Singleton(_ea);
            RunningProgramString = RunningProgramPlaceHolder;

            _ea.GetEvent<ProgramRunEvent>().Subscribe(run);
        }

        private void run(int index) {
            Program = ProgramList[index];
            run(ProgramList[index]);
        }

        private void run(ProgramInfo program) {
            if (program != null) {
                RunningProgramString = program.ProgramName;
            }
        }

        ICommand runCommand;
        public ICommand RunCommand {
            get {
                if (runCommand == null) {
                    runCommand = new DelegateCommand<ProgramInfo>(
                        run);
                }
                return runCommand;
            }
        }
    }
}
