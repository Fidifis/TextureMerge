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
        public static ImageSource ToImageSource(this MagickImage image)
        {
            var stream = new MemoryStream();
            image.Format = MagickFormat.Png;
            image.Write(stream);
            return (ImageSource)new ImageSourceConverter().ConvertFrom(stream)!;
        }

        public static void Save(this MagickImage bitmap, string saveFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(saveFilePath)))
                throw new ArgumentException("Invalid path");

            using FileStream stream = new(saveFilePath, FileMode.Create);
            bitmap.Format = GetExtension(Path.GetExtension(saveFilePath));
            bitmap.Write(stream);
            stream.Close();
        }

        public static Task SaveAsync(this MagickImage bitmap, string saveFilePath)
        {
            return Task.Run(() => bitmap.Save(saveFilePath));
        }

        private static MagickFormat GetExtension(string ext) => ext switch
        {
            //This is some formats that are supported by ImageMagick
            ".png" => MagickFormat.Png,
            ".jpg" => MagickFormat.Jpeg,
            ".jpeg" => MagickFormat.Jpeg,
            ".bmp" => MagickFormat.Bmp,
            ".gif" => MagickFormat.Gif,
            ".tiff" => MagickFormat.Tiff,
            ".tif" => MagickFormat.Tiff,
            ".tga" => MagickFormat.Tga,
            ".webp" => MagickFormat.WebP,
            ".pdf" => MagickFormat.Pdf,
            ".psd" => MagickFormat.Psd,
            ".dib" => MagickFormat.Dib,
            ".ico" => MagickFormat.Ico,
            ".svg" => MagickFormat.Svg,
            _ => throw new ArgumentException("Invalid extension"),
        };
    }
}
