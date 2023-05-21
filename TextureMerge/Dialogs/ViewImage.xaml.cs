using System.Windows;

namespace TextureMerge
{
    public partial class ViewImage : Window
    {
        public ViewImage(TMImage image, string channelName, string channelSource)
        {
            InitializeComponent();
            Extensions.SetImageThumbnail(TheImage, image);

            Title = $"View image - in {channelName} channel - source {channelSource} - {image.Image.Width}x{image.Image.Height} - {image.FileName}";
        }
    }
}
