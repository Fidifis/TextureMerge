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
    public static class Extensions
    {
        public static ImageSource? toImageSource(this SKBitmap bitmap)
        {
            SKImage image = SKImage.FromPixels(bitmap.PeekPixels());
            SKData encoded = image.Encode();
            Stream stream = encoded.AsStream();
            return new ImageSourceConverter().ConvertFrom(stream) as ImageSource;
        }
    }
}
