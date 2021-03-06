﻿
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Threading;
using ptz_camera;
using NotificationCenter;
using System.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;

namespace Util {
    public class PresetParams {
        public string CameraName { get; set; }
        public string presettingId;
        public double pan;
        public double tilt;
        public double zoom;

        public PresetParams() {}

        public PresetParams(string presetId, string id, double p, double t, double z) {
            presettingId = presetId;
            CameraName = id;
            pan = p;
            tilt = t;
            zoom = z;
        }

        public void update(string presetId, string ID, double p, double t, double z) {
            presettingId = presetId;
            CameraName = ID;
            pan = p;
            tilt = t;
            zoom = z;
        }
    }

    public class PresetParamsExtend : BindableBase {

        private bool _canSave;
        public bool CanSave { get { return _canSave; } set { SetProperty(ref _canSave, value); } }

        string presettingId;
        public string PresettingId {
            get { return presettingId; }
            set {
                SetProperty(ref presettingId, value);
                CanSave = true;
            }
        }
        CameraNameWrapper camera;
        public CameraNameWrapper Camera {
            get { return camera; }
            set {
                SetProperty(ref camera, value);
                CanSave = true;
            }
        }

        double pan;
        public double Pan {
            get { return pan; }
            set {
                SetProperty(ref pan, value);
                CanSave = true;
            }
        }

        double tilt;
        public double Tilt {
            get { return tilt; }
            set {
                SetProperty(ref tilt, value);
                CanSave = true;
            }
        }

        double zoom;
        public double Zoom {
            get { return zoom; }
            set {
                SetProperty(ref zoom, value);
                CanSave = true;
            }
        }

        public ObservableCollection<CameraNameWrapper> CameraNameList { get; set; }

        public PresetParamsExtend(ObservableCollection<CameraNameWrapper> cameraNameList) {
            Pan = 0;
            Tilt = 0;
            Zoom = 1;
            CanSave = false;
            CameraNameList = cameraNameList;
        }

        public PresetParamsExtend(string presetId, string id, ObservableCollection<CameraNameWrapper> cameraNameList, double p, double t, double z) {
            CameraNameList = cameraNameList;
            presettingId = presetId;
            Camera = new CameraNameWrapper(null);
            pan = p;
            tilt = t;
            zoom = z;
            CanSave = false;
            foreach (CameraNameWrapper item in CameraNameList) {
                if (item.CameraName == id) { Camera = item; break; }
            }
        }

        public string toString() {
            return "CameraID: " + Camera.ToString() + "   Pan: " + Pan.ToString() + "   Tilt: " + Tilt.ToString() + "   Zoom: " + Zoom.ToString();
        }

    }

    public class ProgramInfo: BindableBase {

        string programName;
        public string ProgramName {
            get { return programName; }
            set { SetProperty(ref programName, value); }
            }
        public List<CameraCommand> commandList;

    }

    public class CameraNameWrapper: BindableBase {

        private string cameraName;
        public string CameraName {
            get { return cameraName; }
            set { SetProperty(ref cameraName, value); }
        }

        private bool notAssociated;
        public bool NotAssociated {
            get { return notAssociated; }
            set { SetProperty(ref notAssociated, value); }
        }

        private string associatedIP;
        public string AssociatedIP {
            get { return associatedIP; }
            set { SetProperty(ref associatedIP, value); }
        }

        public string username { get; set; }
        public string password { get; set; }

        public CameraNameWrapper(string name) {
            CameraName = name;
            NotAssociated = false;
        }

    }

    public class CameraInfo: BindableBase {

        private long lastPtzUpdateTime;

        private EventAggregator _ea;

        public bool isLoggedIn;

        string ip;
        public string IP {
            get { return ip; }
            set { SetProperty(ref ip, value); }
        }

        string cameraName;
        public string CameraName {
            get { return cameraName; }
            set { SetProperty(ref cameraName, value); }
        }

        public string VideoURL { get; set; }

        string userName;
        public string UserName {
            get { return userName; }
            set { SetProperty(ref userName, value); }
        }

        string password;
        public string Password {
            get { return password; }
            set { SetProperty(ref password, value); }
        }

        ImageSource videoSource;
        public ImageSource VideoSource {
            get { return videoSource; }
            set { SetProperty(ref videoSource, value); }
        }

        double pan;
        public double Pan {
            get { return pan; }
            set {
                if (value < 0) { value += 360; } 
                else if (value > 360) { value %= 361; }
                SetProperty(ref pan, value);
            }
        }

        double tilt;
        public double Tilt {
            get { return tilt; }
            set {
                if (value >= -15 && value <= 195) { SetProperty(ref tilt, value); }
            }
        }

        double zoom;
        public double Zoom {
            get { return zoom; }
            set {
                if (value > 0 && value <= 32) { SetProperty(ref zoom, value); }
            }
        }

        double expectedPan;
        double expectedTilt;
        double expectedZoom;

        public DispatcherTimer dispatcherTimer;

        public CameraInfo(string ip, string id, double p, double t, double z) {
            isLoggedIn = false;
            IP = ip;
            CameraName = id;
            expectedPan = Pan = p;
            expectedTilt = Tilt = t;
            expectedZoom = Zoom = z;

            lastPtzUpdateTime = DateTime.Now.Ticks / 10000;
            _ea = Notification.Instance;
            _ea.GetEvent<PositionResponseReceivedEvent>().Subscribe(onGetPositionUpdate);
            _ea.GetEvent<EndSessionResponseReceivedEvent>().Subscribe(OnGetEndSessionResponse);

            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            dispatcherTimer.Tick += new EventHandler(updatePTZ);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 750);
        }

        public void getStreamUri() {
            CameraConnnector.getStreamUri(IP);
        }

        private void onGetPositionUpdate(position_response_t res) {
            if (IP == res.ip_address && res.status_code == status_codes_t.OK) {
                try {
                    Pan = double.Parse(res.pan_value);
                    Tilt = double.Parse(res.tilt_value);
                    Zoom = double.Parse(res.zoom_value);
                    if (Pan != expectedPan || Tilt != expectedTilt || Zoom != expectedZoom) {
                        changePTZ(PTZ_MODE.Absolute, expectedPan, expectedTilt, expectedZoom);
                    }

                } catch(Exception) { return; }
                
            }
        }

        public void initSession() {
            CameraConnnector.initializeSession(IP, UserName, Password);
        }

        public void endSession() {
            CameraConnnector.endSession(IP);
        }

        private void OnGetEndSessionResponse(end_session_response_t res){
            if (res.ip_address == IP) {
                if (res.response == "") {
                    UserName = null;
                    Password = null;
                } else {
                    endSession();
                }
            }
        }

        public void startUpdatePTZ() {
            if (!dispatcherTimer.IsEnabled)
                dispatcherTimer.Start();
        }

        public void stopUpdatePTZ() {
            if (dispatcherTimer.IsEnabled)
                dispatcherTimer.Stop();
        }

        private void updatePTZ(object sender, EventArgs e) {
            CameraConnnector.requestCameraPosition(IP);
        }

        public void changePan(PTZ_MODE mode, double p) {
            expectedPan = p;
        }

        public void changeTilt(PTZ_MODE mode, double t) {
            expectedTilt = t;
        }

        public void changeZoom(PTZ_MODE mode, double z) {
            expectedZoom = z;
        }

        public void changePTZ(PTZ_MODE mode, double p, double t, double z) {
            if (Zoom > 22) {
                CameraConnnector.requestPtzControl(IP, mode, (int)Pan, (int)Tilt, (int)z);
                CameraConnnector.requestPtzControl(IP, mode, (int)p, (int)t, (int)z);
            } else if (Zoom < 12) {
                CameraConnnector.requestPtzControl(IP, mode, (int)p, (int)t, (int)Zoom);
                CameraConnnector.requestPtzControl(IP, mode, (int)p, (int)t, (int)z);
            } else {
                CameraConnnector.requestPtzControl(IP, mode, (int)p, (int)t, (int)z);
            }
            
        }

        /* these set of functions use .NET HttpWebRequest to connect with SumSung Camera
           however it hard-codes the ip address of the camera
         
        private void updatePTZ(object sender, EventArgs e) {
            ptzQueryRequest();
        }

        HttpWebRequest request;

        public void ChangePan(PTZ_MODE mode, double p) {
            string uri = preparePanUri(mode, p);
            sendRequest(uri);
        }

        public void ChangeTilt(PTZ_MODE mode, double t) {
            string uri = prepareTiltUri(mode, t);
            sendRequest(uri);
        }

        public void ChangeZoom(PTZ_MODE mode, double z) {
            ChangeZoom(z);
        }
        public void ChangeZoom(double z) {
            string uri = prepareZoomUri(PTZ_MODE.Absolute, z);
            sendRequest(uri);
        }

        public void setPTZ(double p, double t, double z) {
            string uri = preparePTZUri(p, t, z);
            sendRequest(uri);
        }

        public void ptzQueryRequest() {
            string uri = preparePtzQueryUri();
            //HttpConnector.username = UserName;
            //HttpConnector.password = Password;
            //HttpConnector.requestUseHTTP(uri, RespCallBack);
        }

        private string preparePTZUri(double p, double t, double z) {
            return "http://" + IP + "/stw-cgi/ptzcontrol.cgi?msubmenu=absolute&action=control&Pan=" + p + "&Tilt=" + t + "&Zoom=" + z;
        }

        private string preparePanUri(PTZ_MODE mode, double p) {
            if (mode == PTZ_MODE.Relative) {
                return "http://" + IP + "/stw-cgi/ptzcontrol.cgi?msubmenu=relative&action=control&Pan=" + p;
            } else {
                return "http://" + IP + "/stw-cgi/ptzcontrol.cgi?msubmenu=absolute&action=control&Pan=" + p;
            }
        }

        private string prepareTiltUri(PTZ_MODE mode, double t) {
            if (mode == PTZ_MODE.Relative) {
                return "http://" + IP + "/stw-cgi/ptzcontrol.cgi?msubmenu=relative&action=control&Tilt=" + t;
            } else {
                return "http://" + IP + "/stw-cgi/ptzcontrol.cgi?msubmenu=absolute&action=control&Tilt=" + t;
            }
            
        }

        private string prepareZoomUri(PTZ_MODE mode, double z) {
            if (mode == PTZ_MODE.Relative) {
                return "http://" + IP + "/stw-cgi/ptzcontrol.cgi?msubmenu=relative&action=control&Zoom=" + z;
            } else {
                return "http://" + IP + "/stw-cgi/ptzcontrol.cgi?msubmenu=absolute&action=control&Zoom=" + z;
            }
            
        }

        private string preparePtzQueryUri() {
            return "http://" + IP + "/stw-cgi/ptzcontrol.cgi?msubmenu=query&action=view&Query=Pan,Tilt,Zoom";
        }

        private void sendRequest(string uri) {
            request = (HttpWebRequest)WebRequest.Create(uri);
            if (!string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(Password))
                request.Credentials = new NetworkCredential(UserName, Password);
            request.BeginGetResponse(OnGetResponse, request);
            Debug.WriteLine(uri);
        }

        private void OnGetResponse(IAsyncResult asyncResult) {
            try {
                HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResult);
                response.Close();
            } catch (Exception e) {
                Debug.WriteLine("opps!");
            }
        }


        private void RespCallBack(string result) {
            Dictionary<string, double>  ptz = parsePtzResult(result);
            if (ptz.ContainsKey("Pan")) {
                this.Pan = ptz["Pan"];
            }
            if (ptz.ContainsKey("Tilt")) {
                this.Tilt = ptz["Tilt"];
            }
            if (ptz.ContainsKey("Zoom")) {
                this.Zoom = ptz["Zoom"];
            }
        }

        private Dictionary<string, double> parsePtzResult(string rawResult) {
            Dictionary<string, double> result = new Dictionary<string, double>();
            string[] lines = rawResult.Split(new string[] { Environment.NewLine}, StringSplitOptions.None);
            foreach (string line in lines) {
                if (line.Length > 0) {
                    double number;
                    string[] keyValuePair = line.Split(new string[] { "=" }, StringSplitOptions.None);
                    if (double.TryParse(keyValuePair[1], out number)) {
                        result[keyValuePair[0]] = number;
                    }
                }
            }
            return result;
        }
        */

    }

    public struct PTZ{
        public string cameraID;
        public double pan;
        public double tilt;
        public double zoom;

        public PTZ(string cam, double p, double t, double z) {
            cameraID = cam;
            pan = p;
            tilt = t;
            zoom = z;
        }
    }

    public enum PTZ_MODE {
        Absolute,
        Relative,
        Continuous
    }

    public enum Cmd {
        OUTPUT,
        PRESET,
        WAIT
    }

    public enum PTZcmd {
        Increase,
        Decrease
    }

    public class CameraCommand: BindableBase {
        protected Cmd command;
        public Cmd Command {
            get { return command; }
            set { SetProperty(ref command, value); }
        }

        public string Parameter {
            get {
                if (Command == Cmd.OUTPUT)
                    return SelectedOutputCamera;
                else if (Command == Cmd.PRESET)
                    return SelectedPreset;
                else if (Command == Cmd.WAIT)
                    return WaitTime.ToString();
                else
                    return "nil";
            }
        }

        double waitTime;
        public double WaitTime {
            get { return waitTime; }
            set { SetProperty(ref waitTime, value); }
        }

        string selctedPreset;
        public string SelectedPreset {
            get { return selctedPreset; }
            set { SetProperty(ref selctedPreset, value); }
        }
        string selectedOutputCamera;
        public string SelectedOutputCamera {
            get { return selectedOutputCamera; }
            set { SetProperty(ref selectedOutputCamera, value); }
        }

        public CameraCommand() {}

        public CameraCommand(Cmd cmd, double time, string cam, string preset) {
            this.Command = cmd;
            this.WaitTime = time;
            this.SelectedOutputCamera = cam;
            this.SelectedPreset = preset;
        }

        public CameraCommand(CameraCommandEditWrapper wrapper)
        {
            this.Command = wrapper.Command;
            this.WaitTime = wrapper.WaitTime;
            this.SelectedOutputCamera = wrapper.SelectedOutputCamera;
            this.SelectedPreset = wrapper.SelectedPreset;
        }

        public string toString() {
            return Command.ToString() + ": " + Parameter;
        }
    }
    public class CameraCommandEditWrapper: CameraCommand {
        public List<Cmd> CommandList { get; set; }
        public List<string> CamList { get; set; }
        public List<string> PresetList { get; set; }

        public CameraCommandEditWrapper(CameraCommand cmd, List<string> camList, List<string> presetList) {
            CommandList = new List<Cmd> { Cmd.OUTPUT, Cmd.PRESET, Cmd.WAIT };
            this.Command = cmd.Command;
            this.WaitTime = cmd.WaitTime;
            this.SelectedOutputCamera = cmd.SelectedOutputCamera;
            this.SelectedPreset = cmd.SelectedPreset;
            this.CamList = camList;
            this.PresetList = presetList;
        }

        public CameraCommandEditWrapper(List<string> camList, List<string> presetList) {
            CommandList = new List<Cmd> { Cmd.OUTPUT, Cmd.PRESET, Cmd.WAIT };
            WaitTime = 10;
            command = Cmd.WAIT;
            CamList = camList;
            PresetList = presetList;
        }

    }

    public class Encoder {
        // Change the two values below to be something other than the example.
        // Once changed and in use, do not change the value below again or you
        // won't be able to decrypt previously stored passwords.
        string mByteArray = "@^%*(^&%$$#@^*^&)&*^$#!^#*%(&)*)";
        byte[] mInitializationVector = { 0x01, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xf7, 0xEF };

        public Encoder() {
        }

        public string Encrypt(string Password) {
            return EncryptWithByteArray(Password, mByteArray);
        }

        private string EncryptWithByteArray(string inPassword, string inByteArray) {
            try {
                byte[] tmpKey = new byte[20];
                tmpKey = System.Text.Encoding.UTF8.GetBytes(inByteArray.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputArray = System.Text.Encoding.UTF8.GetBytes(inPassword);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(tmpKey, mInitializationVector), CryptoStreamMode.Write);
                cs.Write(inputArray, 0, inputArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            } catch (Exception ex) {
                throw ex;
            }
        }

        public string Decrypt(string enPassword) {
            return DecryptWithByteArray(enPassword, mByteArray);
        }

        private string DecryptWithByteArray(string strText, string strEncrypt) {
            try {
                byte[] tmpKey = new byte[20];
                tmpKey = System.Text.Encoding.UTF8.GetBytes(strEncrypt.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                Byte[] inputByteArray = inputByteArray = Convert.FromBase64String(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(tmpKey, mInitializationVector), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = System.Text.Encoding.UTF8;
                return encoding.GetString(ms.ToArray());
            } catch (Exception ex) {
                throw ex;
            }
        }

        public string ByteArray {
            get { return mByteArray; }
            set { mByteArray = value; }
        }
    }

    public class ModeColors: BindableBase {

        private EventAggregator _ea;

        public ModeColors() {
            init();
        }

        public ModeColors(EventAggregator ea) {
            init();
            _ea = ea;
            _ea.GetEvent<ChangeModeEvent>().Subscribe(changeMode);
        }

        private static readonly ModeColors _instance = new ModeColors();

        public static ModeColors Singleton(EventAggregator ea) {
            _instance._ea = ea;
            _instance._ea.GetEvent<ChangeModeEvent>().Subscribe(_instance.changeMode);
            return _instance;
        }

        // Color Properties. FirstColor refers to light mode, second refers to dark mode
        // Background colors:
        SolidColorBrush modeColor_WhiteSmoke_MedianDark;
        public SolidColorBrush ModeColor_WhiteSmoke_MedianDark {
            get { return modeColor_WhiteSmoke_MedianDark;}
            set { SetProperty(ref modeColor_WhiteSmoke_MedianDark, value);}
        }

        SolidColorBrush modeColor_SkyBlue_DimDark;
        public SolidColorBrush ModeColor_SkyBlue_DimDark {
            get { return modeColor_SkyBlue_DimDark; }
            set { SetProperty(ref modeColor_SkyBlue_DimDark, value); }
        }

        SolidColorBrush modeColor_WhiteSmoke_Gray;
        public SolidColorBrush ModeColor_WhiteSmoke_Gray {
            get { return modeColor_WhiteSmoke_Gray; }
            set { SetProperty(ref modeColor_WhiteSmoke_Gray, value); }
        }

        SolidColorBrush modeColor_White_Black;
        public SolidColorBrush ModeColor_White_Black {
            get { return modeColor_White_Black; }
            set { SetProperty(ref modeColor_White_Black, value); }
        }

        SolidColorBrush modeColor_LightSkyBlue_MidnightBlue;
        public SolidColorBrush ModeColor_LightSkyBlue_MidnightBlue {
            get { return modeColor_LightSkyBlue_MidnightBlue; }
            set { SetProperty(ref modeColor_LightSkyBlue_MidnightBlue, value); }
        }

        SolidColorBrush modeColor_LightGray_Gray;
        public SolidColorBrush ModeColor_LightGray_Gray {
            get { return modeColor_LightGray_Gray; }
            set { SetProperty(ref modeColor_LightGray_Gray, value); }
        }

        SolidColorBrush modeColor_SmallGray_LargeGray;
        public SolidColorBrush ModeColor_SmallGray_LargeGray {
            get { return modeColor_SmallGray_LargeGray; }
            set { SetProperty(ref modeColor_SmallGray_LargeGray, value); }
        }

        SolidColorBrush modeColor_LightBlue_DimDark;
        public SolidColorBrush ModeColor_LightBlue_DimDark {
            get { return modeColor_LightBlue_DimDark; }
            set { SetProperty(ref modeColor_LightBlue_DimDark, value); }
        }

        SolidColorBrush modeColor_AliceBlue_MedianDark;
        public SolidColorBrush ModeColor_AliceBlue_MedianDark {
            get { return modeColor_AliceBlue_MedianDark; }
            set { SetProperty(ref modeColor_AliceBlue_MedianDark, value); }
        }

        SolidColorBrush modeColor_White_MedianDark;
        public SolidColorBrush ModeColor_White_MedianDark {
            get { return modeColor_White_MedianDark; }
            set { SetProperty(ref modeColor_White_MedianDark, value); }
        }

        SolidColorBrush modeColor_LightGray_Black;
        public SolidColorBrush ModeColor_LightGray_Black {
            get { return modeColor_LightGray_Black; }
            set { SetProperty(ref modeColor_LightGray_Black, value); }
        }

        SolidColorBrush modeColor_AliceBlue_Dark;
        public SolidColorBrush ModeColor_AliceBlue_Dark {
            get { return modeColor_AliceBlue_Dark; }
            set { SetProperty(ref modeColor_AliceBlue_Dark, value); }
        }

        SolidColorBrush modeColor_Gray_Black;
        public SolidColorBrush ModeColor_Gray_Black {
            get { return modeColor_Gray_Black; }
            set { SetProperty(ref modeColor_Gray_Black, value); }
        }


        // Foreground Color:
        SolidColorBrush modeColor_Black_WhiteSmoke;
        public SolidColorBrush ModeColor_Black_WhiteSmoke {
            get { return modeColor_Black_WhiteSmoke; }
            set { SetProperty(ref modeColor_Black_WhiteSmoke, value); }
        }

        SolidColorBrush modeColor_Black_LightGray;
        public SolidColorBrush ModeColor_Black_LightGray {
            get { return modeColor_Black_LightGray; }
            set { SetProperty(ref modeColor_Black_LightGray, value); }
        }

        SolidColorBrush modeColor_WhiteSmoke_LightGray;
        public SolidColorBrush ModeColor_WhiteSmoke_LightGray {
            get { return modeColor_WhiteSmoke_LightGray; }
            set { SetProperty(ref modeColor_WhiteSmoke_LightGray, value); }
        }

        SolidColorBrush modeColor_White_LightGray;
        public SolidColorBrush ModeColor_White_Gray {
            get { return modeColor_White_LightGray; }
            set { SetProperty(ref modeColor_White_LightGray, value); }
        }

        SolidColorBrush modeColor_Gray_LightGray;
        public SolidColorBrush ModeColor_Gray_LightGray {
            get { return modeColor_Gray_LightGray; }
            set { SetProperty(ref modeColor_Gray_LightGray, value); }
        }

        SolidColorBrush modeColor_Black_White;
        public SolidColorBrush ModeColor_Black_White {
            get { return modeColor_Black_White; }
            set { SetProperty(ref modeColor_Black_White, value); }
        }

        private void init() {
            ModeColor_WhiteSmoke_MedianDark = Brushes.WhiteSmoke;
            ModeColor_WhiteSmoke_Gray = Brushes.WhiteSmoke;
            ModeColor_Black_WhiteSmoke = Brushes.Black;
            ModeColor_White_Black = Brushes.White;
            ModeColor_SkyBlue_DimDark = Brushes.AliceBlue;
            ModeColor_LightSkyBlue_MidnightBlue = Brushes.LightSkyBlue;
            ModeColor_LightGray_Gray = Brushes.LightGray;
            ModeColor_LightGray_Black = Brushes.LightGray;
            ModeColor_AliceBlue_MedianDark = Brushes.AliceBlue;
            ModeColor_Black_LightGray = Brushes.Black;
            ModeColor_White_MedianDark = Brushes.White;
            ModeColor_LightBlue_DimDark = Brushes.LightBlue;
            ModeColor_AliceBlue_Dark = Brushes.AliceBlue;
            ModeColor_WhiteSmoke_LightGray = Brushes.WhiteSmoke;
            ModeColor_SmallGray_LargeGray = new SolidColorBrush(Color.FromRgb(235, 235, 235));
            ModeColor_Gray_Black = new SolidColorBrush(Color.FromRgb(235, 235, 235));
            ModeColor_Gray_LightGray = Brushes.Gray;
            ModeColor_White_Gray = Brushes.White;
            ModeColor_Black_White = Brushes.Black;

        }

        private void changeMode(string mode) {
            if (mode == "Dark Mode") {
                ModeColor_WhiteSmoke_MedianDark = new SolidColorBrush(Color.FromRgb(55,55,55));
                ModeColor_WhiteSmoke_Gray = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                ModeColor_Black_WhiteSmoke = Brushes.WhiteSmoke;
                ModeColor_White_Black = new SolidColorBrush(Color.FromRgb(10, 10, 10));
                ModeColor_SkyBlue_DimDark = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                ModeColor_LightSkyBlue_MidnightBlue = Brushes.MidnightBlue;
                ModeColor_LightGray_Gray = new SolidColorBrush(Color.FromRgb(90, 90, 90));
                ModeColor_LightGray_Black = new SolidColorBrush(Color.FromRgb(20, 20, 20));
                ModeColor_AliceBlue_MedianDark = new SolidColorBrush(Color.FromRgb(55, 55, 55));
                ModeColor_Black_LightGray = Brushes.LightGray;
                ModeColor_White_MedianDark = new SolidColorBrush(Color.FromRgb(65, 65, 65));
                ModeColor_LightBlue_DimDark = new SolidColorBrush(Color.FromRgb(25, 25, 25));
                ModeColor_AliceBlue_Dark = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                ModeColor_WhiteSmoke_LightGray = Brushes.LightGray;
                ModeColor_SmallGray_LargeGray = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                ModeColor_Gray_Black = new SolidColorBrush(Color.FromRgb(35, 35, 35));
                ModeColor_Gray_LightGray = Brushes.LightGray;
                ModeColor_White_Gray = Brushes.Gray;
                ModeColor_Black_White = Brushes.White;
            }

            if (mode == "Light Mode") {
                ModeColor_WhiteSmoke_MedianDark = Brushes.WhiteSmoke;
                ModeColor_WhiteSmoke_Gray = Brushes.WhiteSmoke;
                ModeColor_Black_WhiteSmoke = Brushes.Black;
                ModeColor_White_Black = Brushes.White;
                ModeColor_SkyBlue_DimDark = Brushes.AliceBlue;
                ModeColor_LightSkyBlue_MidnightBlue = Brushes.LightSkyBlue;
                ModeColor_LightGray_Gray = Brushes.LightGray;
                ModeColor_LightGray_Black = Brushes.LightGray;
                ModeColor_AliceBlue_MedianDark = Brushes.AliceBlue;
                ModeColor_Black_LightGray = Brushes.Black;
                ModeColor_White_MedianDark = Brushes.White;
                ModeColor_LightBlue_DimDark = Brushes.LightBlue;
                ModeColor_AliceBlue_Dark = Brushes.AliceBlue;
                ModeColor_WhiteSmoke_LightGray = Brushes.WhiteSmoke;
                ModeColor_SmallGray_LargeGray = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                ModeColor_Gray_Black = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                ModeColor_Gray_LightGray = Brushes.Gray;
                ModeColor_White_Gray = Brushes.White;
                ModeColor_Black_White = Brushes.Black;
            }
        }


    }

    // Constants:

    public class Constant {
        public string PRESET_FILE = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/RemoteCameraControllerData/Presetting.xml";
        public string PROGRAM_FILE = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/RemoteCameraControllerData/Programs.xml";
        public string CAMERANAME_FILE = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/RemoteCameraControllerData/CameraName.xml";
        public string RUNTIME_FILE = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/RemoteCameraControllerData/Runtime/camera_rt.exe";
        public const char DEF_REQ_SCAN = '1';
        public const int CAMLIST_AREA_VISIBLE_HEIGHT = 155;
        public const int CAMLIST_AREA_HIDDEN_HEIGHT = 1;

        public const int UNDO_BUFFER_SIZE = 180 * 3 * 180 * 3 * 16 * 3;
    }

    // Events
    public class CameraDiscoverEvent : PubSubEvent<string> {};
    public class CameraSelectEvent : PubSubEvent<CameraInfo> {}
    public class CameraOutPutEvent : PubSubEvent<CameraInfo> {}
    public class CameraDiscoveredEvent : PubSubEvent<List<CameraInfo>> {}
    public class ClearCameraPreviewEvent: PubSubEvent<CameraInfo> {}
    public class ClearCameraOutputEvent : PubSubEvent<CameraInfo> { }

    public class ProgramSaveEvent : PubSubEvent<int> {}
    public class ProgramCancelEvent : PubSubEvent<int> {}
    public class ProgramDeleteEvent : PubSubEvent<int> {}
    public class ProgramRunEvent : PubSubEvent<int?> {}

    public class PresettingDeleteEvent : PubSubEvent<int> {}
    public class SetPresetEvent : PubSubEvent<PresetParams> {}
    public class SaveSettingEvent : PubSubEvent<PresetParams> {}
    public class SaveSettingAsNewEvent: PubSubEvent<PresetParams> {}

    public class MenuBarToPreviewEvent : PubSubEvent<string> {}

    public class PreviewPauseEvent : PubSubEvent<bool> {}
    public class PreviewResumeEvent : PubSubEvent<bool> {}

    public class ChangeModeEvent : PubSubEvent<string> {}
    public class ChangeModeShortCutEvent : PubSubEvent<ModeColors> {}
    public class RelaunchRuntimeEvent: PubSubEvent<string> {}
    public class StatusUpdateEvent: PubSubEvent<string> {}
    public class ShowCameraInfoShortCutEvent: PubSubEvent<string> {}

    public class DiscoveryResponseReceivedEvent : PubSubEvent<discovery_response_t> {}
    public class StreamUriResponseReceivedEvent : PubSubEvent<stream_uri_response_t> {}
    public class PositionResponseReceivedEvent : PubSubEvent<position_response_t> {}
    public class InitSessionResponseReceivedEvent : PubSubEvent<init_session_response_t> {}
    public class EndSessionResponseReceivedEvent : PubSubEvent<end_session_response_t> {}
    public class UpdateOutputCameraReceivedEvent: PubSubEvent<output_request_t> {}
    public class ProgramStartResponseEvent : PubSubEvent<start_program_response_t> {}
    public class ProgramStopResponseEvent : PubSubEvent<stop_program_response_t> {}
    public class ProgramEndMessageReceivedEvent: PubSubEvent<end_program_message_t> {}
    public class ProgramStatusMessageReceivedEvent : PubSubEvent<program_status_message_t> {}
}
