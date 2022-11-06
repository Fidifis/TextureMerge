using System;
using System.Threading.Tasks;
using System.Windows.Media;
using ImageMagick;

namespace TextureMerge
{
    internal class Merge : IDisposable
    {
        MagickImage red = null, green = null, blue = null, alpha = null;
        readonly object redLock = new object(), greenLock = new object(), blueLock = new object(), alphaLock = new object();
        private Channel redChSource = Channel.Red, greenChSource = Channel.Green,
            blueChSource = Channel.Blue, alphaChSource = Channel.Red;

        public Task<MagickImage> DoMergeAsync(MagickColor fillColor, int depth = -1)
        {
            return Task.Run(() => {
                lock (redLock) lock (greenLock) lock (blueLock) lock (alphaLock)
                    return DoMerge(fillColor, depth);
            });
        }

        public MagickImage DoMerge(MagickColor fillColor, int depth = -1)
        {
            if (red is null && green is null && blue is null && alpha is null)
                throw new InvalidOperationException("No image loaded");

            if (!CheckResolution(out int width, out int height))
                throw new InvalidOperationException("Resolution missmatch");

            var result = new MagickImage(fillColor, width, height);
            result.Depth = depth == -1 ? GetHighestDepth() : depth;

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
            resultPix.Dispose();
            return result;
        }

        private ushort[] CreateArrayWithColor(int capacity, ushort color)
        {
            var arr = new ushort[capacity];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = color;
            return arr;
        }

        public bool IsGrayScale(Channel channel)
        {
            MagickImage img;
            switch (channel)
            {
                case Channel.Red: img = red; break;
                case Channel.Green: img = green; break;
                case Channel.Blue: img = blue; break;
                case Channel.Alpha: img = alpha; break;
                default: throw new ArgumentException("Invalid channel");
            }

            if (img == null)
            {
                throw new NullReferenceException("Cannot check grayscale on empty image");
            }

            var pixels = img.GetPixels().ToArray();
            for (int i = 0; i < pixels.Length; i++)
            {
                if (i % 3 != 0)
                    continue;
                if (pixels[i] != pixels[i + 1] || pixels[i] != pixels[i + 2])
                    return false;
            }
            return true;
        }

        public bool IsDepthSame()
        {
            int depth = -1;
            if (red != null)
            {
                depth = red.Depth;
            }
            if (green != null)
            {
                if (depth == -1)
                    depth = green.Depth;
                else if (depth != green.Depth)
                    return false;
            }
            if (blue != null)
            {
                if (depth == -1)
                    depth = blue.Depth;
                else if (depth != blue.Depth)
                    return false;
            }
            if (alpha != null)
            {
                if (depth == -1)
                    depth = alpha.Depth;
                if (depth != alpha.Depth)
                    return false;
            }
            return true;
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
            if (channel == Channel.Alpha)
                throw new ArgumentException("Alpha can't be source channel");

            if (sourceBitmap == null)
                throw new ArgumentException("Source bitmap is null");

            if (sourceBitmap.HasAlpha)
                throw new ArgumentException("Source bitmap has alpha channel");

            var thumb = (MagickImage)sourceBitmap.Clone();
            thumb.Thumbnail(512, 512);
            var result = new MagickImage(new MagickColor(0, 0, 0), thumb.Width, thumb.Height);
            var resPix = result.GetPixels();
            var pixels = resPix.ToArray();
            var sourcePix = thumb.GetPixels();
            var sourcePixels = sourcePix.ToArray();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = sourcePixels[i - (i % 3) + (int)channel];
            }
            resPix.SetPixels(pixels);
            thumb.Dispose();
            resPix.Dispose();
            sourcePix.Dispose();
            return result;
        }

        private static ImageSource MakeChannelThumbnail_MemorySafe(MagickImage sourceBitmap, Channel channel)
        {
            var t = MakeChannelThumbnail(sourceBitmap, channel);
            var i = t.ToImageSource();
            t.Dispose();
            return i;
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
                    red?.Dispose();
                    red = source;
                    redChSource = channelSource;
                    break;
                case Channel.Green:
                    green?.Dispose();
                    green = source;
                    greenChSource = channelSource;
                    break;
                case Channel.Blue:
                    blue?.Dispose();
                    blue = source;
                    blueChSource = channelSource;
                    break;
                case Channel.Alpha:
                    alpha?.Dispose();
                    alpha = source;
                    alphaChSource = channelSource;
                    break;
                default:
                    throw new ArgumentException("Invalid channel");
            }

            return MakeChannelThumbnail_MemorySafe(source, channelSource);
        }

        // TODO This is very similar to LoadChannel. They could be rewriten to avoid duplicit code.
        public ImageSource SetChannelSource(Channel channel, Channel channelSource)
        {
            if (channelSource == Channel.Alpha)
                throw new ArgumentException("Alpha can't be source channel");

            MagickImage thumbnail;
            switch (channel)
            {
                case Channel.Red:
                    redChSource = channelSource;
                    thumbnail = red;
                    break;
                case Channel.Green:
                    greenChSource = channelSource;
                    thumbnail = green;
                    break;
                case Channel.Blue:
                    blueChSource = channelSource;
                    thumbnail = blue;
                    break;
                case Channel.Alpha:
                    alphaChSource = channelSource;
                    thumbnail = alpha;
                    break;
                default:
                    throw new ArgumentException("Invalid channel");
            }

            return MakeChannelThumbnail_MemorySafe(thumbnail, channelSource);
        }

        public ImageSource GetChannelThumbnail(Channel channel)
        {
            MagickImage thumbnail;
            switch (channel)
            {
                case Channel.Red:
                    thumbnail = red;
                    break;
                case Channel.Green:
                    thumbnail = green;
                    break;
                case Channel.Blue:
                    thumbnail = blue;
                    break;
                case Channel.Alpha:
                    thumbnail = alpha;
                    break;
                default:
                    throw new ArgumentException("Invalid channel");
            }

            if (thumbnail == null)
                return null;
            else
                return MakeChannelThumbnail_MemorySafe(thumbnail, GetSourceChannel(channel));
        }

        public void Swap(Channel ch1, Channel ch2)
        {
            MagickImage[] imgs = { red, green, blue, alpha, null };
            Channel[] channels = { redChSource, greenChSource, blueChSource, alphaChSource, 0 };

            imgs[4] = imgs[(int)ch1];
            imgs[(int)ch1] = imgs[(int)ch2];
            imgs[(int)ch2] = imgs[4];

            channels[4] = channels[(int)ch1];
            channels[(int)ch1] = channels[(int)ch2];
            channels[(int)ch2] = channels[4];

            red = imgs[0];
            green = imgs[1];
            blue = imgs[2];
            alpha = imgs[3];

            redChSource = channels[0];
            greenChSource = channels[1];
            blueChSource = channels[2];
            alphaChSource = channels[3];
        }

        public bool IsEmpty(Channel which)
        {
            switch (which)
            {
                case Channel.Red:
                    return red is null;
                case Channel.Green:
                    return green is null;
                case Channel.Blue:
                    return blue is null;
                case Channel.Alpha:
                    return alpha is null;
                default:
                    throw new ArgumentException("Invalid channel");
            }
        }

        public Channel GetSourceChannel(Channel channel)
        {
            switch (channel)
            {
                case Channel.Red:
                    return redChSource;
                case Channel.Green:
                    return greenChSource;
                case Channel.Blue:
                    return blueChSource;
                case Channel.Alpha:
                    return alphaChSource;
                default:
                    throw new ArgumentException("Invalid channel");
            }
        }

        public void Clear(Channel which)
        {
            switch (which)
            {
                case Channel.Red:
                    red.Dispose();
                    red = null;
                    break;
                case Channel.Green:
                    green.Dispose();
                    green = null;
                    break;
                case Channel.Blue:
                    blue.Dispose();
                    blue = null;
                    break;
                case Channel.Alpha:
                    alpha.Dispose();
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
            alpha?.Dispose();
        }
    }
}
