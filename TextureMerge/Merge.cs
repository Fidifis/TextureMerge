using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace TextureMerge
{
    internal class Merge
    {
        SKBitmap? red, green, blue;
        Channel redChSource, greenChSource, blueChSource;
        
        public void DoMerge(string saveFilePath)
        {
            if (!File.Exists(saveFilePath))
                throw new ArgumentException("Invalid path");

            if (!CheckResolution(out int width, out int height))
                throw new InvalidOperationException("Resolution missmatch");
            
            SKColor[] result = new SKColor[width * height];
            
            // TODO read the color from color picker
            SKColor[] redPixels = red is not null ? red.Pixels : new SKColor[width * height];
            SKColor[] greenPixels = green is not null ? green.Pixels : new SKColor[width * height];
            SKColor[] bluePixels = blue is not null ? blue.Pixels : new SKColor[width * height];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new SKColor(
                    redChSource switch
                    {
                        Channel.Red => redPixels[i].Red,
                        Channel.Green => redPixels[i].Green,
                        Channel.Blue => redPixels[i].Blue,
                        _ => throw new ArgumentException("Invalid channel"),
                    },
                    greenChSource switch
                    {
                        Channel.Red => greenPixels[i].Red,
                        Channel.Green => greenPixels[i].Green,
                        Channel.Blue => greenPixels[i].Blue,
                        _ => throw new ArgumentException("Invalid channel"),
                    },
                    blueChSource switch
                    {
                        Channel.Red => bluePixels[i].Red,
                        Channel.Green => bluePixels[i].Green,
                        Channel.Blue => bluePixels[i].Blue,
                        _ => throw new ArgumentException("Invalid channel"),
                    }
                    );
            }

            using var bitmap = new SKBitmap(width, height) { Pixels = result };
            using FileStream stream = new(saveFilePath, FileMode.OpenOrCreate);
            bitmap.Encode(stream, SKEncodedImageFormat.Png, 100); // TODO Detect extension and Add input for quality
            stream.Close();
        }

        public bool CheckResolution(out int width, out int height)
        {
            width = height = 0;
            
            if (red is not null)
            {
                width = red.Width;
                height = red.Height;
            }
            if (green is not null)
            {
                if (width is not 0)
                {
                    if (width != green.Width || height != green.Height)
                        return false;
                }
                else
                {
                    width = green.Width;
                    height = green.Height;
                }
            }
            if (blue is not null)
            {
                if (width is not 0)
                {
                    if (width != blue.Width || height != blue.Height)
                        return false;
                }
            }
            return true;
        }

        public bool CheckResolution() => CheckResolution(out _, out _);

        public void Resize(int width, int height, bool stretch)
        {
            // TODO read color from color picker
            if (red is not null)
                red = stretch ? Stretch(red, width, height) : ResizeKeepRatio(red, width, height, new SKColor(0, 0, 0));
            if (green is not null)
                green = stretch ? Stretch(green, width, height) : ResizeKeepRatio(green, width, height, new SKColor(0, 0, 0));
            if (blue is not null)
                blue = stretch ? Stretch(blue, width, height) : ResizeKeepRatio(blue, width, height, new SKColor(0, 0, 0));
        }

        private SKBitmap Stretch(SKBitmap bitmap, int width, int height) =>
            bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);

        private static SKBitmap ResizeKeepRatio(SKBitmap bitmap, int width, int height, SKColor color) =>
            FillUnusedSpace(Fit(bitmap, width, height), width, height, color);

        private static SKBitmap Fit(SKBitmap bitmap, int width, int height)
        {
            if (bitmap.Width > width && bitmap.Height > height)
            {
                if (bitmap.Width - width < bitmap.Height - height)
                    return Scale(bitmap, false, height);
                else
                    return Scale(bitmap, true, width);
            }
            else if (bitmap.Width < width && bitmap.Height < height)
            {
                if (width - bitmap.Width < height - bitmap.Height)
                    return Scale(bitmap, true, width);
                else
                    return Scale(bitmap, false, height);
            }
            else if (bitmap.Width > width)
            {
                return Scale(bitmap, true, width);
            }
            else if (bitmap.Height > height)
            {
                return Scale(bitmap, false, height);
            }
            else
                return bitmap;
        }
        
        private static SKBitmap Scale(SKBitmap bitmap, bool onWidth, int newRes)
        {
            if (onWidth)
                return bitmap.Resize(new SKImageInfo(newRes, (int)(bitmap.Height * (newRes / (float)bitmap.Width))), SKFilterQuality.High);
            else
                return bitmap.Resize(new SKImageInfo((int)(bitmap.Width * (newRes / (float)bitmap.Height)), newRes), SKFilterQuality.High);
        }
        
        private static SKBitmap FillUnusedSpace(SKBitmap bitmap, int width, int height, SKColor color)
        {
            if (bitmap.Width == width && bitmap.Height == height)
                return bitmap;

            SKBitmap result = new(width, height);
            using SKCanvas canvas = new(result);
            canvas.Clear(color);
            int x = 0, y = 0;

            if (bitmap.Width < width)
                x = (width - bitmap.Width) / 2;
            
            else if (bitmap.Height < height)
                y = (height - bitmap.Height) / 2;

            canvas.DrawBitmap(bitmap, x, y);
            return result;
        }

        private static SKBitmap ExtractChannel(SKBitmap sourceBitmap, Channel channel)
        {
            SKColor[] source = sourceBitmap.Pixels;
            SKColor[] result = new SKColor[source.Length];
            switch (channel)
            {
                case Channel.Red:
                    for (int i = 0; i < source.Length; i++)
                        result[i] = new SKColor(source[i].Red, source[i].Red, source[i].Red);
                    break;
                case Channel.Green:
                    for (int i = 0; i < source.Length; i++)
                        result[i] = new SKColor(source[i].Green, source[i].Green, source[i].Green);
                    break;
                case Channel.Blue:
                    for (int i = 0; i < source.Length; i++)
                        result[i] = new SKColor(source[i].Blue, source[i].Blue, source[i].Blue);
                    break;
                default:
                    throw new ArgumentException("Invalid channel");
            }
            return new SKBitmap(sourceBitmap.Width, sourceBitmap.Height) { Pixels = result };
        }

        public ImageSource LoadChannel(string path, Channel channelSlot, Channel channelSource)
        {
            if (path == string.Empty)
                throw new ArgumentException("Invalid path");

            var source = SKBitmap.Decode(path);
            switch (channelSlot)
            {
                case Channel.Red:
                    red = source;
                    redChSource = channelSource;
                    break;
                case Channel.Green:
                    green = source;
                    greenChSource = channelSource;
                    break;
                case Channel.Blue:
                    blue = source;
                    blueChSource = channelSource;
                    break;
                default:
                    throw new ArgumentException("Invalid channel");
            }
            return ExtractChannel(source, channelSource).ToImageSource();
        }

        public void Clear(Channel which)
        {
            switch (which)
            {
                case Channel.Red:
                    red = null;
                    break;
                case Channel.Green:
                    green = null;
                    break;
                case Channel.Blue:
                    blue = null;
                    break;
            }
        }
    }
}
