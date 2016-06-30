
using System.Windows;
using Util;

namespace RemoteCameraController {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {

            InitializeComponent();
            MainArea.Margin = new Thickness(0, 30, 0, Constant.CAMLIST_AREA_VISIBLE_HEIGHT + 20);

            checkNetwork();
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                DataContext = new MainWindowVM();
            }
            
        }

        private void checkNetwork() {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                System.Windows.Forms.DialogResult dialog = System.Windows.Forms.MessageBox.Show("Please check your network connection and then relaunch application.", "Alert", System.Windows.Forms.MessageBoxButtons.OK);
                Application.Current.Shutdown();
            }
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
                MainArea.Margin = new Thickness(0,30,0, Constant.CAMLIST_AREA_HIDDEN_HEIGHT + 25);
            } else {
                CamAreaList.Height = Constant.CAMLIST_AREA_VISIBLE_HEIGHT;
                MainArea.Margin = new Thickness(0,30,0, Constant.CAMLIST_AREA_VISIBLE_HEIGHT+25);
            }
        }
    }
}
