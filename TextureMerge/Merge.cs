using System;
using System.Threading.Tasks;
using System.Windows.Media;
using ImageMagick;

namespace TextureMerge
{
    internal class Merge : IDisposable
    {
        MagickImage? red = null, green = null, blue = null, alpha = null;
        readonly object redLock = new(), greenLock = new(), blueLock = new(), alphaLock = new();
        Channel redChSource = Channel.Red, greenChSource = Channel.Green,
            blueChSource = Channel.Blue, alphaChSource = Channel.Alpha;

        public Task<MagickImage> DoMergeAsync(MagickColor fillColor)
        {
            return Task.Run(() => {
                lock (redLock) lock (greenLock) lock (blueLock) lock (alphaLock) 
                    return DoMerge(fillColor);
            });
        }

        public MagickImage DoMerge(MagickColor fillColor)
        {
            if (red is null && green is null && blue is null && alpha is null)
                throw new InvalidOperationException("No image loaded");

            if (!CheckResolution(out int width, out int height))
                throw new InvalidOperationException("Resolution missmatch");

            var result = new MagickImage(fillColor, width, height);
            result.Depth = GetHighestDepth();

            if (alpha is not null)
                result.Alpha(AlphaOption.On);
            else
                result.Alpha(AlphaOption.Off);

            var resultPix = result.GetPixels();
            var resultPixels = resultPix.ToArray()!;
            var redPixels = red is not null ? red.GetPixels().ToArray()! : new ushort[width * height * 3];
            var greenPixels = green is not null ? green.GetPixels().ToArray()! : new ushort[width * height * 3];
            var bluePixels = blue is not null ? blue.GetPixels().ToArray()! : new ushort[width * height * 3];
            var alphaPixels = alpha is not null ? alpha.GetPixels().ToArray()! : new ushort[width * height * 3];

            for (int i = 0; i < resultPixels.Length; i++)
            {
                if (alpha is null)
                    resultPixels[i] = (i % 3) switch
                    {
                        0 => redPixels[i],
                        1 => greenPixels[i],
                        2 => bluePixels[i],
                        _ => throw new InvalidOperationException("Impossible Exception")
                    };

                else
                    resultPixels[i] = (i % 4) switch
                    {
                        0 => redPixels[i],
                        1 => greenPixels[i],
                        2 => bluePixels[i],
                        3 => alphaPixels[i],
                        _ => throw new InvalidOperationException("Impossible Exception")
                    };
            }
            resultPix.SetPixels(resultPixels);
            return result;
        }

        private int GetHighestDepth()
        {
            int max = 0;
            if (red is not null)
                max = red.Depth > max ? red.Depth : max;
            if (green is not null)
                max = green.Depth > max ? green.Depth : max;
            if (blue is not null)
                max = blue.Depth > max ? blue.Depth : max;
            if (alpha is not null)
                max = alpha.Depth > max ? alpha.Depth : max;

            return max;
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
                else
                {
                    width = blue.Width;
                    height = blue.Height;
                }
            }
            if (alpha is not null)
            {
                if (width is not 0)
                {
                    if (width != alpha.Width || height != alpha.Height)
                        return false;
                }
                else
                {
                    width = alpha.Width;
                    height = alpha.Height;
                }
            }
            return true;
        }

        public bool CheckResolution() => CheckResolution(out _, out _);

        public Task<Merge> ResizeAsync(int width, int height, bool stretch, MagickColor? fillColor = null)
        {
            return Task.Run(() => {
                lock (redLock) lock (greenLock) lock (blueLock) lock (alphaLock)
                    return Resize(width, height, stretch, fillColor);
            });
        }

        public Merge Resize(int width, int height, bool stretch, MagickColor? fillColor=null)
        {
            Merge merge = this;
            var newInst = new Merge()
            {
                redChSource = merge.redChSource,
                greenChSource = merge.greenChSource,
                blueChSource = merge.blueChSource,
                alphaChSource = merge.alphaChSource
            };
            
            if (red is not null)
                newInst.red = ResizeImage(red, width, height, stretch, fillColor);
            if (green is not null)
                newInst.green = ResizeImage(green, width, height, stretch, fillColor);
            if (blue is not null)
                newInst.blue = ResizeImage(blue, width, height, stretch, fillColor);
            if (alpha is not null)
                newInst.alpha = ResizeImage(alpha, width, height, stretch, fillColor);

            return newInst;
        }

        private static MagickImage ResizeImage(MagickImage source, int width, int height, bool stretch, MagickColor? fillColor = null)
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
                result.Resize(width, height);
                result.Extent(width, height, Gravity.Center,
                    fillColor is not null ? fillColor : new MagickColor(0, 0, 0));
            }

            return result;
        }

        private static MagickImage MakeChannelThumbnail(MagickImage sourceBitmap, Channel channel)
        {
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

        public Task<ImageSource> LoadChannelAsync(string path, Channel channelSlot, Channel channelSource)
        {
            switch (channelSlot)
            {
                case Channel.Red:
                    return Task.Run(() => { lock (redLock) return LoadChannel(path, channelSlot, channelSource); });
                case Channel.Green:
                    return Task.Run(() => { lock (greenLock) return LoadChannel(path, channelSlot, channelSource); });
                case Channel.Blue:
                    return Task.Run(() => { lock (blueLock) return LoadChannel(path, channelSlot, channelSource); });
                case Channel.Alpha:
                    return Task.Run(() => { lock (alphaLock) return LoadChannel(path, channelSlot, channelSource); });
                default:
                    throw new ArgumentException("Invalid channel");
            }
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
                case Channel.Alpha:
                    alpha = source;
                    alphaChSource = channelSource;
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
                case Channel.Alpha:
                    alpha = null;
                    break;
                default:
                    throw new ArgumentException("Invalid channel");
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
