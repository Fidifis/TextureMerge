using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ImageMagick;

namespace TextureMerge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Merge merge = new();
        private bool hasSetupPath = false;
        private bool hasEditedPath = false;

        public MainWindow()
        {
            InitializeComponent();
            MagickNET.Initialize();
        }
        
        private static string GetImagePath()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select an image file",
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*" //TODO: Add more formats
            };
            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileName;
            return string.Empty;
        }
        
        private void SetSaveImagePath()
        {
            var saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = PathToSave.Text,
                FileName = SaveImageName.Text,
                Title = "Select an image file",
                Filter = "PNG (*.png)|*.png|All files (*.*)|*.*" //TODO: Add more formats
            };
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
                var resizeDialog = new Resize(width, height)
                {
                    Owner = this
                };
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
