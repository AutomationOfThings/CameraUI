
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
using System.Windows.Input;
using Util;
using XMLParser;

namespace RemoteCameraController {
    public class MainWindowVM : BindableBase {

        protected EventAggregator notificationCenter = Notification.Instance;

        public ModeColors modeColors { get; set; }

        List<CameraInfo> camInfoList = new List<CameraInfo>();
        // ObservableCollection<CameraVM> camList = new ObservableCollection<CameraVM>();
        List<PresetParams> presetList;
        List<List<CameraCommand>> programList;

        public ProgramVM ProgramVM { get; set; }
        public CameraListVM CamListVM { get; set; }
        public PreviewVM PreviewVM { get; set; }
        public OutputVM OutputVM { get; set; }
        public PresettingVM PresetVM { get; set; }
        public MenuVM MenuBarVM { get; set; }
        public StatusBarVM StatusBarVM { get; set; }

        public MainWindowVM () {
            modeColors = new ModeColors(notificationCenter);
            loadXML(Constant.PRESET_FILE, Constant.PROGRAM_FILE);
            setupViewModels();
        }

        public void loadXML(string presettingFile, string programFile) {
            presetList = new PresettingParser(presettingFile).parse();
            programList = new ProgramParser(programFile).parse();
        }

        private void setupViewModels() {
            // initialize Cameras
            string ip1 = "192.168.0.148";
            string ip2 = "192.168.0.119";
            string URL1 = "http://192.168.0.148/stw-cgi/video.cgi?msubmenu=stream&action=view&Profile=1&CodecType=MJPEG&Resolution=800x600&FrameRate=30&CompressionLevel=10";
            string URL2 = "http://192.168.0.119/stw-cgi/video.cgi?msubmenu=stream&action=view&Profile=4&CodecType=MJPEG&Resolution=800x600&FrameRate=30&CompressionLevel=10";

            camInfoList = new List<CameraInfo>();

            for (int i = 111; i < 999; i += 111) {
                CameraInfo cam = new CameraInfo("ip1", i.ToString(), 0, 0, 1);
                cam.VideoURL = URL1;
                cam.IP = ip1;
                camInfoList.Add(cam);
            }

            // initialize camera list view
            CamListVM = new CameraListVM(camInfoList, modeColors, notificationCenter);
            CamListVM.modeColors = modeColors;

            // initialize preview View
            PreviewVM = new PreviewVM(notificationCenter);
            PreviewVM.modeColors = modeColors;

            // initialize output view

            OutputVM = new OutputVM(notificationCenter);
            OutputVM.modeColors = modeColors;

            // set up bottom right area: presetting
            PresetVM = new PresettingVM(presetList, camInfoList, notificationCenter);
            PresetVM.modeColors = modeColors;

            // set up bottom right area: program
            ProgramVM = new ProgramVM(programList, camInfoList, presetList, notificationCenter);
            ProgramVM.modeColors = modeColors;

            // set up menu bar
            MenuBarVM = new MenuVM(camInfoList, notificationCenter);
            MenuBarVM.modeColors = modeColors;

            // set up status bar
            StatusBarVM = new StatusBarVM(modeColors, notificationCenter);

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
                if (modeCommand == null) {
                    modeCommand = new DelegateCommand<ModeColors>(
                        changeModeShortCut);
                }
                return modeCommand;
            }
        }

        ICommand discoverCommand;
        public ICommand DiscoverCommand {
            get {
                if (discoverCommand == null) {
                    discoverCommand = new DelegateCommand<string>(
                        discoverShortCut);
                }
                return discoverCommand;
            }
        }

    }
}
