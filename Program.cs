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

            var imageLoader = new ImageFileIO(imageFolderPath);
            var processor = new ShapeProcessor(threshold);
            
            Bitmap image;
            ImageScanner scanner;
            List<Shape> shapes;

            // Detect shapes in each image.
            foreach (string filename in imageLoader.FileNames)
            {
                Console.WriteLine($"File '{filename}'");
                image = imageLoader.GetImage(filename);

                // Convert to black and white for easier detection.
                ImageConverter.ToBlackAndWhite(image, 3);

                // Scan image for shapes.
                scanner = new ImageScanner(image);
                scanner.Scan();

                shapes = new List<Shape>();

                // Get shape data from scanner.
                foreach (ShapeData s in scanner.Shapes)
                {
                    Shape shape = processor.InferShape(s.Pixels);
                    shapes.Add(shape);
                }

                // Output
                foreach (Shape s in shapes)
                {
                    // Write result to console.
                    Console.WriteLine($"Type: {s.Type}, Area: {s.Area:F2}, Confidence: {s.Confidence}");

                    // Draw contour on image.
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

                    // Save.
                    image.Save($"{imageSaveFolderPath}/{filename}");
                }
            }
        }
    }
}
