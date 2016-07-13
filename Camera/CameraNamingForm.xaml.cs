
using System.Windows;


namespace Camera {
    /// <summary>
    /// Interaction logic for CameraNamingForm.xaml
    /// </summary>
    public partial class CameraNamingForm : Window {
        public CameraNamingForm(CameraNamingVM model) {
            InitializeComponent();
            DataContext = model;
        }
    }
}
