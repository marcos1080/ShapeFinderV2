using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ShapeFinderV2
{
    /// <summary>
    /// Class that contains colour manipulation methods.
    /// </summary>
    public static class ImageConverter
    {
        public static Bitmap ToBlackAndWhite(Bitmap image, int threshold)
        {
            // Find the grey hue of the background colour.
            Color backgroundColour = image.GetPixel(0, 0);
            int backgroundGrey = ConvertToGrey(backgroundColour);

            // Convert all pixels to either black or white.
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    // Convert pixel colour to greyscale value.
                    Color colour = image.GetPixel(x, y);
                    int grey = ConvertToGrey(colour);

                    // If the colour differs from the background more than the threshold
                    // pixel colour is white, otherwise black.
                    grey = Math.Abs(backgroundGrey - grey) > threshold ? 0 : 255;
                    image.SetPixel(x, y, Color.FromArgb(grey, grey, grey));
                }
            }

            return image;
        }

        private static int ConvertToGrey(Color colour)
        {
            return Convert.ToInt32(.21 * colour.R + .71 * colour.G + .071 * colour.B);
        }
    }
}
