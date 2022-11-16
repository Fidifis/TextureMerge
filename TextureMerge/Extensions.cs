using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageMagick;

namespace TextureMerge
{
    public enum Channel : int { Red = 0, Green = 1, Blue = 2, Alpha = 3 }

    public static class Extensions
    {
        public static string Expand(this string path) => Environment.ExpandEnvironmentVariables(path);

        public static string ToStringRounded(this double value, int decimalPlaces)
        {
            return Math.Round(value, decimalPlaces).ToString();
        }

        public static void SetImageThumbnail(this Image element, TMImage image)
        {
            if (image == null)
            {
                element.Source = null;
                return;
            }

            using (var stream = new MemoryStream())
            {
                image.Image.Format = MagickFormat.Png;
                image.Image.Write(stream);

                var imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.StreamSource = stream;
                imageSource.EndInit();

                element.Source = imageSource;
            }
        }

        public static void Save(this TMImage bitmap, string saveFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(saveFilePath)))
                throw new ArgumentException("Invalid path");

            bitmap.Image.Format = Path.GetExtension(saveFilePath).GetMagickExtension();

            using (FileStream stream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write)) {
                bitmap.Image.Write(stream);
                stream.Close();
            }
        }

        public static Task SaveAsync(this TMImage bitmap, string saveFilePath)
        {
            return Task.Run(() => bitmap.Save(saveFilePath));
        }

        public static Color ToColor(this int color)
        {
            int r, g, b;
            r = (color & 0x00FF0000) >> 16;
            g = (color & 0x0000FF00) >> 8;
            b = (color & 0x000000FF);
            return Color.FromRgb
            (
                (byte)r,
                (byte)g,
                (byte)b
            );
        }

        public static int ToInt(this Color color)
        {
            int r = color.R,
                g = color.G,
                b = color.B;

            return (r << 16) | (g << 8) | b;
        }

        private static MagickFormat GetMagickExtension(this string ext)
        {
            switch (ext)
            {
                //This is some formats that are supported by ImageMagick
                case ".png": return MagickFormat.Png;
                case ".jpg": return MagickFormat.Jpeg;
                case ".jpeg": return MagickFormat.Jpeg;
                case ".bmp": return MagickFormat.Bmp;
                case ".gif": return MagickFormat.Gif;
                case ".tiff": return MagickFormat.Tiff;
                case ".tif": return MagickFormat.Tiff;
                case ".tga": return MagickFormat.Tga;
                case ".webp": return MagickFormat.WebP;
                case ".psd": return MagickFormat.Psd;
                case ".dib": return MagickFormat.Dib;
                case ".ico": return MagickFormat.Ico;
                case ".svg": return MagickFormat.Svg;
                default: throw new ArgumentException("Invalid extension");
            };
        }
    }
}
