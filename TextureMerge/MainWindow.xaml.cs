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
        private bool hasSetupPath = false;
        private bool hasEditedPath = false;

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
        
        private void SetSaveImagePath()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = PathToSave.Text;
            saveFileDialog.FileName = SaveImageName.Text;
            saveFileDialog.Title = "Select an image file";
            saveFileDialog.Filter = "PNG (*.png)|*.png|All files (*.*)|*.*"; //TODO: Add more formats
            if (saveFileDialog.ShowDialog() == true)
            {
                PathToSave.Text = Path.GetDirectoryName(saveFileDialog.FileName);
                SaveImageName.Text = Path.GetFileName(saveFileDialog.FileName);
                hasSetupPath = true;
            }
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
            SetSaveImagePath();
        }

        private void ButtonMerge(object sender, RoutedEventArgs e)
        {
            if (!(hasEditedPath && Directory.Exists(PathToSave.Text)) &&
                !hasSetupPath)
            {
                SetSaveImagePath();
                if (!hasSetupPath)
                {
                    MessageBox.Show("Operation aborted");
                    return;
                }
            }

            if (Path.GetExtension(SaveImageName.Text) is null or "")
            {
                if (
                MessageBox.Show("File don't have an extension!\n" +
                                "Do you want to continue?",
                                "No extension",
                                MessageBoxButton.YesNo)
                != MessageBoxResult.Yes)
                    return;
            }

            Merge correct = merge;

            if (!merge.CheckResolution(out int width, out int height))
            {
                var resizeDialog = new Resize(width, height);
                resizeDialog.Owner = this;
                if (resizeDialog.ShowDialog() == true)
                {
                    correct = merge.Resize(resizeDialog.NewWidth, resizeDialog.NewHeight, resizeDialog.DoStretch.IsChecked == true);
                }
                else
                {
                    MessageBox.Show("Operation aborted");
                    return;
                }
            }
            
            string path = PathToSave.Text + "\\" + SaveImageName.Text;
            if (Directory.Exists(PathToSave.Text))
                correct.DoMerge().Save(path);
            else
                MessageBox.Show("Save path is not valid!\n" +
                    "Check if the path is correct.");
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

        private void PathToSaveChanged(object sender, TextChangedEventArgs e)
        {
            hasEditedPath = true;
            hasSetupPath = false;
        }
    }
}
