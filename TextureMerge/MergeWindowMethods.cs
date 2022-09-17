﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private async Task<bool> ButtonLoad(Image WPFElement, Label label, Channel channel, Channel sourceChannel, string path = null)
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
                return true;
            }
            else return false;
        }

        private void RefreshState(Channel channel)
        {
            switch (channel)
            {
                case Channel.Red:
                    if (merge.isEmpty(Channel.Red))
                    {
                        RedCh.Source = null;
                        redNoDataLabel.Visibility = Visibility.Visible;
                        srcGridGsR.Visibility = Visibility.Hidden;
                        srcGridCR.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        redNoDataLabel.Visibility = Visibility.Hidden;
                        ShowRedSourceGrid();
                        switch (merge.GetSourceChannel(Channel.Red))
                        {
                            // TODO Not optimal to call this methods, because they call SetSourceChannel
                            case Channel.Red:
                                SrcRR(null, null);
                                break;
                            case Channel.Green:
                                SrcRG(null, null);
                                break;
                            case Channel.Blue:
                                SrcRB(null, null);
                                break;
                            default: throw new ArgumentException("Invalid channel");
                        }
                    }
                    break;
                case Channel.Green:
                    if (merge.isEmpty(Channel.Green))
                    {
                        GreenCh.Source = null;
                        greenNoDataLabel.Visibility = Visibility.Visible;
                        srcGridGsG.Visibility = Visibility.Hidden;
                        srcGridCG.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        greenNoDataLabel.Visibility = Visibility.Hidden;
                        ShowGreenSourceGrid();
                        switch (merge.GetSourceChannel(Channel.Green))
                        {
                            // TODO Not optimal to call this methods, because they call SetSourceChannel
                            case Channel.Red:
                                SrcGR(null, null);
                                break;
                            case Channel.Green:
                                SrcGG(null, null);
                                break;
                            case Channel.Blue:
                                SrcGB(null, null);
                                break;
                            default: throw new ArgumentException("Invalid channel");
                        }
                    }
                    break;
                case Channel.Blue:
                    if (merge.isEmpty(Channel.Blue))
                    {
                        BlueCh.Source = null;
                        blueNoDataLabel.Visibility = Visibility.Visible;
                        srcGridGsB.Visibility = Visibility.Hidden;
                        srcGridCB.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        blueNoDataLabel.Visibility = Visibility.Hidden;
                        ShowBlueSourceGrid();
                        switch (merge.GetSourceChannel(Channel.Blue))
                        {
                            // TODO Not optimal to call this methods, because they call SetSourceChannel
                            case Channel.Red:
                                SrcBR(null, null);
                                break;
                            case Channel.Green:
                                SrcBG(null, null);
                                break;
                            case Channel.Blue:
                                SrcBB(null, null);
                                break;
                            default: throw new ArgumentException("Invalid channel");
                        }
                    }
                    break;
                case Channel.Alpha:
                    if (merge.isEmpty(Channel.Alpha))
                    {
                        AlphaCh.Source = null;
                        alphaNoDataLabel.Visibility = Visibility.Visible;
                        srcGridGsA.Visibility = Visibility.Hidden;
                        srcGridCA.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        alphaNoDataLabel.Visibility = Visibility.Hidden;
                        ShowAlphaSourceGrid();
                        switch (merge.GetSourceChannel(Channel.Alpha))
                        {
                            // TODO Not optimal to call this methods, because they call SetSourceChannel
                            case Channel.Red:
                                SrcAR(null, null);
                                break;
                            case Channel.Green:
                                SrcAG(null, null);
                                break;
                            case Channel.Blue:
                                SrcAB(null, null);
                                break;
                            default: throw new ArgumentException("Invalid channel");
                        }
                    }
                    break;
            }
        }
    }
}
