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
            string path = GetImagePath();
            if (path != string.Empty)
                RedCh.Source = merge.LoadChannel(path, Channel.Red, Channel.Red);
        }
        
        private void ButtonLoadG(object sender, RoutedEventArgs e)
        {
            string path = GetImagePath();
            if (path != string.Empty)
                GreenCh.Source = merge.LoadChannel(path, Channel.Green, Channel.Green);
        }

        private void ButtonLoadB(object sender, RoutedEventArgs e)
        {
            string path = GetImagePath();
            if (path != string.Empty)
                BlueCh.Source = merge.LoadChannel(path, Channel.Blue, Channel.Blue);
        }

        private void ButtonClearR(object sender, RoutedEventArgs e)
        {
            merge.Clear(Channel.Red);
            RedCh.Source = null;
        }

        private void ButtonClearG(object sender, RoutedEventArgs e)
        {
            merge.Clear(Channel.Green);
            GreenCh.Source = null;
        }

        private void ButtonClearB(object sender, RoutedEventArgs e)
        {
            merge.Clear(Channel.Blue);
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
            if (!merge.CheckResolution(out int width, out int height))
            {
                // TODO call resize dialog and suggest width or height of exists
                merge.Resize(1024, 1024);
            }
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
