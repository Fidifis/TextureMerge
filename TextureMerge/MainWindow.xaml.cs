using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ImageMagick;
using System;
using System.Threading.Tasks;

namespace TextureMerge
{
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush statusBlueColor = new SolidColorBrush(Color.FromRgb(51, 150, 226));
        private static readonly SolidColorBrush statusGreenColor = new SolidColorBrush(Color.FromRgb(51, 226, 110));
        private readonly Merge merge = new Merge();
        private bool hasSetupPath = false;
        private bool hasEditedPath = false;
        private Color defaultColor;

        public MainWindow()
        {
            InitializeComponent();
            MagickNET.Initialize();
            Config.Load();
            ApplyConfig();
            hasEditedPath = false;
            LoadArgs();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            UpdateAvailable.ShowUpdateIfAvailable();
        }

        private void ApplyConfig()
        {
            PathToSave.Text = Config.Current.PathToSave.Expand();
            SaveImageName.Text = Config.Current.SaveImageName;
            defaultColor = IntToColor(Config.Current.DefaultColorInt);
            DefaultColorRect.Fill = new SolidColorBrush(defaultColor);

            if (Config.Current.UseLastWindowSize &&
                Config.Current.WindowWidth > 0 &&
                Config.Current.WindowHeight > 0)
            {
                Width = Config.Current.WindowWidth;
                Height = Config.Current.WindowHeight;
            }
        }

        private void UpdateConfig()
        {
            Config.Current.WindowWidth = Convert.ToInt32(Width);
            Config.Current.WindowHeight = Convert.ToInt32(Height);
            Config.Current.DefaultColorInt = ColorToInt(defaultColor);

            if (Config.Current.UseLastPathToSave)
                Config.Current.PathToSave = PathToSave.Text;

            if (Config.Current.UseLastSaveImageName)
                Config.Current.SaveImageName = SaveImageName.Text;
        }

        private async void LoadArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 0)
            {
                for (int i = 1; i < args.Length; ++i)
                {
                    switch (i-1) {
                        case 0:
                        if (await ButtonLoad(RedCh, redNoDataLabel, Channel.Red, Channel.Red, args[i]))
                                ShowRedSourceGrid();
                            break;
                        case 1:
                            if (await ButtonLoad(GreenCh, greenNoDataLabel, Channel.Green, Channel.Green, args[i]))
                                ShowGreenSourceGrid();
                            break;
                        case 2:
                            if (await ButtonLoad(BlueCh, blueNoDataLabel, Channel.Blue, Channel.Blue, args[i]))
                                ShowBlueSourceGrid();
                            break;
                        case 3:
                            if (await ButtonLoad(AlphaCh, alphaNoDataLabel, Channel.Alpha, Channel.Red, args[i]))
                                ShowAlphaSourceGrid();
                            break;
                    }
                }
            }
        }

        private async void ButtonLoadR(object sender, RoutedEventArgs e)
        {
            if (await ButtonLoad(RedCh, redNoDataLabel, Channel.Red, Channel.Red))
                ShowRedSourceGrid();
        }

        private async void ButtonLoadG(object sender, RoutedEventArgs e)
        {
            if (await ButtonLoad(GreenCh, greenNoDataLabel, Channel.Green, Channel.Green))
                ShowGreenSourceGrid();
        }

        private async void ButtonLoadB(object sender, RoutedEventArgs e)
        {
            if (await ButtonLoad(BlueCh, blueNoDataLabel, Channel.Blue, Channel.Blue))
                ShowBlueSourceGrid();
        }

        private async void ButtonLoadA(object sender, RoutedEventArgs e)
        {
            if (await ButtonLoad(AlphaCh, alphaNoDataLabel, Channel.Alpha, Channel.Red))
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
        
        private void ChangeDefaultColor(object sender, RoutedEventArgs e)
        {
            ColorPicker picker = new ColorPicker(defaultColor);
            if (picker.ShowDialog() == true)
                DefaultColorRect.Fill = new SolidColorBrush(picker.PickedColor);

            defaultColor = picker.PickedColor;
        }

        private ushort ByteToUshortKeepRatio(byte value) => 
            (ushort)((value * ushort.MaxValue) / 255);


        private MagickColor ColorToMagick(Color color) =>
            new MagickColor(
                ByteToUshortKeepRatio(color.R),
                ByteToUshortKeepRatio(color.G),
                ByteToUshortKeepRatio(color.B));

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
                if (await ButtonLoad(RedCh, redNoDataLabel, Channel.Red, Channel.Red, files[0]))
                    ShowRedSourceGrid();
            }
        }

        private async void GreenDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (await ButtonLoad(GreenCh, greenNoDataLabel, Channel.Green, Channel.Green, files[0]))
                    ShowGreenSourceGrid();
            }
        }

        private async void BlueDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (await ButtonLoad(BlueCh, blueNoDataLabel, Channel.Blue, Channel.Blue, files[0]))
                    ShowBlueSourceGrid();
            }
        }

        private async void AlphaDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (await ButtonLoad(AlphaCh, alphaNoDataLabel, Channel.Alpha, Channel.Red, files[0]))
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

        private void MainWindowClosed(object sender, EventArgs e)
        {
            UpdateConfig();
            Config.Save();
        }

        private void SettingsButton(object sender, RoutedEventArgs e)
        {
            var settings = new Settings(Config.Current);
            settings.ShowDialog();
            if (settings.DialogResult == true && settings.SavedConfig != null)
            {
                Config.ApplyConfig(settings.SavedConfig);
                Config.Save();
            }
        }

        private void SwapRG(object sender, RoutedEventArgs e)
        {
            merge.Swap(Channel.Red, Channel.Green);
            RefreshState(Channel.Red);
            RefreshState(Channel.Green);
        }

        private void SwapGB(object sender, RoutedEventArgs e)
        {
            merge.Swap(Channel.Green, Channel.Blue);
            RefreshState(Channel.Green);
            RefreshState(Channel.Blue);
        }

        private void SwapBA(object sender, RoutedEventArgs e)
        {
            merge.Swap(Channel.Blue, Channel.Alpha);
            RefreshState(Channel.Blue);
            RefreshState(Channel.Alpha);
        }

        private async void LoadWholeImage(object sender, RoutedEventArgs e)
        {
            string path = GetImagePath();
            if (path != string.Empty)
            {
                var r = ButtonLoad(RedCh, redNoDataLabel, Channel.Red, Channel.Red, path);
                var g = ButtonLoad(GreenCh, greenNoDataLabel, Channel.Green, Channel.Green, path);
                var b = ButtonLoad(BlueCh, blueNoDataLabel, Channel.Blue, Channel.Blue, path);
                await r; await g; await b;
                ShowRedSourceGrid();
                ShowGreenSourceGrid();
                ShowBlueSourceGrid();
            }
        }

        private Color IntToColor(int color)
        {
            int r, g, b;
            r = (color & 0x00FF0000) >> 16;
            g = (color & 0x0000FF00) >> 8;
            b = (color & 0x000000FF);
            return new Color()
            {
                R = (byte)r,
                G = (byte)g,
                B = (byte)b,
                A = 255
            };
        }

        private int ColorToInt(Color color)
        {
            int r = color.R,
                g = color.G,
                b = color.B;

            return (r << 16) | (g << 8) | b;
        }
    }
}
