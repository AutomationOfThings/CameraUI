using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.PubSubEvents;
using Util;
using System.Diagnostics;
using System.Xml;
using Microsoft.Practices.Prism.Commands;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using NotificationCenter;

namespace Presetting {
    /*
    public static class MyCommands
    {
        public static CompositeCommand MyCommand = new CompositeCommand();
    }
    */

    public class PresettingVM  {

        protected readonly EventAggregator _ea;

        public ModeColors modeColors { get; set; }

        List<PresetParams> camListForDisk;
        List<string> camIdList;

        ObservableCollection<PresetParamsExtend> camList;
        public ObservableCollection<PresetParamsExtend> CamList {
            get { return camList; }
            set { this.camList = value; }
        }

        Dictionary<PresetParamsExtend, PresetParams> camList2camListForDisk = new Dictionary<PresetParamsExtend, PresetParams>();

        int _selectedIndex = -1;
        public int SelectedIndex {
            get { return _selectedIndex; }
            set { _selectedIndex = value; }
        }

        public Color color { get; set; } = Color.FromRgb(60,0,0);

        public PresettingVM(List<PresetParams> presetList, List<CameraInfo> camList) {
            _ea = Notification.Instance;
            modeColors = ModeColors.Singleton(_ea);
            camListForDisk = presetList;
            this.camList = new ObservableCollection<PresetParamsExtend>();
            camIdList = new List<string>();
            foreach (CameraInfo item in camList) { camIdList.Add(item.CameraID); }
            foreach (PresetParams i in presetList) {
                PresetParamsExtend newItem = new PresetParamsExtend(i.presettingId, i.CamId, camIdList, i.pan, i.tilt, i.zoom);
                this.camList.Add(newItem);
                this.camList2camListForDisk[newItem] = i;
            }
            this._ea.GetEvent<SaveSettingEvent>().Subscribe(saveSetting);
        }

        private void saveSetting(PresetParams setting) {
            foreach (PresetParamsExtend item in CamList) {
                if (item.PresettingId == setting.presettingId) {
                    item.Pan = setting.pan;
                    item.Tilt = setting.tilt;
                    item.Zoom = setting.zoom;
                }
            }
        }

        private void saveToDiskListAtIndex(int index) {
            if (index < camList.Count && index >= 0) {
                PresetParamsExtend toSave = camList[index];
                foreach (PresetParams preset in camListForDisk) {
                    if (preset.presettingId == toSave.PresettingId) {
                        if (!camList2camListForDisk.ContainsKey(toSave) || 
                            (camList2camListForDisk.ContainsKey(toSave) && camList2camListForDisk[toSave] != preset)) {
                            MessageBox.Show("Cannot assian duplicate preset name", "Warning");
                            return;
                        }
                    }
                }
                if (toSave.CameraID == null || toSave.PresettingId == null) {
                    MessageBox.Show("Missing data", "Warning");
                    return;
                }
                if (camList2camListForDisk.ContainsKey(toSave)) {
                    PresetParams itemInCamListForDisk = camList2camListForDisk[toSave];
                    itemInCamListForDisk.update(toSave.PresettingId, toSave.CameraID, toSave.Pan, toSave.Tilt, toSave.Zoom);

                } else {
                    PresetParams savedItem = new PresetParams(toSave.PresettingId, toSave.CameraID, toSave.Pan, toSave.Tilt, toSave.Zoom);
                    camListForDisk.Add(savedItem);
                    camList2camListForDisk[toSave] = savedItem;
                }
                toSave.CanSave = false;
            }
        }

        public void save(object input) {
            int index = _selectedIndex;
            saveToDiskListAtIndex(index);
            saveCamListToDisk();
                
        }

        public void saveAll(object obj) {
            for (int i = 0; i < camList.Count; i++) {
                saveToDiskListAtIndex(i);
            }
            saveCamListToDisk();
        }

        public void saveCamListToDisk() {
            using (XmlWriter writer = XmlWriter.Create(Constant.PRESET_FILE)) {
                writer.WriteStartDocument();
                writer.WriteStartElement("Cameras");

                foreach (PresetParams cam in camListForDisk) {
                    writer.WriteStartElement("Preset");
                    writer.WriteAttributeString("Name", cam.presettingId);
                    writer.WriteElementString("CameraID", cam.CamId.ToString());
                    writer.WriteElementString("Pan", cam.pan.ToString());
                    writer.WriteElementString("Tilt", cam.tilt.ToString());
                    writer.WriteElementString("Zoom", cam.zoom.ToString());

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

        }

        public void delete(object input)  {
            int index = _selectedIndex;
            if (index < camList.Count && index >= 0) {
                PresetParamsExtend toDelete = camList[index];
                if (camList2camListForDisk.ContainsKey(toDelete)) {
                    PresetParams itemInCamListForDisk = camList2camListForDisk[toDelete];
                    camListForDisk.Remove(itemInCamListForDisk);
                }
                camList.Remove(toDelete);
            }
            saveCamListToDisk();
            _ea.GetEvent<PresettingDeleteEvent>().Publish(index);
        }

        public void add(object obj) {
            PresetParamsExtend newItem = new PresetParamsExtend(camIdList);
            this.camList.Add(newItem);
        }

        public void set(object input) {
            int index = _selectedIndex;
            if (index < camList.Count && index >= 0) {
                // Debug.WriteLine("set preset: " + camList[index].toString());
                PresetParamsExtend toSet = camList[index];
                PresetParams preset = new PresetParams(toSet.PresettingId, toSet.CameraID, toSet.Pan, toSet.Tilt, toSet.Zoom);
                _ea.GetEvent<SetPresetEvent>().Publish(preset);
            }
        }


        // ICommands:

        ICommand saveCommand;
        public ICommand SaveCommand {
            get {
                if (saveCommand == null) {
                    saveCommand = new DelegateCommand<Object>(save);
                }
                return saveCommand;
            }
        }

        ICommand deleteCommand;
        public ICommand DeleteCommand {
            get {
                if (deleteCommand == null) {
                    deleteCommand = new DelegateCommand<object>(delete);
                }
                return deleteCommand;
            }
        }

        ICommand setCommand;
        public ICommand SetCommand {
            get {
                if (setCommand == null) {
                    setCommand = new DelegateCommand<object>(set);
                }
                return setCommand;
            }
        }

        ICommand addCommand;
        public ICommand AddCommand {
            get {
                if (addCommand == null) {
                    addCommand = new DelegateCommand<object>(add);
                }
                return addCommand;
            }
        }

        ICommand saveAllCommand;
        public ICommand SaveAllCommand {
            get {
                if (saveAllCommand == null) {
                    saveAllCommand = new DelegateCommand<object>(saveAll);
                }
                return saveAllCommand;
            }
        }

    }
}
