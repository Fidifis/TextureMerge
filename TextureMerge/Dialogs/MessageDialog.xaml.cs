using System.Windows;
using System.Windows.Media;

namespace TextureMerge
{
    /// <summary>
    /// Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : Window
    {
        public enum Type {Error, Warning, Notice}
        public enum Buttons { Ok, OkCancel, YesNo }

        public static bool? Show(string message, string title = "", Type type = Type.Notice, Buttons buttons = Buttons.Ok) =>
            new MessageDialog(message, title, type, buttons).ShowDialog();

        public MessageDialog(string message, string title = "", Type type = Type.Notice, Buttons buttons = Buttons.Ok)
        {
            InitializeComponent();
            MessageText.Text = message;
            ThisDialog.Title = title;

            switch(type)
            {
                case Type.Error:
                    ThisDialog.Background = new SolidColorBrush(Color.FromRgb(64, 50, 50));
                    break;
                case Type.Warning:
                    ThisDialog.Background = new SolidColorBrush(Color.FromRgb(64, 60, 50));
                    break;
            }

            switch(buttons)
            {
                case Buttons.Ok:
                    NegativeButton.Visibility = Visibility.Hidden;
                    break;
                case Buttons.YesNo:
                    PositiveButton.Content = "Yes";
                    NegativeButton.Content = "No";
                    break;
            }
        }

        private void ButtonPossitive(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonNegative(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
