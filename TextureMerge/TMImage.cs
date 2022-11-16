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

        public TMImage(MagickImage image)
        {
            Image = image;
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

        public TMImage Clone() => new TMImage((MagickImage)Image.Clone());

        ~TMImage()
        {
            Image?.Dispose();
        }
    }
}
