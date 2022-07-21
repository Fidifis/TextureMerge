using System.Windows;

namespace TextureMerge
{
    public partial class UpdateAvailable : Window
    {
        const string MessageTemplate =
            "New version available.\n" +
            "{0}\n" +
            "Do you want to download it?";

        public bool Skip { get; private set; } = false;

        public UpdateAvailable(string version)
        {
            InitializeComponent();
            MessageText.Content = string.Format(MessageTemplate, version);
        }

        private void YesButton(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SkipButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Skip = true;
            Close();
        }
    }
}
