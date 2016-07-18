
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Util;

namespace Camera {
    public class CameraNamingVM: BindableBase {

        public ObservableCollection<CameraNameWrapper> CameraNameList { get; set; }

        public ModeColors modeColors { get; set; }

        protected readonly EventAggregator _ea;

        private CameraInfo camera;

        int selectedIndex;
        public int SelectedIndex {
            get { return selectedIndex; }
            set { SetProperty(ref selectedIndex, value); }
        }

        string cameraName;
        public string CameraName {
            get { return cameraName; }
            set { SetProperty(ref cameraName, value); }
        }

        public CameraNamingVM(ObservableCollection<CameraNameWrapper> nameList, CameraInfo cam) {
            CameraNameList = nameList;
            CameraName = cam.CameraName;
            camera = cam;
            SelectedIndex = -1;
            _ea = NotificationCenter.Notification.Instance;
            modeColors = ModeColors.Singleton(_ea);
        }

        private void add(ObservableCollection<CameraNameWrapper> list) {
            CameraNameWrapper item = new CameraNameWrapper(null);
            item.NotAssociated = true;
            CameraNameList.Add(item);
        }

        private void delete(int? index) {
            if (index < CameraNameList.Count) {
                CameraNameList.RemoveAt(SelectedIndex);
            } 
        }

        private void clear(string name) {

            if (CameraName != null) {
                foreach (CameraNameWrapper item in CameraNameList) {
                    if (item.CameraName == CameraName) {
                        item.AssociatedIP = "";
                        item.NotAssociated = true;
                        item.username = "";
                        item.password = "";
                        break;
                    }
                }
            }
            CameraName = null;
            camera.CameraName = null;
        }

        private void set(int? index) {
            if (index < CameraNameList.Count && !string.IsNullOrEmpty(CameraNameList[SelectedIndex].CameraName)) {
                CameraNameWrapper cam = CameraNameList[SelectedIndex];
                foreach (CameraNameWrapper item in CameraNameList) {
                    if (item.CameraName == CameraName) {
                        item.AssociatedIP = "";
                        item.NotAssociated = true;
                        item.username = "";
                        item.password = "";
                        break;
                    }
                }
                
                cam.NotAssociated = false;
                cam.AssociatedIP = camera.IP;
                cam.username = camera.UserName;
                cam.password = camera.Password;
                CameraName = cam.CameraName;
                camera.CameraName = cam.CameraName;
            } else {
                MessageBox.Show("Invalid Camera Name", "Warning");
            }
        }

        ICommand clearCommand;
        public ICommand ClearCommand {
            get {
                if (clearCommand == null) { clearCommand = new DelegateCommand<string>(clear); }
                return clearCommand;
            }
        }

        ICommand addCommand;
        public ICommand AddCommand {
            get {
                if (addCommand == null) { addCommand = new DelegateCommand<ObservableCollection<CameraNameWrapper>>(add); }
                return addCommand;
            }
        }

        ICommand deleteCommand;
        public ICommand DeleteCommand {
            get {
                if (deleteCommand == null) { deleteCommand = new DelegateCommand<int?>(delete); }
                return deleteCommand;
            }
        }

        ICommand setCommand;
        public ICommand SetCommand {
            get {
                if (setCommand == null) { setCommand = new DelegateCommand<int?>(set); }
                return setCommand;
            }
        }

    }
}
