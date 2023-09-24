using System.Windows;
using System.Threading.Tasks;
using System.Threading;

namespace TextureMerge
{
    public partial class ViewImage : Window
    {
        private readonly TMImage FullImage;

        public ViewImage(TMImage image, string channelName, string channelSource)
        {
            InitializeComponent();

            FullImage = image;

            var thubnail = image.Clone();
            thubnail.Image.Thumbnail(512, 512);

            TheImage.SetImageThumbnail(thubnail);
            Title = $"View image - in {channelName} channel - source {channelSource} - {image.Image.Width}x{image.Image.Height} - {image.FileName}";

            int width = image.Image.Width;
            int height = image.Image.Height;
            ResizeToFit(ref width, ref height, 768);

            Width = width;
            Height = height;
        }

        private static void ResizeToFit(ref int width, ref int height, int size)
        {
            double aspectRatio = (double)width / height;

            if (width > size || height > size)
            {
                if (aspectRatio > 1)
                {
                    width = size;
                    height = (int)(size / aspectRatio);
                }
                else
                {
                    height = size;
                    width = (int)(size * aspectRatio);
                }
            }
        }


        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (FullImage == null)
                return;

            Task.Run(() => {
                // This is here because Dispatcher.Invoke locks the image and the preview doesn't get loaded in time and nothing is rendered until the full resolution is processed.
                Thread.Sleep(50);
                TheImage.Dispatcher.Invoke(() =>
                {
                    TheImage.Source = FullImage.ConvertToBitmap();
                });
            });
        }
    }
}
