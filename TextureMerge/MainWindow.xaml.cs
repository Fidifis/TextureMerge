using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace TextureMerge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string GetImagePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select an image file";
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"; //TODO: Add more formats
            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileName;
            return string.Empty;
        }

        private void ButtonLoadR(object sender, RoutedEventArgs e)
        {
            string path = GetImagePath();
        }

        private void ButtonLoadG(object sender, RoutedEventArgs e)
        {
            string path = GetImagePath();
        }

        private void ButtonLoadB(object sender, RoutedEventArgs e)
        {
            string path = GetImagePath();
        }

        private void ButtonClearR(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClearG(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClearB(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonBrowse(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Select an image file";
            saveFileDialog.Filter = "PNG (*.png)|*.png|All files (*.*)|*.*"; //TODO: Add more formats
            if (saveFileDialog.ShowDialog() == true)
            {
                PathToSave.Text = Path.GetDirectoryName(saveFileDialog.FileName);
                SaveImageName.Text = Path.GetFileName(saveFileDialog.FileName);
            }
        }

        private void ButtonMerge(object sender, RoutedEventArgs e)
        {

        }
    }
}
