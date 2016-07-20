﻿
using Camera;
using MenuBar;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using NotificationCenter;
using Output;
using Preset;
using Preview;
using Program;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using Util;
using XMLParser;

namespace RemoteCameraController {
    public class MainWindowVM : BindableBase {

        protected EventAggregator notificationCenter = Notification.Instance;

        public ModeColors modeColors { get; set; }

        Process runtime;

        Dictionary<string, CameraInfo> IP2CameraInfo;
        List<CameraInfo> camInfoList = new List<CameraInfo>();
        List<PresetParams> presetList;
        ObservableCollection<ProgramInfo> programList;
        ObservableCollection<CameraNameWrapper> cameraNameList;
        Dictionary<string, string> CameraName2IP;
        Dictionary<string, string> IP2CameraName;
        Dictionary<string, PresetParams> PresetName2Preset;

        public ProgramVM ProgramVM { get; set; }
        public CameraListVM CamListVM { get; set; }
        public PreviewVM PreviewVM { get; set; }
        public OutputVM OutputVM { get; set; }
        public PresetVM PresetVM { get; set; }
        public MenuVM MenuBarVM { get; set; }
        public StatusBarVM StatusBarVM { get; set; }
        public ProgramRunBarVM ProgramRunBarVM { get; set; }
        private CameraExplorer cameraExplorer;
        
        public MainWindowVM () {
            //modeColors = new ModeColors(notificationCenter);
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                modeColors = ModeColors.Singleton(notificationCenter);
                loadXML(AppDomain.CurrentDomain.BaseDirectory + Constant.PRESET_FILE,
                    AppDomain.CurrentDomain.BaseDirectory + Constant.PROGRAM_FILE,
                    AppDomain.CurrentDomain.BaseDirectory + Constant.CAMERANAME_FILE);
                setupViewModels();
            }
        }

        public void loadXML(string presettingFile, string programFile, string cameraNameFile) {
            PresetName2Preset = new Dictionary<string, PresetParams>();
            presetList = new PresettingParser(presettingFile).parse(PresetName2Preset);
            programList = new ProgramParser(programFile).parse();
            CameraName2IP = new Dictionary<string, string>();
            IP2CameraName = new Dictionary<string, string>();
            cameraNameList = new ObservableCollection<CameraNameWrapper>();
            (new CameraNameParser(cameraNameFile)).parse(CameraName2IP, IP2CameraName, cameraNameList);
        }

        private void setupViewModels() {

            setupRuntime();

            camInfoList = new List<CameraInfo>();
            IP2CameraInfo = new Dictionary<string, CameraInfo>();
            // initialize camera list view
            CamListVM = new CameraListVM(camInfoList, cameraNameList);
            cameraExplorer = new CameraExplorer(camInfoList, IP2CameraInfo, IP2CameraName, cameraNameList);

            // initialize preview View
            PreviewVM = new PreviewVM();

            // initialize output view

            OutputVM = new OutputVM(IP2CameraInfo);

            // set up bottom right area: presetting
            PresetVM = new PresetVM(presetList, camInfoList, PresetName2Preset, cameraNameList);

            // set up bottom right area: program
            ProgramVM = new ProgramVM(programList, camInfoList, cameraNameList, presetList);
            ProgramRunBarVM = new ProgramRunBarVM(programList, PresetName2Preset, CameraName2IP);

            // set up menu bar
            MenuBarVM = new MenuVM(camInfoList, runtime, cameraNameList);

            // set up status bar
            StatusBarVM = new StatusBarVM();

            // change to dark mode when program starts up
            changeModeShortCut(modeColors);

        }

        public void saveCameras() {
            (new CameraNameParser(Constant.CAMERANAME_FILE)).write(cameraNameList);
        }

        private void setupRuntime() {
            Process[] processes = Process.GetProcessesByName("camera_rt");
            foreach (Process p in processes)
                p.Kill();

            try {
                runtime = new Process();
                runtime.StartInfo.FileName = Constant.RUNTIME_FILE;
                // runtime.StartInfo.CreateNoWindow = true;
                runtime.StartInfo.UseShellExecute = false;
                runtime.StartInfo.WorkingDirectory = Path.GetDirectoryName(Constant.RUNTIME_FILE);
            } catch (Exception) {
                MessageBox.Show("Runtime file is not found.", "Attention", MessageBoxButtons.OK);
            }

        }

        public void startRunTime() {
            try {
                runtime.Start();
            } catch (Exception ex) {
                Console.WriteLine("An error occurred in starting runtime!!!: " + ex.Message);
                MessageBox.Show("Meet an error in launching the runtime.", "Attention", MessageBoxButtons.OK);
                return;
            }
        }

        public void stopRunTime() {
            try {
                runtime.Kill();
                runtime.WaitForExit();
            } catch (Exception ex) {
                Console.WriteLine("An error occurred in killing runtime!!!: " + ex.Message);
                return;
            }
        }

        public void endCameraSessions() {
            bool canExit = true;
            int i = 0;
            while (i < 3) {
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

        private void relaunchRuntimeShortCut(string relauch) {
            notificationCenter.GetEvent<RelaunchRuntimeShortCutEvent>().Publish("relaunchRuntime");
        }

        ICommand relaunchRuntimeCommand;
        public ICommand RelaunchRuntimeCommand {
            get {
                if (relaunchRuntimeCommand == null) { relaunchRuntimeCommand = new DelegateCommand<string>(relaunchRuntimeShortCut); }
                return relaunchRuntimeCommand;
            }
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
