using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NotificationCenter;
using Microsoft.Practices.Prism.PubSubEvents;
using PreviewPanel;
using Output;
using XMLParser;
using Util;
using Presetting;
using Program;
using System.Collections.Specialized;
using System.Diagnostics;
using MenuBar;
using System.Collections.ObjectModel;
using Camera;

namespace RemoteCameraController {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        EventAggregator notificationCenter = Notification.Instance;
        PreviewPanel.PreviewPanel previewPanel;

        List<CameraInfo> camInfoList;
        // ObservableCollection<CameraVM> camList = new ObservableCollection<CameraVM>();
        List<PresetParams> presetList;
        List<List<CameraCommand>> programList;

        ModeColors modeColors;

        public MainWindow() {
            modeColors = new ModeColors(notificationCenter);
            MainWindowVM model = new MainWindowVM(notificationCenter);
            model.modeColors = modeColors;
            this.DataContext = model;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            loadXML(Constant.PRESET_FILE, Constant.PROGRAM_FILE);
            checkNetwork();
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                setUpWindow();
            }

        }

        private void checkNetwork() {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                System.Windows.Forms.DialogResult dialog = System.Windows.Forms.MessageBox.Show("Please check your network connection and then relaunch application.", "Alert", System.Windows.Forms.MessageBoxButtons.OK);
                Application.Current.Shutdown();
            }
        }

        public void setUpWindow() {

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
            CameraListVM camListVM = new CameraListVM(camInfoList, modeColors, notificationCenter);
            CameraListView camListView = new CameraListView(camListVM);
            CamList.Children.Add(camListView);

            // initialize preview View
            previewPanel = new PreviewPanel.PreviewPanel(notificationCenter);
            previewPanel.modeColors = modeColors;
            previewView preview = new previewView(previewPanel);
            previewContainer.Children.Add(preview);

            // initialize output view
            
            OutputVM output = new OutputVM(notificationCenter);
            output.modeColors = modeColors;
            OutputView outputView = new OutputView(output);
            OutputContainer.Children.Add(outputView);

            // set up bottom right area: presetting
            PresettingVM presetting = new PresettingVM(presetList, camInfoList, notificationCenter);
            presetting.modeColors = modeColors;
            PresettingView presettingView = new PresettingView(presetting);
            PresettingContainer.Children.Add(presettingView);

            // set up bottom right area: program
            ProgramVM program = new ProgramVM(programList, camInfoList, presetList, notificationCenter);
            program.modeColors = modeColors;
            ProgramView programView = new ProgramView(program);
            ProgramContainer.Children.Add(programView);

            // set up menu bar
            MenuVM mVM = new MenuVM(camInfoList,notificationCenter);
            mVM.modeColors = modeColors;
            MenuView view = new MenuView(mVM);
            Menu.Children.Add(view);

            // set up status bar
            StatusBarVM sVM = new StatusBarVM(modeColors, notificationCenter);
            StatusBarView sV = new StatusBarView(sVM);
            StatusBar.Children.Add(sV);

            MainArea.Margin = new Thickness(0, 30, 0, Constant.CAMLIST_AREA_VISIBLE_HEIGHT + 20);
            
        }

        public void loadXML(string presettingFile, string programFile) {
            presetList = new PresettingParser(presettingFile).parse();
            programList = new ProgramParser(programFile).parse();
        }

        private void closeForm(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                return;
            }
            System.Windows.Forms.DialogResult dialog = System.Windows.Forms.MessageBox.Show("Are you sure you want to exit the application?", "Exit", System.Windows.Forms.MessageBoxButtons.YesNo);
            if (dialog == System.Windows.Forms.DialogResult.No) {
                e.Cancel = true;
            }
        }

        private void changeCamListVisibility(object sender, RoutedEventArgs e) {
            if (CamAreaList.Height > Constant.CAMLIST_AREA_HIDDEN_HEIGHT) {
                CamAreaList.Height = Constant.CAMLIST_AREA_HIDDEN_HEIGHT;
                MainArea.Margin = new Thickness(0,30,0,25);
            } else {
                CamAreaList.Height = Constant.CAMLIST_AREA_VISIBLE_HEIGHT;
                MainArea.Margin = new Thickness(0,30,0, Constant.CAMLIST_AREA_VISIBLE_HEIGHT+25);
            }
        }
    }
}
