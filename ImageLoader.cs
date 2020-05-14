using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ShapeFinderV2
{
    public class ImageLoader
    {
        public ImageLoader(string directory)
        {
            DirectoryPath = directory;
        }

        public string DirectoryPath { get; set; }

        public string[] FileNames
        {
            get
            {
                var filenames = new List<string>();
                foreach(string path in Directory.GetFiles(DirectoryPath))
                {
                    filenames.Add(Path.GetFileName(path));
                }

                return filenames.ToArray();
            }
        }

        public List<Bitmap> GetImages()
        {
            var images = new List<Bitmap>();
            foreach (string path in Directory.GetFiles(DirectoryPath))
            {
                images.Add(new Bitmap(path));
            }

            return images;
        }

        public Bitmap GetImages(string fileName)
        {
            return new Bitmap($"{DirectoryPath}/{fileName}");
        }
    }
}
