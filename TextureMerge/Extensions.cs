using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SkiaSharp;

namespace TextureMerge
{
    public enum Channel { Red, Green, Blue }

    public static class Extensions
    {
        public static ImageSource ToImageSource(this SKBitmap bitmap)
        {
            SKImage image = SKImage.FromPixels(bitmap.PeekPixels());
            SKData encoded = image.Encode();
            Stream stream = encoded.AsStream();
            return (ImageSource)new ImageSourceConverter().ConvertFrom(stream)!;
        }

        public static SKBitmap Save(this SKBitmap bitmap, string saveFilePath)
        {
            if (!File.Exists(saveFilePath))
                throw new ArgumentException("Invalid path");

            using FileStream stream = new(saveFilePath, FileMode.OpenOrCreate);
            bitmap.Encode(stream, SKEncodedImageFormat.Png, 100); // TODO Detect extension and Add input for quality
            stream.Close();
            return bitmap;
        }
    }
}
