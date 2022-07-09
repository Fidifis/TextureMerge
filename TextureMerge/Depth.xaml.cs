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
    /// Interakční logika pro Depth.xaml
    /// </summary>
    public partial class Depth : Window
    {
        public byte NewDepth { get; private set; }
        public Depth()
        {
            InitializeComponent();
            radio8.IsChecked = true;
            NewDepth = 8;
        }

        private void OKButton(object sender, RoutedEventArgs e)
        {
            NewDepth = radio8.IsChecked == true ? (byte)8 : (byte)16;
            DialogResult = true;
            Close();
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
