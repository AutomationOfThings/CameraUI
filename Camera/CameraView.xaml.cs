using System.Windows.Controls;


namespace Camera {
    /// <summary>
    /// Interaction logic for CameraView.xaml
    /// </summary>
    public partial
        class CameraView : UserControl {

        public CameraView(CameraVM model) {
            InitializeComponent();
            this.DataContext = model;
        }

    }

}
