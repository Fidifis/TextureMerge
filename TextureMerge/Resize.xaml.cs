using System.Windows;

namespace TextureMerge
{
    public partial class Resize : Window
    {
        public int NewWidth { get; private set; }
        public int NewHeight { get; private set; }

        public Resize(int width, int height)
        {
            InitializeComponent();
            WidthBox.Text = (NewWidth = width).ToString();
            HeightBox.Text = (NewHeight = height).ToString();
        }

        private void OKButton(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WidthBox.Text, out int width) && int.TryParse(HeightBox.Text, out int height))
            {
                NewWidth = width;
                NewHeight = height;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid input");
            }
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
