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
        ObservableCollection<ProgramInfo> programList;

        List<CameraInfo> camList;
        ObservableCollection<CameraNameWrapper> cameraNameList;
        List<PresetParams> presetList;

        public ObservableCollection<BindingWapper> programStringList { get; set; }

        short _selectedIndex = -1;
        public short SelectedIndex {
            get { return _selectedIndex; }
            set { _selectedIndex = value; }
        }

        public ProgramVM(ObservableCollection<ProgramInfo> programList, List<CameraInfo> camInfoList, ObservableCollection<CameraNameWrapper> cameraNameList,List<PresetParams> presettingList) {
            _ea = Notification.Instance;
            this.programList = programList;
            this.cameraNameList = cameraNameList;
            camList = camInfoList;
            presetList = presettingList;
            modeColors = ModeColors.Singleton(_ea);

            List<CameraCommand> list = new List<CameraCommand>();
            save(-1);
            _ea.GetEvent<ProgramSaveEvent>().Subscribe(save);
        }

        public void edit(object input) {
            int index = _selectedIndex;
            EditProgramVM vm;
            /*
            if (index >= 0 && index < programList.Count) {
                vm = new EditProgramVM(index, programNameList, programList, camList, presetList, _ea);
            } else {
                List<CameraCommand> list = new List<CameraCommand>();
                programList.Add(list);
                string newProgramName = "";
                programNameList.Add(newProgramName);
                vm = new EditProgramVM(index, programNameList, programList, camList, presetList, _ea);
            }
            */
            vm = new EditProgramVM(index, programList, cameraNameList, camList, presetList, _ea);
            vm.modeColors = modeColors;
            EditProgramForm form = new EditProgramForm(vm);
            form.ShowDialog();
        }

        public void save(int index) {
            if (index == -1) {
                programStringList = new ObservableCollection<BindingWapper>();
                foreach (ProgramInfo item in programList) {
                    string name = item.ProgramName;
                    string itemString = "";
                    foreach (CameraCommand cam in item.commandList) {
                        itemString += (cam.toString() + "; ");
                    }
                    if (itemString.Length >= 2)
                        itemString = itemString.Substring(0, itemString.Length - 2);
                    BindingWapper ele = new BindingWapper { ProgramName=name, Content = itemString};
                    programStringList.Add(ele);
                }
            } else {
                string itemString = "";
                foreach (CameraCommand cmd in programList[index].commandList) {
                    itemString += (cmd.toString() + "; ");
                }
                if (itemString.Length >= 2)
                    itemString = itemString.Substring(0, itemString.Length - 2);
                if (index >= programStringList.Count) {
                    BindingWapper item = new BindingWapper();
                    programStringList.Add(item);
                }
                programStringList[index].Content = itemString;
                programStringList[index].ProgramName = programList[index].ProgramName;
                saveProgramListToDisk();
                return;
            }

        }

        public void delete(Object input) {
            int index = _selectedIndex;
            if (index < programStringList.Count && index >= 0) {
                programList.RemoveAt(index);
                programStringList.RemoveAt(index);
            }
            saveProgramListToDisk();
        }

        public void run(Object input) {
            int index = _selectedIndex;
            if (index < programStringList.Count && index >= 0) {
                _ea.GetEvent<ProgramRunEvent>().Publish(index);
            } 
        }

        private void saveProgramListToDisk() {
            using (XmlWriter writer = XmlWriter.Create(Constant.PROGRAM_FILE))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Programs");

                foreach (ProgramInfo item in programList) {
                    writer.WriteStartElement("Program");
                    writer.WriteAttributeString("Name", item.ProgramName);
                    foreach (CameraCommand cmd in item.commandList) {
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
                    applyCommand = new DelegateCommand<Object>(run);
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

        string programName;
        public string ProgramName {
            get { return programName; }
            set { SetProperty(ref programName, value); }
        }

        string _content;
        public string Content {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }
    }

   

}
