using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace Util {
    public class EditProgramVM: BindableBase {

        public int Index { get; set; }
        protected readonly EventAggregator _ea;
        public ObservableCollection<CameraCommandEditWrapper> CommandWrapperList { get; set; }
        private ObservableCollection<ProgramInfo> programList;
        List<string> camStringList;
        List<string> presetStringList;

        string programName;
        public string ProgramName {
            get { return programName; }
            set { SetProperty(ref programName, value); }
        }

        public ModeColors modeColors { get; set; }

        int selectedIndex;
        public int SelectedIndex {
            get { return selectedIndex; }
            set { SetProperty(ref selectedIndex, value); }
        }

        public EditProgramVM(int index, ObservableCollection<ProgramInfo> programList, List<CameraInfo> camList, List<PresetParams> presetList, EventAggregator ea) {
            _ea = ea;
            Index = index;
            this.programList = programList;
            
            CommandWrapperList = new ObservableCollection<CameraCommandEditWrapper>();
            camStringList = new List<string>();
            presetStringList = new List<string>();
            foreach (CameraInfo item in camList) {
                camStringList.Add(item.CameraID);
            }
            foreach (PresetParams item in presetList) {
                presetStringList.Add(item.presettingId);
            }
            if (Index < programList.Count) {
                ProgramName = programList[Index].ProgramName;
                foreach (CameraCommand cmd in programList[Index].commandList) {
                    CameraCommandEditWrapper item = new CameraCommandEditWrapper(cmd, camStringList, presetStringList);
                    CommandWrapperList.Add(item);
                }
            } else {
                ProgramName = "";
            }
            
        }

        public void saveProgram(ObservableCollection<CameraCommandEditWrapper> list)
        {
            if (ProgramName == "") {
                MessageBox.Show("Invalid Program Name!");
                return;
            }
            if (list.Count <= 0) {
                MessageBox.Show("Invalid Program!");
                return;
            } else {
                foreach (CameraCommandEditWrapper item in list) {
                    if (item.Parameter == null) {
                        MessageBox.Show("Invalid Parameter!");
                        return;
                    }
                }
                foreach (ProgramInfo item in programList) {
                    if (item.ProgramName == ProgramName && programList.IndexOf(item)!= Index) {
                        MessageBox.Show("Duplicate Program Name!");
                        return;
                    }
                }
            }
            
            if (Index >= programList.Count) {
                List<CameraCommand> newCmdList = new List<CameraCommand>();
                ProgramInfo newProgramInfo = new ProgramInfo() { ProgramName = ProgramName, commandList = newCmdList };
                programList.Add(newProgramInfo);
            }
            ProgramInfo plist = programList[Index];
            plist.commandList.Clear();
            foreach (CameraCommandEditWrapper item in list)
            {
                CameraCommand newItem = new CameraCommand(item);
                plist.commandList.Add(newItem);
            }
            plist.ProgramName = ProgramName;
            _ea.GetEvent<ProgramSaveEvent>().Publish(Index);
        }

        public void addStep(ObservableCollection<CameraCommandEditWrapper> list) {
            CameraCommandEditWrapper item = new CameraCommandEditWrapper(camStringList, presetStringList);
            CommandWrapperList.Add(item);
        }

        public void deleteStep(ObservableCollection<CameraCommandEditWrapper> list) {
            if (selectedIndex >=0 && selectedIndex < list.Count)
                CommandWrapperList.RemoveAt(selectedIndex);
        }


        ICommand saveCommand;
        public ICommand SaveCommand {
            get {
                if (saveCommand == null)
                {
                    saveCommand = new DelegateCommand<ObservableCollection<CameraCommandEditWrapper>>(saveProgram);
                }
                return saveCommand;
            }
        }

        ICommand addCommand;
        public ICommand AddCommand {
            get {
                if (addCommand == null) {
                    addCommand = new DelegateCommand<ObservableCollection<CameraCommandEditWrapper>>(addStep);
                }
                return addCommand;
            }
        }

        ICommand deleteCommand;
        public ICommand DeleteCommand {
            get {
                if (deleteCommand == null) {
                    deleteCommand = new DelegateCommand<ObservableCollection<CameraCommandEditWrapper>>(deleteStep);
                }
                return deleteCommand;
            }
        }

        
    }
}
