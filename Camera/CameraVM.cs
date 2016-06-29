using System;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Util;
using Microsoft.Practices.Prism.PubSubEvents;
using System.Diagnostics;
using Microsoft.Practices.Prism.Mvvm;
using MjpegProcessor;
using System.Windows;

namespace Camera {
    public class CameraVM: BindableBase {

        public ModeColors modeColors { get; set; }

        Visibility selected = Visibility.Hidden;
        public Visibility Selected { get { return selected; } set { SetProperty(ref selected, value); } }

        SolidColorBrush _outputBackgroundColor = Brushes.Gray;
        public SolidColorBrush OutputBackgroundColor { get { return _outputBackgroundColor; } set { SetProperty(ref _outputBackgroundColor, value); } }

        public CameraInfo camInfo;
        public CameraInfo CamInfo {
            get { return camInfo; }
            set { SetProperty(ref camInfo, value); }
        }

        CameraLoginForm form;

        protected readonly EventAggregator _ea;
        public MjpegDecoder mjpegDecoder = new MjpegDecoder();

        public CameraVM(CameraInfo cam, ModeColors mode ,EventAggregator ea) {
            CamInfo = cam;
            _ea = ea;
            _ea.GetEvent<CameraSelectEvent>().Subscribe(beSelected);
            _ea.GetEvent<CameraOutPutEvent>().Subscribe(beOutput);
            modeColors = mode;

            mjpegDecoder = new MjpegDecoder();
            mjpegDecoder.FrameReady += FrameReady;
            mjpegDecoder.Error += MjpegDecoderError;
            //connect();
            
        }

        private void MjpegDecoderError(object sender, ErrorEventArgs e) {
            this.CamInfo.isLoggedIn = false;
            this.CamInfo.UserName = null;
            this.camInfo.Password = null;
            form.activate();
        }

        public void FrameReady(object sender, FrameReadyEventArgs e) {
            if (form != null && form.IsActive == true) {
                form.Close();
            }
            this.CamInfo.isLoggedIn = true;
            this.CamInfo.dispatcherTimer.Start();
            CamInfo.VideoSource = e.BitmapImage;
        }

        public void connect() {
            mjpegDecoder.ParseStream(new Uri(this.CamInfo.VideoURL, UriKind.Absolute), this.CamInfo.UserName, this.CamInfo.Password);
        }

        private void showLogginWindow() {
            if (!CamInfo.isLoggedIn) {
                form = new CameraLoginForm(this);
                form.ShowDialog();
            }
        }
        public void beOutput(CameraInfo cam) {
            if (this.CamInfo.CameraID == cam.CameraID) {                
                this.OutputBackgroundColor = Brushes.LightGreen;
            } else {
                this.OutputBackgroundColor = Brushes.Gray;
            }
        }

        public void beSelected(CameraInfo param) {
            if (this.CamInfo.CameraID == param.CameraID) {
                this.Selected = Visibility.Visible;
            }
            else {
                this.Selected = Visibility.Hidden;
            }
        }

        void onLoggin(CameraInfo cam) {
            showLogginWindow();
        }

        void onSelected(CameraInfo cam) {
            // Uncomment it to get the feature of camera log in
            //showLogginWindow();
            //if (this.CamInfo.isLoggedIn) {
                _ea.GetEvent<CameraSelectEvent>().Publish(cam);
                _ea.GetEvent<StatusUpdateEvent>().Publish("Camera selected as preview");
            //}
        }

        void onOutput(CameraInfo cam) {
            // Uncomment it to get the feature of camera log in
            //showLogginWindow();
            //if (this.CamInfo.isLoggedIn) {
                _ea.GetEvent<CameraOutPutEvent>().Publish(cam);
                _ea.GetEvent<StatusUpdateEvent>().Publish("Camera selected as output");
            //}
        }

        ICommand selectedCommand;
        public ICommand SelectedCommand {
            get {
                if (selectedCommand == null) {
                    selectedCommand = new DelegateCommand<CameraInfo>(onSelected);
                }
                return selectedCommand;
            }
        }

        ICommand outputCommand;
        public ICommand OutputCommand {
            get {
                if (outputCommand == null) {
                    outputCommand = new DelegateCommand<CameraInfo>(onOutput);
                }
                return outputCommand;
            }
        }

        ICommand logginCommand;
        public ICommand LogginCommand {
            get {
                if (logginCommand == null) {
                    logginCommand = new DelegateCommand<CameraInfo>(onLoggin);
                }
                return logginCommand;
            }
        }

    }





}
