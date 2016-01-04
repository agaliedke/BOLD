using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Threading;

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
        private void AddImage(ImageSlice slice, string sliceName)
        {
            // insert new data image
            slice.sliceFileName = sliceName; 
            _imageData.Add(slice);

            // update combobox
            ComboboxItem item = new ComboboxItem();
            item.Text = sliceName;
            item.Value = _numImage;
            fileNameBox.Items.Add(item);
            fileNameBox.SelectedIndex = _numImage;
            _numImage++;

            // update Image
            NumSlice = _numSlice;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                // load image from file
                ImageSlice slice = new ImageSlice(openFileDialog.FileName);
                _numSlice = 1;

                // update text field
                txtNum.Text = _numSlice.ToString();

                AddImage(slice, Path.GetFileNameWithoutExtension(openFileDialog.FileName));

                // Activate difference button if at least 2 images are added
                if (_imageData.Count >= 2)
                {
                    difference.IsEnabled = true;
                    sum.IsEnabled = true;
                }
            }

        }
        // List of images
        private List<ImageSlice> _imageData = new List<ImageSlice>();
        
        private int _numImage = 0;

        // slice changer controler
        private int _numSlice = 0;
        public int NumSlice
        {
            get { return _numSlice; }
            set
            {
                if (_imageData.Count == 0)
                    return;

                // check if slice is in range
                if (value < 1 )
                    _numSlice = 1;
                else if (value > _imageData[fileNameBox.SelectedIndex].zSize)
                    _numSlice = _imageData[fileNameBox.SelectedIndex].zSize;
                else
                    _numSlice = value;
                // update text box
                txtNum.Text = _numSlice.ToString();
                // insert proper image
                image.Source = _imageData[fileNameBox.SelectedIndex].GetImage(_numSlice - 1);
                // if resize box is checked resize image
                CheckedResize = resize.IsChecked.Value;

            }
        }

        // controler for image resize checkbox
        bool _checkedResize = false;
        public bool CheckedResize
        {
            get { return _checkedResize; }
            set
            {
                if (_imageData.Count == 0)
                    return;
                if (resize.IsChecked ?? true)
                    image.Source = new CroppedBitmap(image.Source as BitmapSource, _imageData[fileNameBox.SelectedIndex].selection);
                else
                    image.Source = _imageData[fileNameBox.SelectedIndex].GetImage(_numSlice - 1);
            }
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            NumSlice++;
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            NumSlice--;
        }

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNum == null)
                return;

            if (int.TryParse(txtNum.Text, out _numSlice))
                NumSlice = _numSlice;


        }

        private void fileName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_numSlice > _imageData[fileNameBox.SelectedIndex].zSize)
                NumSlice = _numSlice = _imageData[fileNameBox.SelectedIndex].zSize;
            else
                NumSlice = _numSlice;
                
        }

        private void resize_Checked(object sender, RoutedEventArgs e)
        {
            CheckedResize = true;
        }
        private void resize_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckedResize = false;
        }

        private void difference_Click(object sender, RoutedEventArgs e)
        {
            ImageSlice slice = _imageData[0] - _imageData[1];
            AddImage(slice, slice.sliceFileName);
        }
        private void sum_Click(object sender, RoutedEventArgs e)
        {
            ImageSlice slice = _imageData[0] + _imageData[1];
            AddImage(slice, slice.sliceFileName);
        }
    }

}
