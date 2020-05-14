using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ShapeFinderV2
{
    /// <summary>
    /// Simple class for loading images from a folder.
    /// </summary>
    public class ImageFileIO
    {
        public ImageFileIO(string directory)
        {
            DirectoryPath = directory;
        }

        public string DirectoryPath { get; set; }

        /// <summary>
        /// Property that retreives all the filenames from a directory.
        /// </summary>
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

        /// <summary>
        /// Get all images in directory.
        /// </summary>
        /// <returns></returns>
        public List<Bitmap> GetImages()
        {
            var images = new List<Bitmap>();
            foreach (string path in Directory.GetFiles(DirectoryPath))
            {
                images.Add(new Bitmap(path));
            }

            return images;
        }

        /// <summary>
        /// Get single image.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Bitmap GetImage(string fileName)
        {
            return new Bitmap($"{DirectoryPath}/{fileName}");
        }
    }
}
