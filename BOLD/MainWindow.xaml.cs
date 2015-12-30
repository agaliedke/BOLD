using Microsoft.Win32;
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

namespace BOLD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtNum.Text = _numValue.ToString();
        }
        // Menu items controlers
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                image.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            _images.Add(image.Source);
            NumValue++;
        }
        // List of images
        private List<ImageSource> _images = new List<ImageSource>();
        // controlers for image changer
        private int _numValue = 0;

        public int NumValue
        {
            get { return _numValue; }
            set
            {
                if (value < 0 && _images.Count==0)
                    _numValue = 0;
                else if (value < 1 && _images.Count>0)
                    _numValue = 1;
                else if (value > _images.Count)
                    _numValue = _images.Count;
                else
                    _numValue = value;
                txtNum.Text = _numValue.ToString();
                if (_images.Count > 0)
                {
                    image.Source = _images[_numValue - 1];
                   
                }
            }
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            NumValue++;
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            NumValue--;
        }

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNum == null)
            {
                return;
            }

            if (!int.TryParse(txtNum.Text, out _numValue))
            {
                if (_numValue < 0)
                    _numValue = 0;
                else if (_numValue > _images.Count)
                    _numValue = _images.Count;
                txtNum.Text = _numValue.ToString();
            }
        }
    }

}
