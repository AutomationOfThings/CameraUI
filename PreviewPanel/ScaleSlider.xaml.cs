using System;
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

namespace PreviewPanel {
    /// <summary>
    /// Interaction logic for ScaleSlider.xaml
    /// </summary>
    public partial class ScaleSlider: UserControl {

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(double), typeof(ScaleSlider), new PropertyMetadata(OnPropertyChange));
        public double Position {
            get { return (double)GetValue(PositionProperty);}
            set { SetValue(PositionProperty, value); }
        }

        private static void OnPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ScaleSlider obj = d as ScaleSlider;
            double offset = (obj.Position - obj.Min) / (obj.Max - obj.Min) * (obj.Width - obj.ArrowWidth);
            obj.Arrow.Margin = new Thickness(offset, 0, 0, 0);
        }

        public double Min { get; set; }
        public double Max { get; set; }

        public double ArrowWidth { get; set; }
        public ScaleSlider() {
            InitializeComponent();
            Container.DataContext = this;
            ArrowWidth = 10;
        }
    }
}
