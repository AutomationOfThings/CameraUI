
using System.Windows;

namespace Util {
    /// <summary>
    /// Interaction logic for CameraInfoForm.xaml
    /// </summary>
    public partial class CameraInfoForm : Window {
        public CameraInfoForm(CameraInfoVM model ) {
            InitializeComponent();
            DataContext = model;
        }
    }
}
