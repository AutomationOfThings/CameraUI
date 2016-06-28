using Microsoft.Practices.Prism.PubSubEvents;
using Util;
using System.Windows;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using System.Diagnostics;
using System;

namespace PreviewPanel {
    public class PreviewPanel: BindableBase {

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

        protected readonly IEventAggregator _ea;

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

        /*
        Thickness panPosition;
        public Thickness PanPosition {
            get { return panPosition; }
            set { SetProperty(ref panPosition, value); }
        }

        Thickness tiltPosition;
        public Thickness TiltPosition {
            get { return tiltPosition; }
            set { SetProperty(ref tiltPosition, value); }
        }
        */

        int sliderPan;
        public int SliderPan {
            get { return sliderPan; }
            set {
                SetProperty(ref sliderPan, value);
                if (CurrentCamera != null) {
                    CurrentCamera.ChangePan(PTZ_MODE.Absolute, value);
                }
            }
        }

        int sliderTilt;
        public int SliderTilt {
            get { return sliderTilt; }
            set {
                SetProperty(ref sliderTilt, value);
                if (CurrentCamera != null) {
                    CurrentCamera.ChangeTilt(PTZ_MODE.Absolute, value);
                }
            }
        }

        public PTZcmd? Increase { get; set; } = PTZcmd.Increase;
        public PTZcmd? Decrease { get; set; } = PTZcmd.Decrease;

        public PreviewPanel(IEventAggregator eventAggregator) {
            this.currentSetting = null;
            this._ea = eventAggregator;
            this._ea.GetEvent<CameraSelectEvent>().Subscribe(acceptCamera);
            this._ea.GetEvent<SetPresetEvent>().Subscribe(acceptPreset);
            this.Idle = Visibility.Visible;
            this.Active = Visibility.Hidden;
            SliderPan = 0;
            SliderTilt = 0;
            //this.PanPosition = new Thickness(0,0,0,0);
            //this.TiltPosition = new Thickness(0,0,0,0);
        }

        public void acceptCamera(CameraInfo cam) {
            CurrentCamera = cam;
            Idle = Visibility.Hidden;
            Active = Visibility.Visible;
            SliderPan = Convert.ToInt32(CurrentCamera.Pan);
            SliderTilt = Convert.ToInt32(currentCamera.Tilt);
        }

        public void acceptPreset(PresetParams preset) {
            if (CurrentCamera == null || preset.CamId != CurrentCamera.CameraID) {
                MessageBox.Show("Preset cannot be applied to this camera.");
            } else {
                this.CurrentSetting = preset;
                currentCamera.setPTZ(preset.pan, preset.tilt, preset.zoom);
                SliderPan = Convert.ToInt32(preset.pan);
                SliderTilt = Convert.ToInt32(preset.tilt);
            }
        }

        /*
        public void onPan(PTZcmd? cmd) {
            CurrentCamera.ChangePanHandler(cmd);
        }

        public void onTilt(PTZcmd? cmd) {
            CurrentCamera.ChangeTiltHandler(cmd);
        }
       */
        public void onZoom(PTZcmd? cmd) {
            CurrentCamera.ChangeZoomHandler(cmd);
        }

        private void saveSetting(CameraInfo camInfo) {
            if (CurrentSetting != null) {
                CurrentSetting.pan = camInfo.Pan;
                CurrentSetting.tilt = camInfo.Tilt;
                CurrentSetting.zoom = camInfo.Zoom;
                _ea.GetEvent<SaveSettingEvent>().Publish(currentSetting);
            }
        }


        // ICommand:
        /*
        ICommand panCommand;
        public ICommand PanCommand {
            get {
                if (panCommand == null) {
                    panCommand = new DelegateCommand<PTZcmd?>(onPan);
                }
                return panCommand;
            }
        }

        ICommand tiltCommand;
        public ICommand TiltCommand {
            get {
                if (tiltCommand == null) {
                    tiltCommand = new DelegateCommand<PTZcmd?>(onTilt);
                }
                return tiltCommand;
            }
        }
        */

        ICommand zoomCommand;
        public ICommand ZoomCommand {
            get {
                if (zoomCommand == null) {
                    zoomCommand = new DelegateCommand<PTZcmd?>(onZoom);
                }
                return zoomCommand;
            }
        }

        ICommand saveSettingCommand;
        public ICommand SaveSettingCommand {
            get {
                if (saveSettingCommand == null) {
                    saveSettingCommand = new DelegateCommand<CameraInfo>(saveSetting);
                }
                return saveSettingCommand;
            }
        }

    }
}
