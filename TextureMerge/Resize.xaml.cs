using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TextureMerge
{
    /// <summary>
    /// Interakční logika pro Resize.xaml
    /// </summary>
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
