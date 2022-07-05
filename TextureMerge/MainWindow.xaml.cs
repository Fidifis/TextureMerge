using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ImageMagick;
using System.Threading.Tasks;

namespace TextureMerge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush statusBlueColor = new(Color.FromRgb(51, 150, 226));
        private static readonly SolidColorBrush statusGreenColor = new(Color.FromRgb(51, 226, 110));
        private readonly Merge merge = new();
        private bool hasSetupPath = false;
        private bool hasEditedPath = false;

        public MainWindow()
        {
            InitializeComponent();
            MagickNET.Initialize();
        }

        private void SetStatus() => SetStatus("", statusBlueColor);

        private void SetStatus(string message, SolidColorBrush color)
        {
            StatusLabel.Foreground = color;
            StatusLabel.Content = message;
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
        
        private async void ButtonLoad(Image WPFElement, Channel channel, Channel sourceChannel)
        {
            string path = GetImagePath();
            if (path != string.Empty)
            {
                SetStatus("Loading...", statusBlueColor);
                WPFElement.Source = await merge.LoadChannelAsync(path, channel, sourceChannel);
                SetStatus();
            }
        }

        private void ButtonLoadR(object sender, RoutedEventArgs e)
        {
            ButtonLoad(RedCh, Channel.Red, Channel.Red);
        }
        
        private void ButtonLoadG(object sender, RoutedEventArgs e)
        {
            ButtonLoad(GreenCh, Channel.Green, Channel.Green);
        }

        private void ButtonLoadB(object sender, RoutedEventArgs e)
        {
            ButtonLoad(BlueCh, Channel.Blue, Channel.Blue);
        }
        
        private void ButtonLoadA(object sender, RoutedEventArgs e)
        {
            ButtonLoad(AlphaCh, Channel.Alpha, Channel.Red);
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
        
        private void ButtonClearA(object sender, RoutedEventArgs e)
        {
            merge.Clear(Channel.Alpha);
            AlphaCh.Source = null;
        }

        private void ButtonBrowse(object sender, RoutedEventArgs e)
        {
            SetSaveImagePath();
        }

        private async void ButtonMerge(object sender, RoutedEventArgs e)
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
                    SetStatus("Resizeing...", statusBlueColor);
                    correct = await merge.ResizeAsync(resizeDialog.NewWidth, resizeDialog.NewHeight, resizeDialog.DoStretch.IsChecked == true);
                }
                else
                {
                    MessageBox.Show("Operation aborted");
                    return;
                }
            }

            SetStatus("Merging...", statusBlueColor);
            string path = PathToSave.Text + "\\" + SaveImageName.Text;

            if (Directory.Exists(PathToSave.Text))
            {
                var result = await correct.DoMergeAsync();
                SetStatus("Saving...", statusBlueColor);
                await result.SaveAsync(path);
            }

            else
                MessageBox.Show("Save path is not valid!\n" +
                    "Check if the path is correct.");
            
            SetStatus("Done!", statusGreenColor);
            await Task.Delay(5000);
            SetStatus();
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
