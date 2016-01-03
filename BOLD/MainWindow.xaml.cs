using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

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
            txtNum.Text = _numSlice.ToString();
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
            {
                ImageSlice slice = new ImageSlice(openFileDialog.FileName);
                _numSlice = 1;
                image.Source = slice.GetImage(_numSlice - 1);
                txtNum.Text = _numSlice.ToString();

                //_images.Add(image.Source);
                _image_data.Add(slice);

                ComboboxItem item = new ComboboxItem();
                item.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                item.Value = _numImage++;
                fileNameBox.Items.Add(item);
                fileNameBox.SelectedIndex = _numImage - 1;

            }

        }
        // List of images
        private List<ImageSlice> _image_data = new List<ImageSlice>();
        // controlers for image slice changer
        private int _numSlice = 0;
        private int _numImage = 0;
        public int NumValue
        {
            get { return _numSlice; }
            set
            {
                if (_image_data.Count == 0)
                    return;
                if (value < 1 )
                    _numSlice = 1;
                else if (value > _image_data[fileNameBox.SelectedIndex].zSize)
                    _numSlice = _image_data[fileNameBox.SelectedIndex].zSize;
                else
                    _numSlice = value;
                txtNum.Text = _numSlice.ToString();
                if (_image_data.Count > 0)
                {
                    image.Source = _image_data[fileNameBox.SelectedIndex].GetImage(_numSlice-1);
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

            if (int.TryParse(txtNum.Text, out _numSlice))
            {
                NumValue = _numSlice;
            }


        }

        private void fileName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_numSlice > _image_data[fileNameBox.SelectedIndex].zSize)
                NumValue = _numSlice = _image_data[fileNameBox.SelectedIndex].zSize;
            else
                image.Source = _image_data[fileNameBox.SelectedIndex].GetImage(_numSlice - 1);

        }

        private void resize_Checked(object sender, RoutedEventArgs e)
        {
            image.Source = new CroppedBitmap(image.Source as BitmapSource, _image_data[fileNameBox.SelectedIndex].selection);
        }
        private void resize_UnChecked(object sender, RoutedEventArgs e)
        {
            image.Source = _image_data[fileNameBox.SelectedIndex].GetImage(_numSlice - 1);
        }
    }

}
