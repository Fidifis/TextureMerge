using ImageMagick;

namespace TextureMerge
{
    /// <summary>
    /// "TextureMerge Image"
    /// Special class as MagickImage wrapper for reliable garbage collection.
    /// </summary>
    public class TMImage
    {
        public MagickImage Image { get; private set; }
        public string FileName { get; private set; } = null;

        public TMImage(MagickImage image)
        {
            Image = image;
        }

        public TMImage(MagickImage image, string name) : this(image)
        {
            FileName = name;
        }

        public ushort[] GetPixelArray()
        {
            using (var pix = Image.GetPixels())
            {
                return pix.ToArray();
            }
        }

        public void SetPixels(ushort[] pixels)
        {
            using (var pix = Image.GetPixels())
            {
                pix.SetPixels(pixels);
            }
        }

        public TMImage Clone()
        {
            return new TMImage((MagickImage)Image.Clone(), FileName);
        }

        ~TMImage()
        {
            Image?.Dispose();
        }
    }
}
