using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ImageMagick;
using System.Threading.Tasks;
using System;

namespace TextureMerge
{
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush statusBlueColor = new SolidColorBrush(Color.FromRgb(51, 150, 226));
        private static readonly SolidColorBrush statusGreenColor = new SolidColorBrush(Color.FromRgb(51, 226, 110));
        private readonly Merge merge = new Merge();
        private bool hasSetupPath = false;
        private bool hasEditedPath = false;

        public MainWindow()
        {
            InitializeComponent();
            PathToSave.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            hasEditedPath = false;
            MagickNET.Initialize();
        }

        private void SetStatus() => SetStatus("", statusBlueColor);

        private void SetStatus(string message, SolidColorBrush color)
        {
            StatusLabel.Foreground = color;
            StatusLabel.Content = message;
        }

        private string GetImagePath()
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = PathToSave.Text,
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
                OverwritePrompt = false,
                Title = "Select an image file",
                Filter =
                "Portable Network Graphics (*.png)|*.png|" +
                "Joint Photographic Experts Group (*.jpg;*.jpeg;*.jpe)|*.jpg;*.jpeg;*.jpe|" +
                "Microsoft Windows Bitmap Format (*.bmp;*.dib)|*.bmp;*.dib|" +
                "Tagged Image File Format (*.tif;*.tiff)|*.tif;*.tiff|" +
                "Targa (*.tga)|*.tga|" +
                "Graphics Interchange Format (*.gif)|*.gif|" +
                "All files|*.*"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                PathToSave.Text = Path.GetDirectoryName(saveFileDialog.FileName);
                SaveImageName.Text = Path.GetFileName(saveFileDialog.FileName);
                hasSetupPath = true;
                hasEditedPath = false;
            }
        }

        private async Task ButtonLoad(Image WPFElement, Label label, Channel channel, Channel sourceChannel, string path = null)
        {
            if (path is null)
                path = GetImagePath();

            if (path != string.Empty)
            {
                if (!hasEditedPath && !hasSetupPath)
                {
                    PathToSave.Text = Path.GetDirectoryName(path);
                    hasEditedPath = false;
                }

                var tmpLabelContent = label.Content;
                label.Content = "Loading...";
                SetStatus("Loading...", statusBlueColor);
                WPFElement.Source = await merge.LoadChannelAsync(path, channel, sourceChannel);
                SetStatus();
                label.Content = tmpLabelContent;
                label.Visibility = Visibility.Hidden;
            }
        }

        private async void ButtonLoadR(object sender, RoutedEventArgs e)
        {
            await ButtonLoad(RedCh, redNoDataLabel, Channel.Red, Channel.Red);
            ShowRedSourceGrid();
        }

        private async void ButtonLoadG(object sender, RoutedEventArgs e)
        {
            await ButtonLoad(GreenCh, greenNoDataLabel, Channel.Green, Channel.Green);
            ShowGreenSourceGrid();
        }

        private async void ButtonLoadB(object sender, RoutedEventArgs e)
        {
            await ButtonLoad(BlueCh, blueNoDataLabel, Channel.Blue, Channel.Blue);
            ShowBlueSourceGrid();
        }

        private async void ButtonLoadA(object sender, RoutedEventArgs e)
        {
            await ButtonLoad(AlphaCh, alphaNoDataLabel, Channel.Alpha, Channel.Red);
            ShowAlphaSourceGrid();
        }

        private void ButtonClearR(object sender, RoutedEventArgs e)
        {
            merge.Clear(Channel.Red);
            RedCh.Source = null;
            redNoDataLabel.Visibility = Visibility.Visible;
            srcGridGsR.Visibility = Visibility.Hidden;
            srcGridCR.Visibility = Visibility.Hidden;
        }

        private void ButtonClearG(object sender, RoutedEventArgs e)
        {
            merge.Clear(Channel.Green);
            GreenCh.Source = null;
            greenNoDataLabel.Visibility = Visibility.Visible;
            srcGridGsG.Visibility = Visibility.Hidden;
            srcGridCG.Visibility = Visibility.Hidden;
        }

        private void ButtonClearB(object sender, RoutedEventArgs e)
        {
            merge.Clear(Channel.Blue);
            BlueCh.Source = null;
            blueNoDataLabel.Visibility = Visibility.Visible;
            srcGridGsB.Visibility = Visibility.Hidden;
            srcGridCB.Visibility = Visibility.Hidden;
        }

        private void ButtonClearA(object sender, RoutedEventArgs e)
        {
            merge.Clear(Channel.Alpha);
            AlphaCh.Source = null;
            alphaNoDataLabel.Visibility = Visibility.Visible;
            srcGridGsA.Visibility = Visibility.Hidden;
            srcGridCA.Visibility = Visibility.Hidden;
        }

        private void ButtonBrowse(object sender, RoutedEventArgs e)
        {
            SetSaveImagePath();
        }

        private async void ButtonMerge(object sender, RoutedEventArgs e)
        {
            // TODO It looks weird. Maybe simplify this to one variable.
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

            if (Path.GetExtension(SaveImageName.Text) is null || Path.GetExtension(SaveImageName.Text) == "")
            {
                if (
                MessageBox.Show("File don't have an extension!\n" +
                                "Do you want to continue?",
                                "No extension",
                                MessageBoxButton.YesNo)
                != MessageBoxResult.Yes)
                    return;
            }

            string path = PathToSave.Text + "\\" + SaveImageName.Text;
            if (File.Exists(path))
            {
                if (MessageBox.Show("File already exist!\n" +
                    "Do you want to overwrite it?",
                    "File already exist",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
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
                    correct = await merge.ResizeAsync(resizeDialog.NewWidth, resizeDialog.NewHeight,
                        resizeDialog.DoStretch.IsChecked == true,
                        GetDefaultFillColor(dummyColorSwap));
                    SetStatus();
                }
                else
                {
                    MessageBox.Show("Operation aborted");
                    return;
                }
            }
            else if (width == 0 || height == 0)
            {
                MessageBox.Show("No images loaded", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int newDepth = -1;
            if (!correct.IsDepthSame())
            {
                var depthDialog = new Depth()
                {
                    Owner = this
                };
                if (depthDialog.ShowDialog() == true)
                {
                    newDepth = depthDialog.NewDepth;
                }
                else
                {
                    MessageBox.Show("Operation aborted");
                    return;
                }
            }

            SetStatus("Merging...", statusBlueColor);

            if (Directory.Exists(PathToSave.Text))
            {
                var result = await correct.DoMergeAsync(GetDefaultFillColor(dummyColorSwap), newDepth);
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

        private MagickColor GetDefaultFillColor(bool useWhite) =>
            useWhite ? new MagickColor(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue) : new MagickColor(0, 0, 0);

        private void PathToSaveChanged(object sender, TextChangedEventArgs e)
        {
            hasEditedPath = true;
            hasSetupPath = false;
        }

        private void SaveImageNameChanged(object sender, TextChangedEventArgs e)
        {
            hasEditedPath = true;
        }

        private async void RedDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                await ButtonLoad(RedCh, redNoDataLabel, Channel.Red, Channel.Red, files[0]);
                ShowRedSourceGrid();
            }
        }

        private async void GreenDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                await ButtonLoad(GreenCh, greenNoDataLabel, Channel.Green, Channel.Green, files[0]);
                ShowGreenSourceGrid();
            }
        }

        private async void BlueDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                await ButtonLoad(BlueCh, blueNoDataLabel, Channel.Blue, Channel.Blue, files[0]);
                ShowBlueSourceGrid();
            }
        }

        private async void AlphaDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                await ButtonLoad(AlphaCh, alphaNoDataLabel, Channel.Alpha, Channel.Red, files[0]);
                ShowAlphaSourceGrid();
            }
        }

        private void ShowRedSourceGrid()
        {
            if (merge.IsGrayScale(Channel.Red))
            {
                srcGridGsR.Visibility = Visibility.Visible;
                srcGridCR.Visibility = Visibility.Hidden;
            }
            else
            {
                srcGridGsR.Visibility = Visibility.Hidden;
                srcGridCR.Visibility = Visibility.Visible;
            }
        }

        private void ShowGreenSourceGrid()
        {
            if (merge.IsGrayScale(Channel.Green))
            {
                srcGridGsG.Visibility = Visibility.Visible;
                srcGridCG.Visibility = Visibility.Hidden;
            }
            else
            {
                srcGridGsG.Visibility = Visibility.Hidden;
                srcGridCG.Visibility = Visibility.Visible;
            }
        }

        private void ShowBlueSourceGrid()
        {
            if (merge.IsGrayScale(Channel.Blue))
            {
                srcGridGsB.Visibility = Visibility.Visible;
                srcGridCB.Visibility = Visibility.Hidden;
            }
            else
            {
                srcGridGsB.Visibility = Visibility.Hidden;
                srcGridCB.Visibility = Visibility.Visible;
            }
        }

        private void ShowAlphaSourceGrid()
        {
            if (merge.IsGrayScale(Channel.Alpha))
            {
                srcGridGsA.Visibility = Visibility.Visible;
                srcGridCA.Visibility = Visibility.Hidden;
            }
            else
            {
                srcGridGsA.Visibility = Visibility.Hidden;
                srcGridCA.Visibility = Visibility.Visible;
            }
        }
        // TODO make constants for colors
        private void SrcRR(object sender, RoutedEventArgs e)
        {
            srcRR.Background = new SolidColorBrush(Color.FromRgb(204, 0, 0));
            srcRG.Background = new SolidColorBrush(Color.FromRgb(0, 68, 0));
            srcRB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 68));
            RedCh.Source = merge.SetChannelSource(Channel.Red, Channel.Red);
        }

        private void SrcRG(object sender, RoutedEventArgs e)
        {
            srcRR.Background = new SolidColorBrush(Color.FromRgb(68, 0, 0));
            srcRG.Background = new SolidColorBrush(Color.FromRgb(0, 204, 0));
            srcRB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 68));
            RedCh.Source = merge.SetChannelSource(Channel.Red, Channel.Green);
        }

        private void SrcRB(object sender, RoutedEventArgs e)
        {
            srcRR.Background = new SolidColorBrush(Color.FromRgb(68, 0, 0));
            srcRG.Background = new SolidColorBrush(Color.FromRgb(0, 68, 0));
            srcRB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 204));
            RedCh.Source = merge.SetChannelSource(Channel.Red, Channel.Blue);
        }

        private void SrcGR(object sender, RoutedEventArgs e)
        {
            srcGR.Background = new SolidColorBrush(Color.FromRgb(204, 0, 0));
            srcGG.Background = new SolidColorBrush(Color.FromRgb(0, 68, 0));
            srcGB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 68));
            GreenCh.Source = merge.SetChannelSource(Channel.Green, Channel.Red);
        }

        private void SrcGG(object sender, RoutedEventArgs e)
        {
            srcGR.Background = new SolidColorBrush(Color.FromRgb(68, 0, 0));
            srcGG.Background = new SolidColorBrush(Color.FromRgb(0, 204, 0));
            srcGB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 68));
            GreenCh.Source = merge.SetChannelSource(Channel.Green, Channel.Green);
        }

        private void SrcGB(object sender, RoutedEventArgs e)
        {
            srcGR.Background = new SolidColorBrush(Color.FromRgb(68, 0, 0));
            srcGG.Background = new SolidColorBrush(Color.FromRgb(0, 68, 0));
            srcGB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 204));
            GreenCh.Source = merge.SetChannelSource(Channel.Green, Channel.Blue);
        }

        private void SrcBR(object sender, RoutedEventArgs e)
        {
            srcBR.Background = new SolidColorBrush(Color.FromRgb(204, 0, 0));
            srcBG.Background = new SolidColorBrush(Color.FromRgb(0, 68, 0));
            srcBB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 68));
            BlueCh.Source = merge.SetChannelSource(Channel.Blue, Channel.Red);
        }

        private void SrcBG(object sender, RoutedEventArgs e)
        {
            srcBR.Background = new SolidColorBrush(Color.FromRgb(68, 0, 0));
            srcBG.Background = new SolidColorBrush(Color.FromRgb(0, 204, 0));
            srcBB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 68));
            BlueCh.Source = merge.SetChannelSource(Channel.Blue, Channel.Green);
        }

        private void SrcBB(object sender, RoutedEventArgs e)
        {
            srcBR.Background = new SolidColorBrush(Color.FromRgb(68, 0, 0));
            srcBG.Background = new SolidColorBrush(Color.FromRgb(0, 68, 0));
            srcBB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 204));
            BlueCh.Source = merge.SetChannelSource(Channel.Blue, Channel.Blue);
        }

        private void SrcAR(object sender, RoutedEventArgs e)
        {
            srcAR.Background = new SolidColorBrush(Color.FromRgb(204, 0, 0));
            srcAG.Background = new SolidColorBrush(Color.FromRgb(0, 68, 0));
            srcAB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 68));
            AlphaCh.Source = merge.SetChannelSource(Channel.Alpha, Channel.Red);
        }

        private void SrcAG(object sender, RoutedEventArgs e)
        {
            srcAR.Background = new SolidColorBrush(Color.FromRgb(68, 0, 0));
            srcAG.Background = new SolidColorBrush(Color.FromRgb(0, 204, 0));
            srcAB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 68));
            AlphaCh.Source = merge.SetChannelSource(Channel.Alpha, Channel.Green);
        }

        private void SrcAB(object sender, RoutedEventArgs e)
        {
            srcAR.Background = new SolidColorBrush(Color.FromRgb(68, 0, 0));
            srcAG.Background = new SolidColorBrush(Color.FromRgb(0, 68, 0));
            srcAB.Background = new SolidColorBrush(Color.FromRgb(0, 0, 204));
            AlphaCh.Source = merge.SetChannelSource(Channel.Alpha, Channel.Blue);
        }
    }
}
