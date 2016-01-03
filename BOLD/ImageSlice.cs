using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Media;

namespace BOLD
{
    class ImageSlice
    {
        public int xSize { get; set; }
        public int ySize { get; set;  }
        public int zSize { get; set; }
        //private int xSize, ySize, zSize;
        private double xRealSize, yRealSize, zRealSize;
        private string realType;
        private string sliceName;
        private int[, ,] sliceData;
        private int minIntensity, maxIntensity;
        public ImageSlice(string filePath)
        {
            string header="", data="";
            string[] words = null;
            try
            {   // Open the text file using a stream reader.
                using (StreamReader reader = new StreamReader(filePath))
                {
                    data = reader.ReadLine() + " ";
                    while (data[0] == '#')
                    {
                        header += data;
                        data = reader.ReadLine() + " ";
                    }
                    data += reader.ReadToEnd();
                    words = header.Split(new[] { ' ', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            // reading the header
            if (words.Count() == 0)
                throw new ArgumentException("ImageSlice: Header data not found");
            for (int i = 0; i < words.Count(); i++)
            {
                if (words[i]=="#")
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
                throw new ArgumentException("ImageSlice: Image data not found");
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

        }
        public BitmapSource GetImage(int i_slice)
        {

            byte[] pixelData = new byte[xSize * ySize];
            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                {
                    byte color = sliceData[i, j, i_slice]==0?(byte)0:
                        (byte)(Math.Round(sliceData[i, j, i_slice]-minIntensity/(double)(maxIntensity-minIntensity)*255.0));
                    pixelData[j + i * xSize] = color;
                }

            BitmapSource bmpSource = BitmapSource.Create(xSize, ySize, 96, 96, PixelFormats.Gray8, null, pixelData, xSize);
            return bmpSource;
        }
    }
}
