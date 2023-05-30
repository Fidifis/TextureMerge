namespace TextureMerge
{
    internal static class ImageEdits
    {
        public static TMImage Invert(TMImage image)
        {
            var result = image.Clone();
            result.Image.Level(ushort.MaxValue, ushort.MinValue);
            return result;
        }

        public static TMImage FullRange(TMImage image)
        {
            var result = image.Clone();
            result.Image.AutoLevel();
            return result;
        }
    }
}
