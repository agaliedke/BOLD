﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Collections.Generic;

namespace BOLD
{
    [Serializable]
    class ImageSlice
    {
        public int xSize { get; private set; }
        public int ySize { get; private set; }
        public int zSize { get; private set; }
        //private int xSize, ySize, zSize;
        public double xRealSize { get; private set; }
        public double yRealSize { get; private set; }
        public double zRealSize { get; private set; }
        public string realUnit { get; private set; }
        public string sliceName { get; private set; }
        public string sliceFileName { get; set; }
        public double[,,] sliceData { get; private set; }
        public double minIntensity { get; set; }
        public double maxIntensity { get; set; }
        public double zeroIntensity { get; private set; }
        public Int32Rect selection { get; private set; }
        public string header { get; private set; }

        /// <summary>
        /// Constructor of clase ImageSlice. Loades a file with an image to a object ImageSlice
        /// </summary>
        /// <param name="filePath">Full path to file to be load</param>
        public ImageSlice(string filePath)
        {
            string data = "";
            string[] words = null;
            header = "";
            try
            {   // Open the text file using a stream reader.
                using (StreamReader reader = new StreamReader(filePath))
                {
                    data = reader.ReadLine() + "\n";
                    while (data[0] == '#')
                    {
                        header += data;
                        data = reader.ReadLine() + "\n";
                    }
                    data += reader.ReadToEnd();
                    words = header.Split(new[] { ' ', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The file could not be read: " + e.Message, "ImageSlice", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // reading the header
            if (words.Count() == 0)
                throw new ArgumentException("ImageSlice", "Header data not found");
            for (int i = 0; i < words.Count(); i++)
            {
                if (words[i] == "#")
                {
                    if (words[i + 1] == "x_dimension:")
                        xSize = Int32.Parse(words[i + 2]);
                    if (words[i + 1] == "y_dimension:")
                        ySize = Int32.Parse(words[i + 2]);
                    if (words[i + 1] == "z_dimension:")
                        zSize = Int32.Parse(words[i + 2]);
                    if (words[i + 1] == "File:")
                        sliceName = words[i + 2];
                    if (words[i + 1] == "Voxel_Size:")
                    {
                        xRealSize = Double.Parse(words[i + 2], NumberStyles.Any, CultureInfo.InvariantCulture);
                        yRealSize = Double.Parse(words[i + 3], NumberStyles.Any, CultureInfo.InvariantCulture);
                        zRealSize = Double.Parse(words[i + 4], NumberStyles.Any, CultureInfo.InvariantCulture);
                    }
                    if (words[i + 1] == "Voxel_Units:")
                        realUnit = words[i + 2];

                }
            }
            words = data.Split(new[] { ' ', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Count() == 0)
                throw new ArgumentException("ImageSlice", "Image data not found");
            // reading the contrast data
            sliceData = new double[xSize, ySize, zSize];
            maxIntensity = 0;
            minIntensity = Int32.MaxValue;

            for (int i = 0; i < words.Count(); i += 4)
            {
                double word = Double.Parse(words[i + 3]);
                sliceData[Int32.Parse(words[i]) - 1, Int32.Parse(words[i + 1]) - 1, Int32.Parse(words[i + 2]) - 1] = word;
                if (maxIntensity < word)
                    maxIntensity = word;
                if (word != 0 && minIntensity > word)
                    minIntensity = word;
            }
            if (minIntensity > 0)
                zeroIntensity = 0.5; 
            else
                zeroIntensity = -minIntensity / (double)(maxIntensity - minIntensity);
            // initialize selection
            selection = new Int32Rect();
            int minX, maxX, minY, maxY, slice_index=0;
            do
            {
                minX = xSize; maxX = 0; minY = ySize; maxY = 0;
                for (int i = 0; i < xSize; i++)
                    for (int j = 0; j < ySize; j++)
                    {
                        if (sliceData[i, j, slice_index] != 0)
                        {
                            if (minX > i)
                                minX = i;
                            if (maxX < i)
                                maxX = i;
                            if (minY > j)
                                minY = j;
                            if (maxY < j)
                                maxY = j;
                        }
                    }
                if (maxX < minX)
                    minX = 0;
                if (maxY < minY)
                    minY = 0;
                slice_index++;
            } while (minX == maxX && minY == maxY && slice_index<zSize+1);

            selection = new Int32Rect(minY, minX, maxY - minY, maxX - minX);

        }
        /// <summary>
        /// Converts numeric data from the class with slice to BitmapSource
        /// </summary>
        /// <param name="i_slice">number of slice to return</param>
        /// <param name="zeroPoint">separation between red and blue color</param>
        /// <param name="paletteBrush">color palette taken e.g. from Rectangle.Fill source</param>
        /// <returns>BitmapSource with image form this class.</returns>
        public BitmapSource GetImage(int i_slice, int zeroPoint, LinearGradientBrush paletteBrush)
        {
            // TO DO: implement grayscal and rgb reaction on slider

            int stride = ySize  + (ySize % 4);
            byte[] pixelData = new byte[xSize * stride];
            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                {
                    byte color = sliceData[i, j, i_slice] == 0 ? (byte)0 :
                        (byte)(Math.Round((sliceData[i, j, i_slice] - minIntensity) / (double)(maxIntensity - minIntensity) * 255.0));


                        pixelData[j + i * stride] =  color;
                }
            Color col = GradientStopCollectionExtensions.GetRelativeColor(paletteBrush.GradientStops, 0.25);
            col = GradientStopCollectionExtensions.GetRelativeColor(paletteBrush.GradientStops, 0.75);
            var myPalette = new BitmapPalette(
                Enumerable.Range(0, 256).Select(c => GradientStopCollectionExtensions.GetRelativeColor(paletteBrush.GradientStops, 1-c/255.0)).ToList()
                );
            var pixelFormat = PixelFormats.Indexed8;
            return BitmapSource.Create(xSize, ySize, 96, 96, pixelFormat, myPalette, pixelData, stride);
        }
        /// <summary>
        /// Saves image to file
        /// </summary>
        /// <param name="filePath">Path to a file as a string.</param>
        public void SaveImage(string filePath)
        {
            try
            {   // Open the text file using a stream writer
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write(header);
                    for (int k = 0; k < zSize; k++)
                        for (int j = 0; j < ySize; j++)
                            for (int i = 0; i < xSize; i++)
                            {
                                //if (sliceData[i,j,k]>0)
                                writer.WriteLine((i + 1).ToString() + " " + (j + 1).ToString() + " " + (k + 1).ToString() + " " + sliceData[i, j, k].ToString());
                            }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The file could not be write: " + e.Message, "SaveImage", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        /// <summary>
        /// Calculates average of selected region.
        /// </summary>
        /// <param name="r">Rectangle object with coordinates of the selected region</param>
        /// <param name="i_slice">number of slice.</param>
        /// <returns>Tuple with avarage and standard deviation of the region.</returns>
        public Tuple<double, double> GetAverage(Int32Rect r, int i_slice)
        {
            var avg = 0.0;
            var std = 0.0;
            for (int i = r.X < 0 ? 0 : r.X; i < (r.X + r.Width > xSize ? xSize : r.X + r.Width) ; i++)
                for (int j = r.Y < 0 ? 0 : r.Y ; j < (r.Y + r.Height > ySize ? ySize : r.Y + r.Height) ; j++)
                {
                    avg += sliceData[i, j, i_slice-1];
                    std += (sliceData[i, j, i_slice-1] * sliceData[i, j, i_slice-1]);
                }
            avg /= r.Width * r.Height;
            std = Math.Sqrt(std / r.Width / r.Height - avg * avg);
            return Tuple.Create(avg, std);
        }
        /// <summary>
        /// Summs up two images.
        /// </summary>
        /// <param name="c1">image 1<param>
        /// <param name="c2">image 2</param>
        /// <returns>image 1 + image 2</returns>
        public static ImageSlice operator +(ImageSlice c1, ImageSlice c2)
        {
            if (c1.xSize != c2.xSize || c1.ySize != c2.ySize || c1.zSize != c2.zSize)
                throw new ArgumentOutOfRangeException("operator +: all sizes have to be the same");
            ImageSlice c = ObjectCopier.Clone<ImageSlice>(c1);
            c.sliceFileName = "(" + c1.sliceFileName + "+" + c2.sliceFileName + ")"; ;
            var tmp = c.minIntensity;
            c.minIntensity = c.maxIntensity;
            c.maxIntensity = tmp;
            for (int i = 0; i < c.xSize; i++)
                for (int j = 0; j < c.ySize; j++)
                    for (int k = 0; k < c.zSize; k++)
                    {
                        c.sliceData[i, j, k] += c2.sliceData[i, j, k];
                        if (c.sliceData[i, j, k] < c.minIntensity)
                            c.minIntensity = c.sliceData[i, j, k];
                        if (c.sliceData[i, j, k] > c.maxIntensity)
                            c.maxIntensity = c.sliceData[i, j, k];
                    }
            if (c.minIntensity > 0)
                c.zeroIntensity = 0.5;
            else
                c.zeroIntensity = -c.minIntensity / (double)(c.maxIntensity - c.minIntensity);

            return c;
        }
        /// <summary>
        /// Differentiate two images.
        /// </summary>
        /// <param name="c1">image 1</param>
        /// <param name="c2">image 2</param>
        /// <returns>image 1 - image 2</returns>
        public static ImageSlice operator -(ImageSlice c1, ImageSlice c2)
        {
            if (c1.xSize != c2.xSize || c1.ySize != c2.ySize || c1.zSize != c2.zSize)
                throw new ArgumentOutOfRangeException("operator -", "all sizes have to be the same");

            ImageSlice c = ObjectCopier.Clone<ImageSlice>(c1);
            c.sliceFileName = "(" + c1.sliceFileName + "-" + c2.sliceFileName + ")";
            var tmp = c.minIntensity;
            c.minIntensity = c.maxIntensity;
            c.maxIntensity = tmp;
            for (int i = 0; i < c.xSize; i++)
                for (int j = 0; j < c.ySize; j++)
                    for (int k = 0; k < c.zSize; k++)
                    {
                        c.sliceData[i, j, k] -= c2.sliceData[i, j, k];
                        if (c.sliceData[i, j, k] < c.minIntensity)
                            c.minIntensity = c.sliceData[i, j, k];
                        if (c.sliceData[i, j, k] > c.maxIntensity)
                            c.maxIntensity = c.sliceData[i, j, k];
                    }
            if (c.minIntensity > 0)
                c.zeroIntensity = 0.5;
            else
                c.zeroIntensity = -c.minIntensity / (double)(c.maxIntensity - c.minIntensity);
            return c;
        }
        public static ImageSlice operator /(ImageSlice c1, double c2)
        {
            ImageSlice c = ObjectCopier.Clone<ImageSlice>(c1);
            c.sliceFileName = "(" + c1.sliceFileName + "/3)";
            var tmp = c.minIntensity;
            c.minIntensity = c.maxIntensity;
            c.maxIntensity = tmp;
            for (int i = 0; i < c.xSize; i++)
                for (int j = 0; j < c.ySize; j++)
                    for (int k = 0; k < c.zSize; k++)
                    {
                        c.sliceData[i, j, k] = (c.sliceData[i, j, k]/c2);
                        if (c.sliceData[i, j, k] < c.minIntensity)
                            c.minIntensity = c.sliceData[i, j, k];
                        if (c.sliceData[i, j, k] > c.maxIntensity)
                            c.maxIntensity = c.sliceData[i, j, k];
                    }
            if (c.minIntensity > 0)
                c.zeroIntensity = 0.5;
            else
                c.zeroIntensity = -c.minIntensity / (double)(c.maxIntensity - c.minIntensity);
            return c;
        }
        public static ImageSlice operator /(ImageSlice c1, ImageSlice c2)
        {
            ImageSlice c = ObjectCopier.Clone<ImageSlice>(c1);
            c.sliceFileName = "(" + c1.sliceFileName + "/" + c2.sliceFileName + ")";
            var tmp = c.minIntensity;
            c.minIntensity = c.maxIntensity;
            c.maxIntensity = tmp;
            for (int i = 0; i < c.xSize; i++)
                for (int j = 0; j < c.ySize; j++)
                    for (int k = 0; k < c.zSize; k++)
                    {
                        if (c2.sliceData[i, j, k]==0)
                        {
                            c.sliceData[i, j, k] = 0;
                            continue;
                        }
                        c.sliceData[i, j, k] = (c.sliceData[i, j, k] / c2.sliceData[i, j, k]);
                        if (c.sliceData[i, j, k] < c.minIntensity)
                            c.minIntensity = c.sliceData[i, j, k];
                        if (c.sliceData[i, j, k] > c.maxIntensity)
                            c.maxIntensity = c.sliceData[i, j, k];
                    }
            if (c.minIntensity > 0)
                c.zeroIntensity = 0.5;
            else
                c.zeroIntensity = -c.minIntensity / (double)(c.maxIntensity - c.minIntensity);
            return c;
        }

    }
}
