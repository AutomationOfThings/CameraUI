using System.Windows;

namespace Camera {
    /// <summary>
    /// Interaction logic for CameraLoginForm.xaml
    /// </summary>
    public partial class CameraLoginForm : Window {

        CameraVM model;

        public CameraLoginForm(CameraVM model) {
            InitializeComponent();
            this.model = model;
            DataContext = model;
            ConnectionStatus.Visibility = Visibility.Hidden;
        }

        private void signIn(object sender, RoutedEventArgs e) {
            model.CamInfo.UserName = UsernameBox.Text;
            model.CamInfo.Password = PasswordBox.Password;
            ConnectionStatus.Visibility = Visibility.Visible;
            SignIn.Visibility = Visibility.Hidden;
            this.model.connect();
        }

        public void activate() {
            ConnectionStatus.Visibility = Visibility.Hidden;
            SignIn.Visibility = Visibility.Visible;
        }
    }
}
