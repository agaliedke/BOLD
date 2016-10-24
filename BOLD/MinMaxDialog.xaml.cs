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

namespace BOLD
{
    /// <summary>
    /// Interaction logic for MinMaxDialog.xaml
    /// </summary>
    public partial class MinMaxDialog : Window
    {
        public MinMaxDialog(string sMin="0", string sMax="0")
        {
            InitializeComponent();
            MinScale = sMin;
            MaxScale = sMax;

        }
        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            minScale.SelectAll();
            minScale.Focus();
        }
        public string MinScale
        {
            get { return minScale.Text; }
            set { minScale.Text = value;  }
        }
        public string MaxScale
        {
            get { return maxScale.Text; }
            set { maxScale.Text = value; }
        }
    }
}
