using Microsoft.Practices.Prism.PubSubEvents;
using Util;
using System.Windows;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using System;
using NotificationCenter;
using System.Collections.Generic;

namespace Preview {
    public class PreviewVM: BindableBase {

        bool previewIsForbidden;
        private bool PreviewIsForbidden {
            get { return previewIsForbidden; }
            set {
                previewIsForbidden = value;
                if (previewIsForbidden)
                    Forbidden = Visibility.Visible;
                else
                    Forbidden = Visibility.Hidden;
            }
        }

        Visibility forbidden;
        public Visibility Forbidden {
            get { return forbidden; }
            set { SetProperty(ref forbidden, value); }
        }

        private bool isUndoMode = false;
        private bool isRedoMode = false;

        Visibility idle;
        public Visibility Idle {
            get { return idle; }
            set { SetProperty(ref idle, value);}
        }

        Visibility active;
        public Visibility Active {
            get { return active; }
            set { SetProperty(ref active, value); }
        }

        public UndoRedo<ptz> UndoRedoManager { get; set; }

        protected readonly EventAggregator _ea;

        public ModeColors modeColors { get; set; }

        CameraInfo currentCamera;
        public CameraInfo CurrentCamera {
            get { return currentCamera; }
            set { SetProperty(ref currentCamera, value); }
        }

        private PresetParams currentSetting;
        public PresetParams CurrentSetting {
            get { return currentSetting; }
            set { SetProperty(ref currentSetting, value); }
        }

        int sliderPan;
        public int SliderPan {
            get { return sliderPan; }
            set {
                if (!previewIsForbidden) {
                    if (CurrentCamera != null && sliderPan != value) {
                        CurrentCamera.changePan(PTZ_MODE.Absolute, value);
                        SetProperty(ref sliderPan, value);
                    }
                    if (!isRedoMode && !isUndoMode) {
                        ptz dataPoint = new ptz(sliderPan, sliderTilt, sliderZoom);
                        UndoRedoManager.add(dataPoint);
                    }
                }
            }
        }

        int sliderTilt;
        public int SliderTilt {
            get { return sliderTilt; }
            set {
                if (!previewIsForbidden) {
                    if (CurrentCamera != null && sliderTilt != value) {
                        CurrentCamera.changeTilt(PTZ_MODE.Absolute, value);
                        SetProperty(ref sliderTilt, value);
                    }
                    if (!isRedoMode && !isUndoMode) {
                        ptz dataPoint = new ptz(sliderPan, sliderTilt, sliderZoom);
                        UndoRedoManager.add(dataPoint);
                    }
                }
            }
        }

        int sliderZoom;
        public int SliderZoom {
            get { return sliderZoom; }
            set {
                if (!previewIsForbidden) {
                    if (CurrentCamera != null && sliderZoom != value) {
                        CurrentCamera.changeZoom(PTZ_MODE.Absolute, value);
                        SetProperty(ref sliderZoom, value);
                    }
                    if (!isRedoMode && !isUndoMode) {
                        ptz dataPoint = new ptz(sliderPan, sliderTilt, sliderZoom);
                        UndoRedoManager.add(dataPoint);
                    }
                }
            }
        }

        public PreviewVM() {
            currentSetting = null;
            PreviewIsForbidden = false;
            _ea = Notification.Instance;
            modeColors = ModeColors.Singleton(_ea);
            _ea.GetEvent<CameraSelectEvent>().Subscribe(acceptCamera);
            _ea.GetEvent<SetPresetEvent>().Subscribe(acceptPreset);
            _ea.GetEvent<CameraDiscoverEvent>().Subscribe(clearBeforeDiscover);
            _ea.GetEvent<PreviewPauseEvent>().Subscribe(pausePreview);
            _ea.GetEvent<PreviewResumeEvent>().Subscribe(resumePreview);
            _ea.GetEvent<MenuBarToPreviewEvent>().Subscribe(processMenuBarCommand);
            UndoRedoManager = new UndoRedo<ptz>(Constant.UNDO_BUFFER_SIZE);
            Idle = Visibility.Visible;
            Active = Visibility.Hidden;
            Forbidden = Visibility.Hidden;
            SliderPan = 0;
            SliderTilt = 0;
            UndoRedoManager.clear();
            SliderZoom = 1;

        }

        private void pausePreview(bool i) {
            PreviewIsForbidden = true;
            if (CurrentCamera != null)
                CurrentCamera.stopUpdatePTZ();
        }

        private void resumePreview(bool i) {
            PreviewIsForbidden = false;
            if (CurrentCamera != null)
                CurrentCamera.startUpdatePTZ();
        }

        public void acceptCamera(CameraInfo cam) {
            if (CurrentCamera != null)
                CurrentCamera.stopUpdatePTZ();
            CurrentCamera = cam;
            if (!previewIsForbidden) { CurrentCamera.startUpdatePTZ(); }
            Idle = Visibility.Hidden;
            Active = Visibility.Visible;
            SliderPan = Convert.ToInt32(CurrentCamera.Pan);
            SliderTilt = Convert.ToInt32(currentCamera.Tilt);
            UndoRedoManager.clear();
            SliderZoom = Convert.ToInt32(CurrentCamera.Zoom);
            
        }

        public void acceptPreset(PresetParams preset) {
            if (CurrentCamera == null || preset.CameraName != CurrentCamera.CameraName || preset.presettingId == null) {
                MessageBox.Show("Preset cannot be applied to this camera.");
            } else {
                CurrentSetting = preset;
                // CurrentCamera.changePTZ(PTZ_MODE.Absolute, preset.pan, preset.tilt, preset.zoom);
                SliderPan = Convert.ToInt32(preset.pan);
                SliderTilt = Convert.ToInt32(preset.tilt);
                UndoRedoManager.clear();
                SliderZoom = Convert.ToInt32(preset.zoom);
            }
        }

        private void saveSetting(CameraInfo camInfo) {
            if (CurrentCamera != null) {
                if (CurrentSetting != null) {
                    CurrentSetting.pan = camInfo.Pan;
                    CurrentSetting.tilt = camInfo.Tilt;
                    CurrentSetting.zoom = camInfo.Zoom;
                } else {
                    CurrentSetting = new PresetParams("", CurrentCamera.CameraName, currentCamera.Pan, CurrentCamera.Tilt, CurrentCamera.Zoom);
                }
                _ea.GetEvent<SaveSettingEvent>().Publish(CurrentSetting);
            }
        }

        private void saveAsNew(CameraInfo camInfo) {
            if (CurrentCamera != null) {
                if (CurrentSetting != null) {
                    CurrentSetting.pan = camInfo.Pan;
                    CurrentSetting.tilt = camInfo.Tilt;
                    CurrentSetting.zoom = camInfo.Zoom;
                } else {
                    CurrentSetting = new PresetParams("", CurrentCamera.CameraName, currentCamera.Pan, CurrentCamera.Tilt, CurrentCamera.Zoom);
                }
                _ea.GetEvent<SaveSettingAsNewEvent>().Publish(CurrentSetting);
            }
        }

        private void clearBeforeDiscover(string disc) {
            clear(CurrentCamera);
        }

        private void clear(CameraInfo camInfo) {
            _ea.GetEvent<ClearCameraPreviewEvent>().Publish(CurrentCamera);
            CurrentSetting = null;
            CurrentCamera = null;
            Idle = Visibility.Visible;
            Active = Visibility.Hidden;
            UndoRedoManager.clear();
        }

        private void undo(UndoRedo<ptz> ud) {
            isUndoMode = true;
            ptz datapint = UndoRedoManager.undo();
            SliderPan = datapint.pan;
            SliderTilt = datapint.tilt;
            SliderZoom = datapint.zoom;
            isUndoMode = false;
        }

        private void redo(UndoRedo<ptz> rd) {
            isRedoMode = true;
            ptz datapint = UndoRedoManager.redo();
            SliderPan = datapint.pan;
            SliderTilt = datapint.tilt;
            SliderZoom = datapint.zoom;
            isRedoMode = false;
        }

        private void processMenuBarCommand(string input) {
            if (input == "SaveSetting")
                saveSetting(CurrentCamera);
            if (input == "SaveSettingAsNew")
                saveAsNew(CurrentCamera);
            if (input == "ClearPreview")
                clear(CurrentCamera);
        }


        ICommand undoCommand;
        public ICommand UndoCommand {
            get {
                if (undoCommand == null) {
                    undoCommand = new DelegateCommand<UndoRedo<ptz>>(undo);
                }
                return undoCommand;
            }
        }

        ICommand redoCommand;
        public ICommand RedoCommand {
            get {
                if (redoCommand == null) {
                    redoCommand = new DelegateCommand<UndoRedo<ptz>>(redo);
                }
                return redoCommand;
            }
        }

    }

    public struct ptz {
        public short pan;
        public short tilt;
        public byte zoom;

        public ptz(double p, double t, double z) {
            pan = (short)p;
            tilt = (short)t;
            zoom = (byte)z;
        }
    }

    public class UndoRedo<T>: BindableBase {

        public int BufferSize;

        protected Deque<T> UndoDeque;
        protected Deque<T> RedoDeque;

        private bool _canUndo;
        public bool CanUndo { get { return _canUndo; } set { SetProperty(ref _canUndo, value); } }

        private bool _canRedo;
        public bool CanRedo { get { return _canRedo; } set { SetProperty(ref _canRedo, value); } }

        public UndoRedo(int buffer){
            BufferSize = buffer;
            UndoDeque = new Deque<T>();
            RedoDeque = new Deque<T>();
            CanUndo = false;
            CanRedo = false;
        }

        public void clear() {
            if (UndoDeque.Count > 0) { UndoDeque.RemoveRange(0, UndoDeque.Count); }
            if (RedoDeque.Count > 0) { RedoDeque.RemoveRange(0, RedoDeque.Count); }
            CanUndo = false;
            CanRedo = false;
            
        }

        public T undo() {
            if (UndoDeque.Count > 1) {
                T item = UndoDeque.RemoveBack();
                RedoDeque.AddBack(item);
                if (UndoDeque.Count == 1) { CanUndo = false; }
                CanRedo = true;
                return UndoDeque.Get(UndoDeque.Count-1);
            }
            if (UndoDeque.Count == 1) { CanUndo = false; }
            return default(T);
        }

        public void add(T item) {
            if (UndoDeque.Count > BufferSize) { UndoDeque.RemoveFront(); }
            UndoDeque.AddBack(item);
            if (UndoDeque.Count > 1) { CanUndo = true; }
            if (RedoDeque.Count > 0) {
                RedoDeque.RemoveRange(0, RedoDeque.Count);
                CanRedo = false;
            }
        }

        public T redo() {
            if (RedoDeque.Count > 0) {
                T item = RedoDeque.RemoveBack();
                UndoDeque.AddBack(item);
                if (RedoDeque.Count == 0) { CanRedo = false; }
                CanUndo = true;
                return item;
            }
            if (RedoDeque.Count == 0) { CanRedo = false; }
            return default(T);
        }
    }
}
