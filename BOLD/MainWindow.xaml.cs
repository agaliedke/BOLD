using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System;

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
        private void exit_Click(object sender, RoutedEventArgs e)
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
            fileNameBox.SelectedIndex = fileNameBox.Items.Count - 1;
            _numImage++;

            // update Image
            NumSlice = _numSlice;
        }
        private void ReplaceImage(ImageSlice slice, string sliceName)
        {
            // replace data image
            slice.sliceFileName = sliceName;
            _imageData[fileNameBox.SelectedIndex] = slice;

            // update combobox
            (fileNameBox.Items[fileNameBox.SelectedIndex] as ComboboxItem).Text = sliceName;

            // update Image
            NumSlice = _numSlice;
        }

        private void add_Click(object sender, RoutedEventArgs e)
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
                    differenceAB.IsEnabled = true;
                    differenceBA.IsEnabled = true;
                    sum.IsEnabled = true;
                }
            }
        }
        private void replace_Click(object sender, RoutedEventArgs e)
        {
            if (fileNameBox.SelectedIndex == -1)
                throw new System.Exception("No image selected, cannot replace!");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                // load image from file
                ImageSlice slice = new ImageSlice(openFileDialog.FileName);
                _numSlice = 1;
                // update text field
                txtNum.Text = _numSlice.ToString();

                ReplaceImage(slice, Path.GetFileNameWithoutExtension(openFileDialog.FileName));
            }
        }
        private void new_Click(object sender, RoutedEventArgs e)
        {
            _imageData.Clear();
            fileNameBox.Items.Clear();
            image.Source = null;
            differenceAB.IsEnabled = false;
            differenceBA.IsEnabled = false;
            sum.IsEnabled = false;
            txtNum.Text = "0";
        }
        private void save_As_Click(object sender, RoutedEventArgs e)
        {
            if (fileNameBox.SelectedIndex == -1)
                throw new System.Exception("No image selected, cannot save!");
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                _imageData[fileNameBox.SelectedIndex].SaveImage(saveFileDialog.FileName);
            }
        }
        private void about_Click(object sender, RoutedEventArgs e)
        {
            var about = new About();
            about.Show();
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
                // change dataContent for Image: max and min intensity
                upperBond.Content = _imageData[fileNameBox.SelectedIndex].maxIntensity.ToString();
                lowerBond.Content = _imageData[fileNameBox.SelectedIndex].minIntensity.ToString();
                // set DataContext for zeroIntensity
                // this will be then used by xaml and MarginConverter
                if (_imageData[fileNameBox.SelectedIndex].zeroIntensity == -1)
                {
                    zeroBond.Content = "";
                    zeroBond.DataContext = 0.0;
                    zeroBond.Visibility = Visibility.Collapsed;
                }
                else
                {
                    zeroBond.Content = "0";
                    zeroBond.DataContext = _imageData[fileNameBox.SelectedIndex].zeroIntensity;
                    zeroBond.Visibility = Visibility.Visible;
                }
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
        private void resize_Checked(object sender, RoutedEventArgs e)
        {
            CheckedResize = true;
        }
        private void resize_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckedResize = false;
        }

        // controler for combobox -> loaded image changer
        private void fileName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fileNameBox.SelectedIndex == -1)
                return;
            if (_numSlice > _imageData[fileNameBox.SelectedIndex].zSize)
                NumSlice = _numSlice = _imageData[fileNameBox.SelectedIndex].zSize;
            else
                NumSlice = _numSlice;
                
        }


        // buttons of different operation controllers
        private void difference_ClickAB(object sender, RoutedEventArgs e)
        {
            ImageSlice slice = _imageData[0] - _imageData[1];
            AddImage(slice, slice.sliceFileName);
        }
        private void difference_ClickBA(object sender, RoutedEventArgs e)
        {
            ImageSlice slice = _imageData[1] - _imageData[0];
            AddImage(slice, slice.sliceFileName);
        }
        private void sum_Click(object sender, RoutedEventArgs e)
        {
            ImageSlice slice = _imageData[0] + _imageData[1];
            AddImage(slice, slice.sliceFileName);
        }

        // remove button
        private void remove_Click(object sender, RoutedEventArgs e)
        {
            if (fileNameBox.SelectedIndex == -1)
                throw new System.Exception("No image selected, cannot remove!");
            _imageData.RemoveAt(fileNameBox.SelectedIndex);
            fileNameBox.Items.RemoveAt(fileNameBox.SelectedIndex);
            image.Source = null;
            if (_imageData.Count < 2)
            {
                differenceAB.IsEnabled = false;
                differenceBA.IsEnabled = false;
                sum.IsEnabled = false;
            }

        }


        // functionality of selection box for active image

        bool mouseDown = false; // Set to 'true' when mouse is held down.
        Point mouseDownPos; // The point where the mouse button was clicked down.
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Capture and track the mouse.
            mouseDown = true;
            mouseDownPos = e.GetPosition(image);
            mouseDownPos = transformMouse(mouseDownPos);

            image.CaptureMouse();

            // Initial placement of the drag selection box.         
            Canvas.SetLeft(selectionBox, mouseDownPos.X);
            Canvas.SetTop(selectionBox, mouseDownPos.Y);
            selectionBox.Width = 0;
            selectionBox.Height = 0;

            // Make the drag selection box visible.
            selectionBox.Visibility = Visibility.Visible;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Release the mouse capture and stop tracking it.
            mouseDown = false;
            image.ReleaseMouseCapture();

            // Hide the drag selection box.
            selectionBox.Visibility = Visibility.Collapsed;

            Point mouseUpPos = e.GetPosition(image);
            mouseUpPos = transformMouse(mouseUpPos);
            //
            // The mouse has been released, calculate all related statistics below
            //
            xPos.Text = Math.Round(mouseUpPos.X).ToString();
            yPos.Text = Math.Round(mouseUpPos.Y).ToString();
            if (selectionBox.Width>0 && selectionBox.Height>0)
            {
                var r = new Int32Rect(
                    Convert.ToInt32(Canvas.GetLeft(selectionBox) * _imageData[fileNameBox.SelectedIndex].xSize / image.Width),
                    Convert.ToInt32(Canvas.GetTop(selectionBox) * _imageData[fileNameBox.SelectedIndex].ySize / image.Height),
                    Convert.ToInt32(selectionBox.Width * _imageData[fileNameBox.SelectedIndex].xSize / image.Width),
                    Convert.ToInt32(selectionBox.Height * _imageData[fileNameBox.SelectedIndex].ySize / image.Height)
                    );
                var average = _imageData[fileNameBox.SelectedIndex].GetAverage(r, _numSlice);
                avgImg.Text = (Math.Truncate(average.Item1 * 10) / 10).ToString();
                stdImg.Text = (Math.Truncate(average.Item2 * 10) / 10).ToString();
                xSize.Text = (Math.Truncate(r.Width * _imageData[fileNameBox.SelectedIndex].xRealSize * 100.0) / 100.0).ToString() +
                    " " + _imageData[fileNameBox.SelectedIndex].realUnit;
                ySize.Text = (Math.Truncate(r.Height * _imageData[fileNameBox.SelectedIndex].yRealSize * 100.0) / 100.0).ToString() +
                    " " + _imageData[fileNameBox.SelectedIndex].realUnit;
                zSize.Text = _imageData[fileNameBox.SelectedIndex].zRealSize.ToString() + " " + _imageData[fileNameBox.SelectedIndex].realUnit;
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                // When the mouse is held down, reposition the drag selection box.

                Point mousePos = e.GetPosition(image);

                mousePos = transformMouse(mousePos);

                if (mouseDownPos.X < mousePos.X)
                {
                    Canvas.SetLeft(selectionBox, mouseDownPos.X);
                    selectionBox.Width = mousePos.X - mouseDownPos.X;
                }
                else
                {
                    Canvas.SetLeft(selectionBox, mousePos.X);
                    selectionBox.Width = mouseDownPos.X - mousePos.X;
                }

                if (mouseDownPos.Y < mousePos.Y)
                {
                    Canvas.SetTop(selectionBox, mouseDownPos.Y);
                    selectionBox.Height = mousePos.Y - mouseDownPos.Y;
                }
                else
                {
                    Canvas.SetTop(selectionBox, mousePos.Y);
                    selectionBox.Height = mouseDownPos.Y - mousePos.Y;
                }
            }
        }
        // transforming mouse position invers to transformation of image object
        private Point transformMouse(Point mousePos)
        {
            var rt = new RotateTransform(-90, image.ActualWidth / 2.0, image.ActualHeight / 2.0);
            var st = new ScaleTransform(-1, 1, image.ActualWidth / 2.0, image.ActualHeight / 2.0);
            mousePos = st.Transform(mousePos);
            mousePos = rt.Transform(mousePos);
            return mousePos;
        }
    }

}
