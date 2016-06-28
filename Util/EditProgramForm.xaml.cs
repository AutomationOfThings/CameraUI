using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Util {

    public partial class EditProgramForm : Window {

        public EditProgramForm(EditProgramVM model) {
            InitializeComponent();
            DataContext = model;
        }

    }

}
