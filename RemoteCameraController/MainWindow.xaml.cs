
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Util;

namespace RemoteCameraController {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private Storyboard camListHideStoryboard;
        private Storyboard camListShowStoryboard;
        private Storyboard mainAreaShowStoryboard;
        private Storyboard mainAreaHideStoryboard;

        public MainWindow() {

            InitializeComponent();
            MainArea.Margin = new Thickness(0, 30, 0, Constant.CAMLIST_AREA_VISIBLE_HEIGHT + 20);

            checkNetwork();
            DataContext = new MainWindowVM();
            ((MainWindowVM)DataContext).startRunTime();
            setUpAnimations();
        }

        private void setUpAnimations() {
            DoubleAnimation CamListHide = new DoubleAnimation();
            CamListHide.To = Constant.CAMLIST_AREA_HIDDEN_HEIGHT;
            CamListHide.From = Constant.CAMLIST_AREA_VISIBLE_HEIGHT;
            CamListHide.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            CamListHide.AutoReverse = false;

            DoubleAnimation CamListShow = new DoubleAnimation();
            CamListShow.From = Constant.CAMLIST_AREA_HIDDEN_HEIGHT;
            CamListShow.To = Constant.CAMLIST_AREA_VISIBLE_HEIGHT;
            CamListShow.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            CamListShow.AutoReverse = false;

            ThicknessAnimation MainAreaShow = new ThicknessAnimation();
            MainAreaShow.From = new Thickness(0, 30, 0, Constant.CAMLIST_AREA_HIDDEN_HEIGHT + 25);
            MainAreaShow.To = new Thickness(0, 30, 0, Constant.CAMLIST_AREA_VISIBLE_HEIGHT + 25);
            MainAreaShow.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            MainAreaShow.AutoReverse = false;

            ThicknessAnimation MainAreaHide = new ThicknessAnimation();
            MainAreaHide.To = new Thickness(0, 30, 0, Constant.CAMLIST_AREA_HIDDEN_HEIGHT + 25);
            MainAreaHide.From = new Thickness(0, 30, 0, Constant.CAMLIST_AREA_VISIBLE_HEIGHT + 25);
            MainAreaHide.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            MainAreaHide.AutoReverse = false;

            camListHideStoryboard = new Storyboard();
            camListHideStoryboard.Children.Add(CamListHide);
            Storyboard.SetTargetName(CamListHide, CamAreaList.Name);
            Storyboard.SetTargetProperty(CamListHide, new PropertyPath(HeightProperty));

            camListShowStoryboard = new Storyboard();
            camListShowStoryboard.Children.Add(CamListShow);
            Storyboard.SetTargetName(CamListShow, CamAreaList.Name);
            Storyboard.SetTargetProperty(CamListShow, new PropertyPath(HeightProperty));

            mainAreaShowStoryboard = new Storyboard();
            mainAreaShowStoryboard.Children.Add(MainAreaShow);
            Storyboard.SetTargetName(MainAreaShow, MainArea.Name);
            Storyboard.SetTargetProperty(MainAreaShow, new PropertyPath(MarginProperty));

            mainAreaHideStoryboard = new Storyboard();
            mainAreaHideStoryboard.Children.Add(MainAreaHide);
            Storyboard.SetTargetName(MainAreaHide, MainArea.Name);
            Storyboard.SetTargetProperty(MainAreaHide, new PropertyPath(MarginProperty));
        }

        private void checkNetwork() {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                System.Windows.Forms.DialogResult dialog = System.Windows.Forms.MessageBox.Show("Please check your network connection and then relaunch application.", "Attention", System.Windows.Forms.MessageBoxButtons.OK);
                Application.Current.Shutdown();
            }
        }


        private void closeForm(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                return;
            }
            System.Windows.Forms.DialogResult dialog = System.Windows.Forms.MessageBox.Show("Are you sure you want to exit the application?", "Exit", System.Windows.Forms.MessageBoxButtons.YesNo);
            ((MainWindowVM)DataContext).saveCameras();
            if (dialog == System.Windows.Forms.DialogResult.No) {
                e.Cancel = true;
                return;
            }
            ((MainWindowVM) DataContext).endCameraSessions();
            ((MainWindowVM)DataContext).stopRunTime();
        }

        private void changeCamListVisibility(object sender, RoutedEventArgs e) {
            if (CamAreaList.Height > Constant.CAMLIST_AREA_HIDDEN_HEIGHT) {
                
                //CamAreaList.Height = Constant.CAMLIST_AREA_HIDDEN_HEIGHT;
                camListHideStoryboard.Begin(this);
                // MainArea.Margin = new Thickness(0,30,0, Constant.CAMLIST_AREA_HIDDEN_HEIGHT + 25);
                mainAreaHideStoryboard.Begin(this);
            } else {
                //CamAreaList.Height = Constant.CAMLIST_AREA_VISIBLE_HEIGHT;
                camListShowStoryboard.Begin(this);
                //MainArea.Margin = new Thickness(0,30,0, Constant.CAMLIST_AREA_VISIBLE_HEIGHT+25);
                mainAreaShowStoryboard.Begin(this);
            }
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }


    }
}
