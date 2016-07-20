
using System;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Util;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Mvvm;
using MjpegProcessor;
using System.Windows;
using ptz_camera;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Camera {
    public class CameraVM: BindableBase {

        public ModeColors modeColors { get; set; }

        Visibility preview = Visibility.Hidden;
        public Visibility Preview { get { return preview; } set { SetProperty(ref preview, value); } }

        Visibility output = Visibility.Hidden;
        public Visibility Output { get { return output; } set { SetProperty(ref output, value); } }

        public CameraInfo camInfo;
        public CameraInfo CamInfo {
            get { return camInfo; }
            set { SetProperty(ref camInfo, value); }
        }

        CameraLoginForm form;
        CameraNamingForm namingForm;

        ObservableCollection<CameraNameWrapper> cameraNameList;

        protected readonly EventAggregator _ea;
        public MjpegDecoder mjpegDecoder;

        public CameraVM(CameraInfo cam, ObservableCollection<CameraNameWrapper> cameraNameList, ModeColors mode, EventAggregator ea) {
            CamInfo = cam;
            this.cameraNameList = cameraNameList;
            _ea = ea;
            _ea.GetEvent<CameraSelectEvent>().Subscribe(bePreview);
            _ea.GetEvent<CameraOutPutEvent>().Subscribe(beOutput);
            _ea.GetEvent<ClearCameraOutputEvent>().Subscribe(unOutput);
            _ea.GetEvent<ClearCameraPreviewEvent>().Subscribe(unPreview);
            _ea.GetEvent<InitSessionResponseReceivedEvent>().Subscribe(OnGetInitSessionResponse, ThreadOption.UIThread);
            _ea.GetEvent<StreamUriResponseReceivedEvent>().Subscribe(OnGetStreamUri, ThreadOption.UIThread);
            modeColors = mode;

            mjpegDecoder = new MjpegDecoder();
            mjpegDecoder.FrameReady += FrameReady;
            mjpegDecoder.Error += MjpegDecoderError;
        }

        private void connectionErrorHandler() {
            CamInfo.isLoggedIn = false;
            CamInfo.UserName = null;
            camInfo.Password = null;
            if (form != null) {
                form.activate();
            }
        }

        private void MjpegDecoderError(object sender, ErrorEventArgs e) {
            connectionErrorHandler();
        }

        public void FrameReady(object sender, FrameReadyEventArgs e) {
            if (form != null && form.IsActive == true) {
                form.Close();
            }
            if (!CamInfo.isLoggedIn) {
                CamInfo.isLoggedIn = true;
                updateCameraNameList();
            }
            CamInfo.VideoSource = e.BitmapImage;
        }

        public void unSubscribe() {
            mjpegDecoder.FrameReady -= FrameReady;
            mjpegDecoder.Error -= MjpegDecoderError;
            _ea.GetEvent<InitSessionResponseReceivedEvent>().Unsubscribe(OnGetInitSessionResponse);
            _ea.GetEvent<StreamUriResponseReceivedEvent>().Unsubscribe(OnGetStreamUri);
        }

        private void OnGetInitSessionResponse(init_session_response_t res) {
            if ( res.ip_address == CamInfo.IP ) {
                if (res.status_code == status_codes_t.OK) {
                    if (CamInfo.VideoURL != null && CamInfo.VideoURL != "" ) {
                        if (mjpegDecoder != null)
                            mjpegDecoder.ParseStream(new Uri(this.CamInfo.VideoURL, UriKind.Absolute), CamInfo.UserName, CamInfo.Password);
                    } 
                    else { CamInfo.getStreamUri(); }
                } else { connectionErrorHandler(); }
            } 
        }

        public void getStreamUri() {
            if (CamInfo.VideoURL != null && CamInfo.VideoURL != "") {
                if (mjpegDecoder != null)
                    mjpegDecoder.ParseStream(new Uri(this.CamInfo.VideoURL, UriKind.Absolute), CamInfo.UserName, CamInfo.Password);
            }
            else {CamInfo.getStreamUri();}
        }

        private void OnGetStreamUri(stream_uri_response_t res) {
            if (CamInfo.IP == res.ip_address) {
                if (res.status_code == status_codes_t.OK) {
                    if (mjpegDecoder != null) {
                        CamInfo.VideoURL = res.uri;
                        mjpegDecoder.ParseStream(new Uri(CamInfo.VideoURL, UriKind.Absolute), CamInfo.UserName, CamInfo.Password);
                    }
                } else { CamInfo.getStreamUri(); }
            }
        }


        public void connect() {
            CamInfo.initSession();
        }

        private void showLogginWindow() {
            if (!CamInfo.isLoggedIn) {
                form = new CameraLoginForm(this);
                form.ShowDialog();                
            }
        }

        private void showNamingWindow() {
            CameraNamingVM vm = new CameraNamingVM(cameraNameList, CamInfo);
            namingForm = new CameraNamingForm(vm);
            namingForm.ShowDialog();
        }

        private void updateCameraNameList() {
            foreach (CameraNameWrapper item in cameraNameList) {
                if (item.CameraName == CamInfo.CameraName) {
                    item.username = CamInfo.UserName;
                    item.password = CamInfo.Password;
                    break;
                }
            }
        }

        public void beOutput(CameraInfo cam) {
            if (CamInfo.CameraName == cam.CameraName) { Output = Visibility.Visible; } 
            else { Output = Visibility.Hidden; }
        }

        public void bePreview(CameraInfo cam) {
            if (CamInfo.CameraName == cam.CameraName) { Preview = Visibility.Visible; }
            else { Preview = Visibility.Hidden; }
        }

        private void unPreview(CameraInfo cam) {
            if (cam != null && CamInfo.CameraName == cam.CameraName) { Preview = Visibility.Hidden; }
        }

        private void unOutput(CameraInfo cam) {
            if (cam != null && CamInfo.CameraName == cam.CameraName) { Output = Visibility.Hidden; } 
        }

        void onLoggin(CameraInfo cam) {
            showLogginWindow();
        }

        void onSelected(CameraInfo cam) {
            // Uncomment it to get the feature of camera log in
            showLogginWindow();
            if (CamInfo.isLoggedIn) {
                _ea.GetEvent<CameraSelectEvent>().Publish(cam);
                _ea.GetEvent<StatusUpdateEvent>().Publish("Camera selected as preview");
            }
        }

        void onOutput(CameraInfo cam) {
            // Uncomment it to get the feature of camera log in
            showLogginWindow();
            if (CamInfo.isLoggedIn) {
                _ea.GetEvent<CameraOutPutEvent>().Publish(cam);
                _ea.GetEvent<StatusUpdateEvent>().Publish("Camera selected as output");
            }
        }

        void onName(CameraInfo cam) {
            showNamingWindow();
        }

        ICommand nameCommand;
        public ICommand NameCommand {
            get {
                if (nameCommand == null) { nameCommand = new DelegateCommand<CameraInfo>(onName); }
                return nameCommand;
            }
        }

        ICommand selectedCommand;
        public ICommand SelectedCommand {
            get {
                if (selectedCommand == null) { selectedCommand = new DelegateCommand<CameraInfo>(onSelected); }
                return selectedCommand;
            }
        }

        ICommand outputCommand;
        public ICommand OutputCommand {
            get {
                if (outputCommand == null) { outputCommand = new DelegateCommand<CameraInfo>(onOutput); }
                return outputCommand;
            }
        }

        ICommand logginCommand;
        public ICommand LogginCommand {
            get {
                if (logginCommand == null) { logginCommand = new DelegateCommand<CameraInfo>(onLoggin); }
                return logginCommand;
            }
        }

    }





}
