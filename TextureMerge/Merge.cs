using System;
using System.Windows.Media;
using ImageMagick;

namespace TextureMerge
{
    internal class Merge : IDisposable
    {
        MagickImage? red = null, green = null, blue = null;
        Channel redChSource = Channel.Red, greenChSource = Channel.Green, blueChSource = Channel.Blue;

        public MagickImage DoMerge()
        {
            if (red is null && green is null && blue is null)
                throw new InvalidOperationException("No image loaded");

            if (!CheckResolution(out int width, out int height))
                throw new InvalidOperationException("Resolution missmatch");

            // TODO read the color from color picker
            var result = new MagickImage(new MagickColor(0, 0, 0), width, height);
            using var resultPixels = result.GetPixels();
            using var redPixels = red?.GetPixels();
            using var greenPixels = green?.GetPixels();
            using var bluePixels = blue?.GetPixels();

            foreach (Pixel p in resultPixels)
            {
                p.SetValues(new ushort[] {
                    redPixels is not null ? redPixels[p.X, p.Y]!.GetChannel((int)redChSource) : (ushort)0,
                    greenPixels is not null ? greenPixels[p.X, p.Y]!.GetChannel((int)greenChSource) : (ushort)0,
                    bluePixels is not null ? bluePixels[p.X, p.Y]!.GetChannel((int)blueChSource) : (ushort)0,
                });
                resultPixels.SetPixel(p);
            }

            return result;
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
        
        public Merge Resize(int width, int height, bool stretch)
        {
            var newInst = new Merge()
            {
                redChSource = redChSource,
                greenChSource = greenChSource,
                blueChSource = blueChSource
            };
            
            if (red is not null)
                newInst.red = ResizeImage(red, width, height, stretch);
            if (green is not null)
                newInst.green = ResizeImage(green, width, height, stretch);
            if (blue is not null)
                newInst.blue = ResizeImage(blue, width, height, stretch);
            return newInst;
        }

        private static MagickImage ResizeImage(MagickImage source, int width, int height, bool stretch)
        {
            var result = (MagickImage)source.Clone();
            if (stretch)
            {
                var geo = new MagickGeometry(width, height)
                {
                    IgnoreAspectRatio = true
                };
                result.Resize(geo);
            }
            else
            {
                // TODO read color from color picker
                result.Resize(width, height);
                result.Extent(width, height, Gravity.Center, new MagickColor(0, 0, 0));
            }
            return result;
        }

        private static MagickImage MakeChannelThumbnail(MagickImage sourceBitmap, Channel channel)
        {
            // TODO read color from color picker
            using var thumb = (MagickImage)sourceBitmap.Clone();
            thumb.Thumbnail(512, 512);
            var result = new MagickImage(new MagickColor(0, 0, 0), thumb.Width, thumb.Height);
            using var pixels = result.GetPixels();
            using var sourcePixels = thumb.GetPixels();
            foreach (Pixel p in pixels)
            {
                p.SetValues(new ushort[] {
                    sourcePixels[p.X, p.Y]!.GetChannel((int)channel),
                    sourcePixels[p.X, p.Y]!.GetChannel((int)channel),
                    sourcePixels[p.X, p.Y]!.GetChannel((int)channel)});
                pixels.SetPixel(p);
            }

            return result;
        }

        public ImageSource LoadChannel(string path, Channel channelSlot, Channel channelSource)
        {
            if (path == string.Empty)
                throw new ArgumentException("Invalid path");

            var source = new MagickImage(path);

            if (source is null)
                throw new ArgumentException("Failed to load image");

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

            return MakeChannelThumbnail(source, channelSource).ToImageSource();
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

        public void Dispose()
        {
            red?.Dispose();
            green?.Dispose();
            blue?.Dispose();
        }
    }
}
