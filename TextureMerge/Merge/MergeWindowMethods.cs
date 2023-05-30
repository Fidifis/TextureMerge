using ImageMagick;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TextureMerge
{
    public partial class MainWindow : Window
    {
        private bool CheckSaveImagePath() => !hasEditedPath || !Directory.Exists(PathToSave.Text);

        private string CheckFileExtension(string filePath)
        {
            switch (Path.GetExtension(filePath))
            {
                case null:
                case "":
                    return "File don't have an extension!";
                case ".jpeg":
                case ".jpg":
                    if (!merge.IsEmpty(Channel.Alpha))
                    {
                        return "This image format do not support alpha channel.";
                    }
                    break;
            }
            return null;
        }

        private async Task<Merge> ResizeMergeSetAsync(Merge merge, int width, int height)
        {
            var resizeDialog = new Resize(width, height)
            {
                Owner = this
            };
            if (resizeDialog.ShowDialog() == true)
            {
                try
                {
                    SetStatus("Resizeing...", statusBlueColor);
                    return await merge.ResizeAsync(resizeDialog.NewWidth, resizeDialog.NewHeight,
                        resizeDialog.DoStretch.IsChecked == true,
                        ColorToMagick(defaultColor));
                }
                catch (ArgumentException ex)
                {
                    MessageDialog.Show("Failed to resize." + Environment.NewLine + ex.Message,
                    "Error", MessageDialog.Type.Error);
                    return null;
                }
                finally
                {
                    SetStatus();
                }
            }
            else
            {
                MessageDialog.Show("Operation aborted");
                return null;
            }
        }

        private int GetDepth(Merge merge)
        {
            if (!merge.IsDepthSame())
            {
                var depthDialog = new Depth()
                {
                    Owner = this
                };
                if (depthDialog.ShowDialog() == true)
                {
                    return depthDialog.NewDepth;
                }
                else
                {
                    return -2;
                }
            }
            return -1;
        }

        private async Task<bool> SaveMerge(Merge merge, int depth, string path)
        {
            if (Directory.Exists(PathToSave.Text))
            {
                try
                {
                    SetStatus("Merging...", statusBlueColor);
                    var result = await merge.DoMergeAsync(ColorToMagick(defaultColor), depth);

                    SetStatus("Saving...", statusBlueColor);
                    await result.SaveAsync(path);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageDialog.Show("Failed to save image." + Environment.NewLine + ex.Message,
                        "Error", MessageDialog.Type.Error);
                    return false;
                }
            }

            else
            {
                MessageDialog.Show("Save path is not valid!\n" +
                    "Check if the path is correct.",
                    type: MessageDialog.Type.Error);
                return false;
            }
        }

        private async void ButtonMerge(object sender, RoutedEventArgs e)
        {
            bool isResolutionValid = merge.CheckResolution(out int width, out int height);
            if (width == 0 || height == 0)
            {
                MessageDialog.Show("No images loaded", type: MessageDialog.Type.Error);
                return;
            }

            if (CheckSaveImagePath() && !SetSaveImagePath())
            {
                MessageDialog.Show("Operation aborted");
                return;
            }

            string error;
            if ((error = CheckFileExtension(SaveImageName.Text)) != null)
            {
                MessageDialog.Show(error, "Error", MessageDialog.Type.Error);
                return;
            }

            string path = Path.Combine(PathToSave.Text, SaveImageName.Text);
            if (File.Exists(path) &&
                MessageDialog.Show("File already exist!\n" +
                    "Do you want to overwrite it?",
                    "File already exist",
                    MessageDialog.Type.Warning,
                    MessageDialog.Buttons.YesNo) != true)
            {
                return;
            }

            Merge correct = merge;
            if (!isResolutionValid)
            {
                correct = await ResizeMergeSetAsync(merge, width, height);
            }
            if (correct == null)
                return;

            int depth = GetDepth(correct);
            if (depth == -2)
            {
                MessageDialog.Show("Operation aborted");
                return;
            }

            if (await SaveMerge(correct, depth, path))
            {
                SetStatus("Done!", statusGreenColor);
                await Task.Delay(5000);
            }
            SetStatus();
        }
        
        private void SetStatus() => SetStatus("", statusBlueColor);

        private void SetStatus(string message, SolidColorBrush color)
        {
            StatusLabel.Foreground = color;
            StatusLabel.Content = message;
        }

        private bool? AskForImagePath(out string path)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = PathToSave.Text,
                Title = "Select an image file",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.tif;*.tga;*.webp;*.psd;*.dib;*.ico;*.svg)|" +
                                      "*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.tif;*.tga;*.webp;*.psd;*.dib;*.ico;*.svg|" +
                                      "All files (*.*)|*.*"
            };
            bool? r = openFileDialog.ShowDialog();
            path = openFileDialog.FileName;
            return r;
        }

        private bool SetSaveImagePath()
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
                return true;
            }
            return false;
        }

        private void ButtonLoad(Channel channel)
        {
            if (merge.IsEmpty(channel))
            {
                if (AskForImagePath(out string path) == true)
                {
                    LoadToChannelAsync(channel, path);
                }
            }
            else
            {
                ClearChannel(channel);
                mapper.slots[(int)channel].loadButton.Content = LOAD_TEXT;
            }
        }

        private void ClearChannel(Channel channel)
        {
            int ichannel = (int)channel;
            merge.Clear(channel);
            mapper.slots[ichannel].image.Source = null;
            mapper.slots[ichannel].image.ToolTip = null;
            mapper.slots[ichannel].label.Visibility = Visibility.Visible;
            mapper.slots[ichannel].grayscaleSourceGrid.Visibility = Visibility.Hidden;
            mapper.slots[ichannel].colorSourceGrid.Visibility = Visibility.Hidden;
        }

        private async void LoadToChannelAsync(Channel channel, string path)
        {
            if (path == null || path.Length == 0 || !File.Exists(path))
            {
                throw new ArgumentException("Invalid path");
            }

            int ichannel = (int)channel;
            var label = mapper.slots[ichannel].label;
            var image = mapper.slots[ichannel].image;

            if (!hasEditedPath || !Directory.Exists(PathToSave.Text))
            {
                PathToSave.Text = Path.GetDirectoryName(path);
                hasEditedPath = false;
            }

            var tmpLabelContent = label.Content;
            label.Content = "Loading...";
            SetStatus("Loading...", statusBlueColor);
            try
            {
                var sourceChannel = channel == Channel.Alpha ? Channel.Red : channel;
                await merge.LoadChannelAsync(path, channel, sourceChannel);
                image.SetImageThumbnail(await merge.GetChannelThumbnailAsync(channel));
                label.Visibility = Visibility.Hidden;
                UpdateSourceGrid(channel);
            }
            catch (Exception ex)
            {
                MessageDialog.Show("Failed to load image." + Environment.NewLine + ex.Message,
                    "Error", MessageDialog.Type.Error);
            }
            finally
            {
                SetStatus();
                label.Content = tmpLabelContent;
            }
        }

        private SolidColorBrush GetColorBrushFor(Channel channel, bool highlight)
        {
            byte value = highlight ? (byte)204 : (byte)68;
            switch (channel)
            {
                case Channel.Red:
                    return new SolidColorBrush(Color.FromRgb(value, 0, 0));
                case Channel.Green:
                    return new SolidColorBrush(Color.FromRgb(0, value, 0));
                case Channel.Blue:
                    return new SolidColorBrush(Color.FromRgb(0, 0, value));
                default:
                    throw new ArgumentException("Invalid channel. Only R, G, B are allowed");
            }
        }

        private void UpdateSourceGrid(Channel channel)
        {
            int ichannel = (int)channel;
            var load = mapper.slots[ichannel].loadButton;
            var label = mapper.slots[ichannel].label;
            var grayscaleGrid = mapper.slots[ichannel].grayscaleSourceGrid;
            var colorGrid = mapper.slots[ichannel].colorSourceGrid;
            var image = mapper.slots[ichannel].image;

            if (merge.IsEmpty(channel))
            {
                load.Content = LOAD_TEXT;
                label.Visibility = Visibility.Visible;
                grayscaleGrid.Visibility = Visibility.Hidden;
                colorGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                load.Content = CLEAR_TEXT;
                label.Visibility = Visibility.Hidden;
                image.ToolTip = merge.GetOriginFileName(channel);
                if (merge.IsGrayScale(channel))
                {
                    grayscaleGrid.Visibility = Visibility.Visible;
                    colorGrid.Visibility = Visibility.Hidden;
                }
                else
                {
                    grayscaleGrid.Visibility = Visibility.Hidden;
                    colorGrid.Visibility = Visibility.Visible;

                    int source = (int)merge.GetSourceChannel(channel);
                    if (source >= 3)
                        throw new InvalidOperationException("Source channel is something else than R, G, B. (Alpha cannot be source channel)");
                    for (int i = 0; i < 3; i++)
                    {
                        mapper.slots[ichannel].sourceChannelButton[i].Background = GetColorBrushFor((Channel)i, source == i);
                    }
                }
            }
        }

        private ushort ByteToUshortKeepRatio(byte value) =>
            (ushort)((value * ushort.MaxValue) / 255);


        private MagickColor ColorToMagick(Color color) =>
            new MagickColor(
                ByteToUshortKeepRatio(color.R),
                ByteToUshortKeepRatio(color.G),
                ByteToUshortKeepRatio(color.B));
    }
}
