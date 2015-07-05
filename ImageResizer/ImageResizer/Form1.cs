using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageResizer
{
    public partial class Form1 : Form
    {
        string filenameIn = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filenameIn = openFileDialog1.FileName;
                Image originalImage = Image.FromFile(this.filenameIn);
                pictureBox1.Image = originalImage;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Image originalImage = Image.FromFile(this.filenameIn);
            
            double percentageUp = 100;
            double percentageMedian = 50;
            double percentBottom = 0;

            long outputSize = long.MaxValue;
            int desiredSize = (int)this.numericUpDown1.Value * 1024; 
            int desiredMax =  desiredSize + (int)(desiredSize * 0.1);
            int desiredMin = desiredSize - (int)(desiredSize * 0.1);

            for (int i = 0; i < 10; i++)
            {
                int newWidth = (int)(originalImage.Width * percentageMedian / 100);
                int newHeight = (int)(originalImage.Height * percentageMedian / 100);
                using (MemoryStream mem = new MemoryStream())
                {
                    ResizeImage(originalImage, newWidth, newHeight).Save(mem, ImageFormat.Jpeg);
                    outputSize = mem.Length;
                    if (outputSize > desiredMax)
                    {
                        percentageUp = percentageMedian;
                        percentageMedian = (percentageMedian - percentBottom) / 2 + percentBottom;
                    }
                    else if (outputSize < desiredMin)
                    {
                        percentBottom = percentageMedian;
                        percentageMedian = (percentageUp - percentageMedian) / 2 + percentageMedian;
                    }
                    else
                    {   // open save file dialog 
                        SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                        saveFileDialog1.Filter = "JPG files (*.jpg)|*.jpg";
                        saveFileDialog1.RestoreDirectory = true;
                        FileInfo fi = new FileInfo(filenameIn);
                        //saveFileDialog1.InitialDirectory = fi.DirectoryName;
                        saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(fi.FullName) + "-small.jpg";

                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            using (var fs = File.Create(saveFileDialog1.FileName))
                            {
                                mem.Position = 0;
                                mem.CopyTo(fs);
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Image ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return (Image)destImage;
        }
    }
}
