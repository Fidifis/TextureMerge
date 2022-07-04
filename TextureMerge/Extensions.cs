using System;
using System.IO;
using System.Windows.Media;
using ImageMagick;

namespace TextureMerge
{
    public enum Channel : int { Red, Green, Blue }

    public static class Extensions
    {
        public static ImageSource ToImageSource(this MagickImage image)
        {
            var stream = new MemoryStream();
            image.Format = MagickFormat.Png;
            image.Write(stream);
            return (ImageSource)new ImageSourceConverter().ConvertFrom(stream)!;
        }

        public static void Save(this MagickImage bitmap, string saveFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(saveFilePath)))
                throw new ArgumentException("Invalid path");

            using FileStream stream = new(saveFilePath, FileMode.Create);
            bitmap.Format = MagickFormat.Png; //TODO read extension and decide format
            bitmap.Write(stream);
            stream.Close();
        }
    }
}
