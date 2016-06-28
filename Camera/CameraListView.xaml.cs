
using System.Windows.Controls;

namespace Camera {
    /// <summary>
    /// Interaction logic for CameraListView.xaml
    /// </summary>
    public partial class CameraListView : UserControl {
        public CameraListView(CameraListVM model) {
            InitializeComponent();
            DataContext = model;
            CamScrollableList.ItemsSource = ((CameraListVM)DataContext).CamList;
        }
    }
}
