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
using System.Windows.Shapes;

namespace Exchanger1C.Statements
{
    public partial class ExcelProgressWindow : Window
    {
        public ExcelProgressWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int value, string txt) {
            TxtBlock.Text = txt;
            undefinedProgressBar.Value = value;
            InvalidateVisual();
            UpdateLayout();
        }
    }
}
