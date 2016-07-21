
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Preview {
    /// <summary>
    /// Interaction logic for ScaleSlider.xaml
    /// </summary>
    public partial class ScaleSlider: UserControl {

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(double), typeof(ScaleSlider), new PropertyMetadata(OnPositionPropertyChange));
        public double Position {
            get { return (double)GetValue(PositionProperty);}
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register("LabelRotation", typeof(double), typeof(ScaleSlider), new PropertyMetadata(OnTransformPropertyChange));
        public double LabelRotation {
            get { return (double)GetValue(RotationProperty); }
            set { SetValue(RotationProperty, value); }
        }

        public static readonly DependencyProperty LabelScaleXTransformProperty = DependencyProperty.Register("LabelScaleXTransform", typeof(double), typeof(ScaleSlider), new PropertyMetadata(OnTransformPropertyChange));
        public double LabelScaleXTransform {
            get { return (double)GetValue(LabelScaleXTransformProperty); }
            set { SetValue(LabelScaleXTransformProperty, value); }
        }


        public static readonly DependencyProperty LabelScaleYTransformProperty = DependencyProperty.Register("LabelScaleYTransform", typeof(double), typeof(ScaleSlider), new PropertyMetadata(OnTransformPropertyChange));
        public double LabelScaleYTransform {
            get { return (double)GetValue(LabelScaleYTransformProperty); }
            set { SetValue(LabelScaleYTransformProperty, value); }
        }

        private static void OnPositionPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ScaleSlider obj = d as ScaleSlider;
            double offset = (obj.Position - obj.Min) / (obj.Max - obj.Min) * (obj.Width - obj.ArrowWidth);
            obj.Arrow.Margin = new Thickness(offset, 0, 0, 0);
            obj.PositionLabel.Margin = new Thickness(offset-3, 8, 0, 0);
            obj.PositionLabel.Content = obj.Position;
        }

        private static void OnTransformPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ScaleSlider obj = d as ScaleSlider;
            RotateTransform myRotateTransform = new RotateTransform();
            ScaleTransform myScaleTransform = new ScaleTransform();
            myScaleTransform.ScaleX = obj.LabelScaleXTransform;
            myScaleTransform.ScaleY = obj.LabelScaleYTransform;
            myScaleTransform.CenterY = 1;
            myRotateTransform.Angle = obj.LabelRotation;
            TransformGroup myTransformGroup = new TransformGroup();
            myTransformGroup.Children.Add(myRotateTransform);
            myTransformGroup.Children.Add(myScaleTransform);
            obj.PositionLabel.RenderTransform = myTransformGroup;
            obj.PositionLabel.RenderTransformOrigin = new Point(0.45, 0.65);
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
