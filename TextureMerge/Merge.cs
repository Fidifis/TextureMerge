using System;
using System.Collections;
using System.Threading.Tasks;
using System.Windows.Media;
using ImageMagick;

namespace TextureMerge
{
    internal class Merge : IDisposable
    {
        MagickImage red = null, green = null, blue = null, alpha = null;
        readonly object redLock = new object(), greenLock = new object(), blueLock = new object(), alphaLock = new object();
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

            if (alpha is null)
                result.Alpha(AlphaOption.Off);
            else
                result.Alpha(AlphaOption.On);

            var resultPix = result.GetPixels();
            var resultPixels = resultPix.ToArray();
            var redPixels = red is null ? CreateArrayWithColor(width * height * 3, fillColor.R) : red.GetPixels().ToArray();
            var greenPixels = green is null ? CreateArrayWithColor(width * height * 3, fillColor.G) : green.GetPixels().ToArray();
            var bluePixels = blue is null ? CreateArrayWithColor(width * height * 3, fillColor.B) : blue.GetPixels().ToArray();
            var alphaPixels = alpha is null ? CreateArrayWithColor(width * height * 3, fillColor.A) : alpha.GetPixels().ToArray();

            for (int i = 0; i < resultPixels.Length; i++)
            {
                var redi = i - (i % 3) + (int)redChSource;
                var greeni = i - (i % 3) + (int)greenChSource;
                var bluei = i - (i % 3) + (int)blueChSource;
                var alphai = i - (i % 3) + (int)alphaChSource;

                if (alpha is null)
                {
                    switch (i % 3)
                    {
                        case 0: resultPixels[i] = redPixels[redi]; break;
                        case 1: resultPixels[i] = greenPixels[greeni]; break;
                        case 2: resultPixels[i] = bluePixels[bluei]; break;
                        default: throw new InvalidOperationException("Impossible Exception");
                    };
                }
                else
                {
                    switch (i % 4)
                    {
                        case 0: resultPixels[i] = redPixels[(i / 4) * 3 + (redi % 3)]; break;
                        case 1: resultPixels[i] = greenPixels[(i / 4) * 3 + (greeni % 3)]; break;
                        case 2: resultPixels[i] = bluePixels[(i / 4) * 3 + (bluei % 3)]; break;
                        case 3: resultPixels[i] = alphaPixels[(i / 4) * 3 + (alphai % 3)]; break;
                        default: throw new InvalidOperationException("Impossible Exception");
                    };
                }
            }
            resultPix.SetPixels(resultPixels);
            return result;
        }

        private ushort[] CreateArrayWithColor(int capacity, ushort color)
        {
            var arr = new ushort[capacity];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = color;
            return arr;
        }

        private int GetHighestDepth()
        {
            int max = 0;
            if (red != null)
                max = red.Depth > max ? red.Depth : max;
            if (green != null)
                max = green.Depth > max ? green.Depth : max;
            if (blue != null)
                max = blue.Depth > max ? blue.Depth : max;
            if (alpha != null)
                max = alpha.Depth > max ? alpha.Depth : max;

            return max;
        }

        public bool CheckResolution(out int width, out int height)
        {
            width = height = 0;

            if (red != null)
            {
                width = red.Width;
                height = red.Height;
            }
            if (green != null)
            {
                if (width != 0)
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
            if (blue != null)
            {
                if (width != 0)
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
            if (alpha != null)
            {
                if (width != 0)
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

        public Task<Merge> ResizeAsync(int width, int height, bool stretch, MagickColor fillColor = null)
        {
            return Task.Run(() => {
                lock (redLock) lock (greenLock) lock (blueLock) lock (alphaLock)
                    return Resize(width, height, stretch, fillColor);
            });
        }

        public Merge Resize(int width, int height, bool stretch, MagickColor fillColor = null)
        {
            Merge merge = this;
            var newInst = new Merge()
            {
                redChSource = merge.redChSource,
                greenChSource = merge.greenChSource,
                blueChSource = merge.blueChSource,
                alphaChSource = merge.alphaChSource
            };

            if (red != null)
                newInst.red = ResizeImage(red, width, height, stretch, fillColor);
            if (green != null)
                newInst.green = ResizeImage(green, width, height, stretch, fillColor);
            if (blue != null)
                newInst.blue = ResizeImage(blue, width, height, stretch, fillColor);
            if (alpha != null)
                newInst.alpha = ResizeImage(alpha, width, height, stretch, fillColor);

            return newInst;
        }

        private static MagickImage ResizeImage(MagickImage source, int width, int height, bool stretch, MagickColor fillColor = null)
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
                    fillColor ?? new MagickColor(0, 0, 0));
            }

            return result;
        }

        private static MagickImage MakeChannelThumbnail(MagickImage sourceBitmap, Channel channel)
        {
            if (sourceBitmap.HasAlpha)
                throw new ArgumentException("Source bitmap has alpha channel");

            var thumb = (MagickImage)sourceBitmap.Clone();
            thumb.Thumbnail(512, 512);
            var result = new MagickImage(new MagickColor(0, 0, 0), thumb.Width, thumb.Height);
            var resPix = result.GetPixels();
            var pixels = resPix.ToArray();
            var sourcePixels = thumb.GetPixels().ToArray();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = sourcePixels[i - (i % 3) + (int)channel];
            }
            resPix.SetPixels(pixels);
            thumb.Dispose();
            resPix.Dispose();
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

            if (channelSource == Channel.Alpha)
                throw new ArgumentException("Alpha can't be source channel");

            var source = new MagickImage(path);

            if (source is null)
                throw new ArgumentException("Failed to load image");

            source.Alpha(AlphaOption.Off);

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
