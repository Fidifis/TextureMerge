using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SkiaSharp;
using ImageMagick;

namespace TextureMerge
{
    public enum Channel : int { Red, Green, Blue }

    public static class Extensions
    {
        public static ImageSource ToImageSource(this SKBitmap bitmap)
        {
            SKImage image = SKImage.FromPixels(bitmap.PeekPixels());
            SKData encoded = image.Encode();
            Stream stream = encoded.AsStream();
            return (ImageSource)new ImageSourceConverter().ConvertFrom(stream)!;
        }

        public static ImageSource ToImageSource(this MagickImage bitmap)
        {
            using MemoryStream stream = new MemoryStream();
            bitmap.Format = MagickFormat.Jpeg;
            bitmap.Write(stream);
            return (ImageSource)new ImageSourceConverter().ConvertFrom(stream)!;
        }

        public static SKBitmap Save(this SKBitmap bitmap, string saveFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(saveFilePath)))
                throw new ArgumentException("Invalid path");

            using FileStream stream = new(saveFilePath, FileMode.OpenOrCreate);
            bitmap.Encode(stream, SKEncodedImageFormat.Png, 100); // TODO Detect extension and Add input for quality
            stream.Close();
            return bitmap;
        }

        public static void Save(this MagickImage bitmap, string saveFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(saveFilePath)))
                throw new ArgumentException("Invalid path");

            using FileStream stream = new(saveFilePath, FileMode.OpenOrCreate);
            bitmap.Format = MagickFormat.Png; //TODO read extension and decide format
            bitmap.Write(stream);
            stream.Close();
        }
    }
}
