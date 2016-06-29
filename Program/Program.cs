using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.PubSubEvents;
using Util;
using System.Xml;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using NotificationCenter;

namespace Program {
    public class ProgramVM {

        public ModeColors modeColors { get; set; }

        protected readonly EventAggregator _ea;
        List<List<CameraCommand>> programList;
        public List<List<CameraCommand>> ProgramList { get; set; }

        List<CameraInfo> camList;
        List<PresetParams> presetList;

        public ObservableCollection<BindingWapper> programStringList { get; set; }

        Int16 _selectedIndex = -1;
        public Int16 SelectedIndex {
            get { return _selectedIndex; }
            set { _selectedIndex = value; }
        }

        public ProgramVM(List<List<CameraCommand>> programList, List<CameraInfo> camInfoList, List<PresetParams> presettingList) {
            this._ea = Notification.Instance;
            this.programList = programList;
            this.camList = camInfoList;
            this.presetList = presettingList;
            modeColors = ModeColors.Singleton(_ea);

            List<CameraCommand> list = new List<CameraCommand>();
            save(-1);
            this._ea.GetEvent<ProgramSaveEvent>()
                .Subscribe(save);
            this._ea.GetEvent<ProgramCancelEvent>()
                .Subscribe(cancelEdit);
        }

        public void edit(Object input) {
            int index = _selectedIndex;
            EditProgramVM vm;
            if (index >= 0 && index < programList.Count) {
                vm = new EditProgramVM(index, programList, camList, presetList, _ea);
            } else {
                List<CameraCommand> list = new List<CameraCommand>();
                programList.Add(list);
                vm = new EditProgramVM(index, programList, camList, presetList, _ea);
            }
            vm.modeColors = modeColors;
            EditProgramForm form = new EditProgramForm(vm);
            form.ShowDialog();
        }

        public void save(int index) {
            if (index == -1) {
                this.programStringList = new ObservableCollection<BindingWapper>();
                foreach (List<CameraCommand> item in programList) {
                    string itemString = "";
                    foreach (CameraCommand cam in item) {
                        itemString += (cam.toString() + "; ");
                    }
                    if (itemString.Length >= 2)
                        itemString = itemString.Substring(0, itemString.Length - 2);
                    BindingWapper ele = new BindingWapper {Content = itemString};
                    programStringList.Add(ele);
                }
            } else {
                string itemString = "";
                foreach (CameraCommand cmd in programList[index]) {
                    itemString += (cmd.toString() + "; ");
                }
                if (itemString.Length >= 2)
                    itemString = itemString.Substring(0, itemString.Length - 2);
                if (index >= programStringList.Count) {
                    BindingWapper item = new BindingWapper();
                    programStringList.Add(item);
                }
                this.programStringList[index].Content = itemString;
                saveProgramListToDisk();
                return;
            }

        }

        public void cancelEdit(int index) {
            this.programStringList.RemoveAt(index);
            this.programList.RemoveAt(index);
            saveProgramListToDisk();
        }

        public void delete(Object input) {
            int index = _selectedIndex;
            if (index < programStringList.Count && index >= 0) {
                programList.RemoveAt(index);
                programStringList.RemoveAt(index);
            }
            saveProgramListToDisk();
        }

        public void apply(Object input) {
            int index = _selectedIndex;
            if (index < programStringList.Count && index >= 0) {
            } 
        }

        private void saveProgramListToDisk() {
            using (XmlWriter writer = XmlWriter.Create(Constant.PROGRAM_FILE))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Programs");

                foreach (List<CameraCommand> item in programList) {
                    writer.WriteStartElement("Program");

                    foreach (CameraCommand cmd in item) {
                        writer.WriteStartElement("Step");

                        writer.WriteElementString("Command", cmd.Command.ToString());
                        writer.WriteElementString("Parameter", cmd.Parameter.ToString());

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        // ICommands:

        ICommand deleteCommand;
        public ICommand DeleteCommand {
            get {
                if (deleteCommand == null) {
                    deleteCommand = new DelegateCommand<Object>(
                        delete);
                }
                return deleteCommand;
            }
        }

        ICommand applyCommand;
        public ICommand ApplyCommand {
            get {
                if (applyCommand == null) {
                    applyCommand = new DelegateCommand<Object>(
                        apply);
                }
                return applyCommand;
            }
        }
        
        ICommand editCommand;
        public ICommand EditCommand {
            get {
                if (editCommand == null) {
                    editCommand = new DelegateCommand<Object>(
                        edit);
                }
                return editCommand;
            }
        }


    }

    public class BindingWapper: BindableBase {

        string _content;
        public string Content {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }
    }

   

}
