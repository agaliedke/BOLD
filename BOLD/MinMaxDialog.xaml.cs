using System;
using System.Windows;

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
