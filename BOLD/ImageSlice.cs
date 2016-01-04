using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace BOLD
{
    [Serializable]
    class ImageSlice
    {
        public int xSize { get; private set; }
        public int ySize { get; private set; }
        public int zSize { get; private set; }
        //private int xSize, ySize, zSize;
        private double xRealSize, yRealSize, zRealSize;
        private string realType;
        public string sliceName { get; private set; }
        public string sliceFileName { get; set; }
        public int[, ,] sliceData { get; private set; }
        private int minIntensity, maxIntensity;
        public Int32Rect selection { get; private set; }
        public string header { get; private set; }

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
                        xRealSize = Double.Parse(words[i + 2]);
                        yRealSize = Double.Parse(words[i + 3]);
                        zRealSize = Double.Parse(words[i + 4]);
                    }
                    if (words[i + 1] == "Voxel_Units:")
                        realType = words[i + 2];

                }
            }
            words = data.Split(new[] { ' ', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Count() == 0)
                throw new ArgumentException("ImageSlice", "Image data not found");
            // reading the contrast data
            sliceData = new int[xSize, ySize, zSize];
            maxIntensity = 0;
            minIntensity = Int32.MaxValue;

            for (int i = 0; i < words.Count(); i += 4)
            {
                int word = Int32.Parse(words[i + 3]);
                sliceData[Int32.Parse(words[i]), Int32.Parse(words[i + 1]), Int32.Parse(words[i + 2]) - 1] = word;
                if (maxIntensity < word)
                    maxIntensity = word;
                if (word != 0 && minIntensity > word)
                    minIntensity = word;
            }

            // initialize selection
            selection = new Int32Rect();
            int minX, maxX, minY, maxY;
            minX = xSize; maxX = 0; minY = ySize; maxY = 0;
            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                {
                    if (sliceData[i, j, 0] != 0)
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
            selection = new Int32Rect(minY, minX, maxY - minY, maxX - minX);

        }
        public BitmapSource GetImage(int i_slice)
        {

            byte[] pixelData = new byte[xSize * ySize];
            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                {
                    byte color = sliceData[i, j, i_slice] == 0 ? (byte)0 :
                        (byte)(Math.Round((sliceData[i, j, i_slice] - minIntensity) / (double)(maxIntensity - minIntensity) * 255.0));
                    pixelData[j + i * xSize] = color;
                }

            BitmapSource bmpSource = BitmapSource.Create(xSize, ySize, 96, 96, PixelFormats.Gray8, null, pixelData, xSize);
            return bmpSource;
        }
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
                                if (sliceData[i,j,k]>0)
                                    writer.WriteLine(i.ToString() + " " + j.ToString() + " " + (k+1).ToString() + " " + sliceData[i, j, k].ToString());
                            }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The file could not be write: " + e.Message, "SaveImage", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        public static ImageSlice operator +(ImageSlice c1, ImageSlice c2)
        {
            if (c1.xSize != c2.xSize || c1.ySize != c2.ySize || c1.zSize != c2.zSize)
                throw new ArgumentOutOfRangeException("operator +: all sizes have to be the same");
            ImageSlice c = ObjectCopier.Clone<ImageSlice>(c1);
            c.sliceFileName = c1.sliceFileName + "+" + c2.sliceFileName;
            for (int i = 0; i < c.xSize; i++)
                for (int j = 0; j < c.ySize; j++)
                    for (int k = 0; k < c.zSize; k++)
                        c.sliceData[i, j, k] += c2.sliceData[i, j, k];
            return c;
        }
        public static ImageSlice operator -(ImageSlice c1, ImageSlice c2)
        {
            if (c1.xSize != c2.xSize || c1.ySize != c2.ySize || c1.zSize != c2.zSize)
                throw new ArgumentOutOfRangeException("operator -", "all sizes have to be the same");

            ImageSlice c = ObjectCopier.Clone<ImageSlice>(c1);
            c.sliceFileName = c1.sliceFileName + "-" + c2.sliceFileName;
            for (int i = 0; i < c.xSize; i++)
                for (int j = 0; j < c.ySize; j++)
                    for (int k = 0; k < c.zSize; k++)
                        c.sliceData[i, j, k] -= c2.sliceData[i, j, k];
            return c;
        }

    }
}
