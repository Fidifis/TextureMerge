using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
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

        public static ImageSource ToImageSource(this MagickImage image)
        {
            return null;
            // TODO This is memory leak. But it dont work when stream is disposed.
            //var stream = new MemoryStream();
            //image.Format = MagickFormat.Png;
            //image.Write(stream);
            //return (ImageSource)new ImageSourceConverter().ConvertFrom(stream);

        }

        public static void Save(this MagickImage bitmap, string saveFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(saveFilePath)))
                throw new ArgumentException("Invalid path");

            bitmap.Format = Path.GetExtension(saveFilePath).GetMagickExtension();

            using (FileStream stream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write)) {
                bitmap.Write(stream);
                stream.Close();
            }
        }

        public static Task SaveAsync(this MagickImage bitmap, string saveFilePath)
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
                case ".pdf": return MagickFormat.Pdf;
                case ".psd": return MagickFormat.Psd;
                case ".dib": return MagickFormat.Dib;
                case ".ico": return MagickFormat.Ico;
                case ".svg": return MagickFormat.Svg;
                default: throw new ArgumentException("Invalid extension");
            };
        }
    }
}
