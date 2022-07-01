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
    }
}
