using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ShapeFinderV2
{
    public class ImageConverter
    {
        private readonly string _outputPath;
        private readonly int _threshold = 5;

        public ImageConverter(string outputPath)
        {
            _outputPath = outputPath;
        }

        public Bitmap ToBlackAndWhite(Bitmap image)
        {
            Color background = image.GetPixel(0, 0);
            int backgroundGrey = Convert.ToInt32(.21 * background.R + .71 * background.G + .071 * background.B);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color c = image.GetPixel(x, y);
                    int grey = Convert.ToInt32(.21 * c.R + .71 * c.G + .071 * c.B);

                    grey = Math.Abs(backgroundGrey - grey) > _threshold ? 0 : 255;
                    image.SetPixel(x, y, Color.FromArgb(grey, grey, grey));
                }
            }

            return image;
        }

        public void Show(Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void Save(Bitmap image, string fileName)
        {
            image.Save($"{_outputPath}/{fileName}");
        }
    }
}
