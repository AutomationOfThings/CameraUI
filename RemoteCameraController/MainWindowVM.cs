
using Camera;
using MenuBar;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using NotificationCenter;
using Output;
using Presetting;
using PreviewPanel;
using Program;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using Util;
using XMLParser;

namespace RemoteCameraController {
    public class MainWindowVM : BindableBase {

        protected EventAggregator notificationCenter = Notification.Instance;

        public ModeColors modeColors { get; set; }

        List<string> usedCamList = new List<string>();

        List<CameraInfo> camInfoList = new List<CameraInfo>();
        List<PresetParams> presetList;
        ObservableCollection<ProgramInfo> programList;
        ObservableCollection<CameraNameWrapper> cameraNameList;
        Dictionary<string, string> CameraName2IP;
        Dictionary<string, string> IP2CameraName;

        public ProgramVM ProgramVM { get; set; }
        public CameraListVM CamListVM { get; set; }
        public PreviewVM PreviewVM { get; set; }
        public OutputVM OutputVM { get; set; }
        public PresettingVM PresetVM { get; set; }
        public MenuVM MenuBarVM { get; set; }
        public StatusBarVM StatusBarVM { get; set; }
        public ProgramRunBarVM ProgramRunBarVM { get; set; }
        private CameraExplorer cameraExplorer;
        
        public MainWindowVM () {
            //modeColors = new ModeColors(notificationCenter);
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                modeColors = ModeColors.Singleton(notificationCenter);
                loadXML(Constant.PRESET_FILE, Constant.PROGRAM_FILE, Constant.CAMERANAME_FILE);
                setupViewModels();
            }
        }

        public void loadXML(string presettingFile, string programFile, string cameraNameFile) {
            presetList = new PresettingParser(presettingFile).parse(usedCamList);
            programList = new ProgramParser(programFile).parse();
            CameraName2IP = new Dictionary<string, string>();
            IP2CameraName = new Dictionary<string, string>();
            cameraNameList = new ObservableCollection<CameraNameWrapper>();
            (new CameraNameParser(cameraNameFile)).parse(CameraName2IP, IP2CameraName, cameraNameList);
        }

        private void setupViewModels() {
            // initialize Cameras
            // string ip1 = "192.168.0.148";
            // string ip2 = "192.168.0.119";
            // string URL1 = "http://192.168.1.211/stw-cgi/video.cgi?msubmenu=stream&action=view&Profile=1&CodecType=MJPEG";
            // string URL2 = "http://192.168.0.119/stw-cgi/video.cgi?msubmenu=stream&action=view&Profile=4&CodecType=MJPEG&Resolution=800x600&FrameRate=30&CompressionLevel=10";

            camInfoList = new List<CameraInfo>();

            // initialize camera list view
            CamListVM = new CameraListVM(camInfoList, cameraNameList);
            cameraExplorer = new CameraExplorer(camInfoList, IP2CameraName, cameraNameList);
            // initialize preview View
            PreviewVM = new PreviewVM();

            // initialize output view

            OutputVM = new OutputVM();

            // set up bottom right area: presetting
            PresetVM = new PresettingVM(presetList, camInfoList, usedCamList, cameraNameList);

            // set up bottom right area: program
            ProgramVM = new ProgramVM(programList, camInfoList, cameraNameList, presetList);
            ProgramRunBarVM = new ProgramRunBarVM(programList);

            // set up menu bar
            MenuBarVM = new MenuVM(camInfoList);

            // set up status bar
            StatusBarVM = new StatusBarVM();

            // change to dark mode when program starts up
            changeModeShortCut(modeColors);

        }

        public void saveCameras() {
            (new CameraNameWriter(Constant.CAMERANAME_FILE)).writeToDisk(cameraNameList);
        }

        public void endCameraSessions() {
            bool canExit = true;
            int i = 0;
            while (i < 6) {
                foreach (CameraInfo cam in camInfoList) {
                    if (cam.UserName != null) {
                        cam.endSession();
                        canExit = false;
                    } 
                }
                if (canExit)
                    return;
                Thread.Sleep(500);
                i ++;                    
            }
        }

        private void changeModeShortCut(ModeColors modeColors) {
            notificationCenter.GetEvent<ChangeModeShortCutEvent>().Publish(modeColors);
        }

        private void discoverShortCut(string discovery) {
            notificationCenter.GetEvent<CameraDiscoverShortCutEvent>().Publish("discover");
        }

        ICommand modeCommand;
        public ICommand ModeCommand {
            get {
                if (modeCommand == null) { modeCommand = new DelegateCommand<ModeColors>(changeModeShortCut); }
                return modeCommand;
            }
        }

        ICommand discoverCommand;
        public ICommand DiscoverCommand {
            get {
                if (discoverCommand == null) { discoverCommand = new DelegateCommand<string>(discoverShortCut); }
                return discoverCommand;
            }
        }

    }
}
