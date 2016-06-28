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

namespace PreviewPanel
{
    /// <summary>
    /// Interaction logic for previewView.xaml
    /// </summary>
    public partial class previewView : UserControl
    {
        public previewView(PreviewPanel model)
        {
            InitializeComponent();
            this.DataContext = model;
        }
    }
}
