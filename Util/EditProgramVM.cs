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
        private List<List<CameraCommand>> programList;
        List<string> camStringList;
        List<string> presetStringList;

        public ModeColors modeColors { get; set; }

        int selectedIndex;
        public int SelectedIndex {
            get { return selectedIndex; }
            set { SetProperty(ref selectedIndex, value); }
        }

        public EditProgramVM(int index, List<List<CameraCommand>> programList, List<CameraInfo> camList, List<PresetParams> presetList, EventAggregator ea) {
            this._ea = ea;
            this.Index = index;
            this.programList = programList;
            this.CommandWrapperList = new ObservableCollection<CameraCommandEditWrapper>();
            this.camStringList = new List<string>();
            this.presetStringList = new List<string>();
            foreach (CameraInfo item in camList) {
                camStringList.Add(item.CameraID);
            }
            foreach (PresetParams item in presetList) {
                presetStringList.Add(item.presettingId);
            }
            foreach (CameraCommand cmd in programList[index]) {
                CameraCommandEditWrapper item = new CameraCommandEditWrapper(cmd, camStringList, presetStringList);
                CommandWrapperList.Add(item);
            }
        }

        public void saveProgram(ObservableCollection<CameraCommandEditWrapper> list)
        {
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
            }
            

            if (Index >= programList.Count) {
                List<CameraCommand> newList = new List<CameraCommand>();
                programList.Add(newList);
            }
            List<CameraCommand> plist = programList[Index];
            plist.Clear();
            foreach (CameraCommandEditWrapper item in list)
            {
                CameraCommand newItem = new CameraCommand(item);
                plist.Add(newItem);
            }
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
