using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShapeFinderV2
{
    class Program
    {
        static void Main(string[] args)
        {
            double threshold = 30;
            string imageFolderPath = ""; // Insert path to save images here.
            string imageSaveFolderPath = ""; // Insert path to save images here.

            var imageLoader = new ImageLoader(imageFolderPath);
            var imageConverter = new ImageConverter(imageSaveFolderPath);
            
            Bitmap image;
            ImageScanner scanner;
            List<Shape> shapes;

            string filename = imageLoader.FileNames[1];
            Console.WriteLine($"File '{filename}'");
            image = imageLoader.GetImages(filename);
            imageConverter.ToBlackAndWhite(image);
            //imageConverter.Save(image, "test5.png");

            scanner = new ImageScanner(image);
            scanner.Scan();

            shapes = new List<Shape>();

            foreach (ShapeData s in scanner.Shapes)
            {
                shapes.Add(new Shape(s.Contour, s.Pixels, threshold));
            }

            foreach (Shape s in shapes)
            {
                Console.WriteLine($"Type: {s.Type}, Confidence: {s.Confidence}");
                foreach (Point c in s.Contour)
                {
                    image.SetPixel(c.X, c.Y, Color.FromArgb(0, 200, 0));
                }

                // Label image
                var gr = Graphics.FromImage(image);
                Rectangle rect = new Rectangle(s.Left, s.Top, s.Width, s.Height);
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                StringFormat format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                gr.DrawString(s.Type, new Font("Tahoma", 8), Brushes.CornflowerBlue, rect, format);
                gr.Flush();

                imageConverter.Save(image, filename);
            }

            Console.WriteLine();
        }
    }
}
