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
        private Merge merge = new Merge();
        
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
            RedCh.Source = merge.LoadRedChannel(GetImagePath());
        }

        private void ButtonLoadG(object sender, RoutedEventArgs e)
        {
            GreenCh.Source = merge.LoadGreenChannel(GetImagePath());
        }

        private void ButtonLoadB(object sender, RoutedEventArgs e)
        {
            BlueCh.Source = merge.LoadBlueChannel(GetImagePath());
        }

        private void ButtonClearR(object sender, RoutedEventArgs e)
        {
            RedCh.Source = null;
        }

        private void ButtonClearG(object sender, RoutedEventArgs e)
        {
            GreenCh.Source = null;
        }

        private void ButtonClearB(object sender, RoutedEventArgs e)
        {
            BlueCh.Source = null;
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
            merge.DoMerge(PathToSave.Text + "\\" + SaveImageName.Text);
        }

        private bool dummyColorSwap = false;
        private void ChangeDefaultColor(object sender, RoutedEventArgs e)
        {
            //TODO: Implement color picker
            if (dummyColorSwap)
                DefaultColorRect.Fill = new SolidColorBrush(Colors.Black);
            else
                DefaultColorRect.Fill = new SolidColorBrush(Colors.White);
            dummyColorSwap = !dummyColorSwap;
        }
    }
}
