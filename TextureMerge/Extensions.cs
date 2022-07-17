using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using ImageMagick;

namespace TextureMerge
{
    public enum Channel : int { Red, Green, Blue, Alpha }

    public static class Extensions
    {
        public static string Expand(this string path) => Environment.ExpandEnvironmentVariables(path);

        public static ImageSource ToImageSource(this MagickImage image)
        {
            var stream = new MemoryStream();
            image.Format = MagickFormat.Png;
            image.Write(stream);
            return (ImageSource)new ImageSourceConverter().ConvertFrom(stream);
        }

        public static void Save(this MagickImage bitmap, string saveFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(saveFilePath)))
                throw new ArgumentException("Invalid path");

            using (FileStream stream = new FileStream(saveFilePath, FileMode.Create)) {
                bitmap.Format = GetExtension(Path.GetExtension(saveFilePath));
                bitmap.Write(stream);
                stream.Close();
            }
        }

        public static Task SaveAsync(this MagickImage bitmap, string saveFilePath)
        {
            return Task.Run(() => bitmap.Save(saveFilePath));
        }

        private static MagickFormat GetExtension(string ext)
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
