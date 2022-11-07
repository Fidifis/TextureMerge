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
        private async void ButtonMerge(object sender, RoutedEventArgs e)
        {
            // TODO It looks weird. Maybe simplify this to one variable.
            if (!(hasEditedPath && Directory.Exists(PathToSave.Text)) &&
                !hasSetupPath)
            {
                SetSaveImagePath();
                if (!hasSetupPath)
                {
                    MessageDialog.Show("Operation aborted");
                    return;
                }
            }

            switch (Path.GetExtension(SaveImageName.Text))
            {
                case null:
                case "":
                    if (MessageDialog.Show("File don't have an extension!\n" +
                                        "Do you want to continue?",
                                        "No extension",
                                        MessageDialog.Type.Warning,
                                        MessageDialog.Buttons.YesNo)
                            != true)
                        return;
                    break;
                case ".jpeg":
                case ".jpg":
                    if (!merge.IsEmpty(Channel.Alpha))
                    {
                        MessageDialog.Show("This image format do not support alpha channel.", "Error", MessageDialog.Type.Error);
                        return;
                    }
                    break;
            }

            string path = PathToSave.Text + "\\" + SaveImageName.Text;
            if (File.Exists(path))
            {
                if (MessageDialog.Show("File already exist!\n" +
                    "Do you want to overwrite it?",
                    "File already exist",
                    MessageDialog.Type.Warning,
                    MessageDialog.Buttons.YesNo) != true)
                {
                    return;
                }
            }

            // TODO possible memory leak
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
                        ColorToMagick(defaultColor));
                    SetStatus();
                }
                else
                {
                    MessageDialog.Show("Operation aborted");
                    return;
                }
            }
            else if (width == 0 || height == 0)
            {
                MessageDialog.Show("No images loaded", type: MessageDialog.Type.Error);
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
                    MessageDialog.Show("Operation aborted");
                    return;
                }
            }

            SetStatus("Merging...", statusBlueColor);

            if (Directory.Exists(PathToSave.Text))
            {
                var result = await correct.DoMergeAsync(ColorToMagick(defaultColor), newDepth);
                
                SetStatus("Saving...", statusBlueColor);
                try
                {
                    await result.SaveAsync(path);
                }
                catch (Exception ex)
                {
                    MessageDialog.Show("Failed to save image." + Environment.NewLine + ex.Message,
                        "Error", MessageDialog.Type.Error);
                }
                
            }

            else
                MessageDialog.Show("Save path is not valid!\n" +
                    "Check if the path is correct.",
                    type: MessageDialog.Type.Error);


            SetStatus("Done!", statusGreenColor);
            await Task.Delay(5000);
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
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*" //TODO: Add more formats
            };
            bool? r = openFileDialog.ShowDialog();
            path = openFileDialog.FileName;
            return r;
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

        private async void LoadToChannelAsync(Channel channel, string path)
        {
            if (path == null || path.Length == 0)
            {
                throw new ArgumentException("Invalid path");
            }

            int ichannel = (int)channel;
            var label = mapper.slots[ichannel].label;
            var image = mapper.slots[ichannel].image;

            if (!hasEditedPath && !hasSetupPath)
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
                image.SetImageThumbnail (await merge.LoadChannelAsync(path, channel, sourceChannel));
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
            var image = mapper.slots[ichannel].image;
            var label = mapper.slots[ichannel].label;
            var grayscaleGrid = mapper.slots[ichannel].grayscaleSourceGrid;
            var colorGrid = mapper.slots[ichannel].colorSourceGrid;

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
