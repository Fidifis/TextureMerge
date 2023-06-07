using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ImageMagick;
using System;

namespace TextureMerge
{
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush statusBlueColor = new SolidColorBrush(Color.FromRgb(51, 150, 226));
        private static readonly SolidColorBrush statusGreenColor = new SolidColorBrush(Color.FromRgb(51, 226, 110));
        private readonly Merge merge = new Merge();
        private bool hasEditedPath = false;
        private Color defaultColor;

        private const string LOAD_TEXT = "Load";
        private const string CLEAR_TEXT = "Clear";

        public MainWindow()
        {
            InitializeComponent();
            MapResources();

            try
            {
                MagickNET.Initialize();
            }
            catch (Exception ex)
            {
                MessageDialog.Show("Failed to initialize MagickNET library." + Environment.NewLine + ex.Message,
                "Error", MessageDialog.Type.Error);
            }

            ToolTipService.ShowDurationProperty.OverrideMetadata(
                typeof(DependencyObject), new FrameworkPropertyMetadata(60000));

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
            defaultColor = Config.Current.DefaultColorInt.ToColor();
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
            Config.Current.DefaultColorInt = defaultColor.ToInt();

            if (Config.Current.UseLastPathToSave)
                Config.Current.PathToSave = PathToSave.Text;

            if (Config.Current.UseLastSaveImageName)
                Config.Current.SaveImageName = SaveImageName.Text;
        }

        private void LoadArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 0)
            {
                for (int i = 1; i < args.Length; ++i)
                {
                    switch (i - 1)
                    {
                        case 0:
                            LoadToChannelAsync(Channel.Red, args[i]);
                            break;
                        case 1:
                            LoadToChannelAsync(Channel.Green, args[i]);
                            break;
                        case 2:
                            LoadToChannelAsync(Channel.Blue, args[i]);
                            break;
                        case 3:
                            LoadToChannelAsync(Channel.Alpha, args[i]);
                            break;
                    }
                }
            }
        }

        private void ButtonLoadR(object sender, RoutedEventArgs e)
        {
            ButtonLoad(Channel.Red);
        }

        private void ButtonLoadG(object sender, RoutedEventArgs e)
        {
            ButtonLoad(Channel.Green);
        }

        private void ButtonLoadB(object sender, RoutedEventArgs e)
        {
            ButtonLoad(Channel.Blue);
        }

        private void ButtonLoadA(object sender, RoutedEventArgs e)
        {
            ButtonLoad(Channel.Alpha);
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

        private void PathToSaveChanged(object sender, TextChangedEventArgs e)
        {
            hasEditedPath = true;
        }

        private void SaveImageNameChanged(object sender, TextChangedEventArgs e)
        {
            hasEditedPath = true;
        }

        private void RedDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadToChannelAsync(Channel.Red, files[0]);
            }
        }

        private void GreenDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadToChannelAsync(Channel.Green, files[0]);
            }
        }

        private void BlueDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadToChannelAsync(Channel.Blue, files[0]);
            }
        }

        private void AlphaDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadToChannelAsync(Channel.Alpha, files[0]);
            }
        }

        private async void SrcRR(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Red, Channel.Red);
            RedCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Red));
            UpdateSourceGrid(Channel.Red);
        }

        private async void SrcRG(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Red, Channel.Green);
            RedCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Red));
            UpdateSourceGrid(Channel.Red);
        }

        private async void SrcRB(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Red, Channel.Blue);
            RedCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Red));
            UpdateSourceGrid(Channel.Red);
        }

        private async void SrcGR(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Green, Channel.Red);
            GreenCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Green));
            UpdateSourceGrid(Channel.Green);
        }

        private async void SrcGG(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Green, Channel.Green);
            GreenCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Green));
            UpdateSourceGrid(Channel.Green);
        }

        private async void SrcGB(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Green, Channel.Blue);
            GreenCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Green));
            UpdateSourceGrid(Channel.Green);
        }

        private async void SrcBR(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Blue, Channel.Red);
            BlueCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Blue));
            UpdateSourceGrid(Channel.Blue);
        }

        private async void SrcBG(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Blue, Channel.Green);
            BlueCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Blue));
            UpdateSourceGrid(Channel.Blue);
        }

        private async void SrcBB(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Blue, Channel.Blue);
            BlueCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Blue));
            UpdateSourceGrid(Channel.Blue);
        }

        private async void SrcAR(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Alpha, Channel.Red);
            AlphaCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Alpha));
            UpdateSourceGrid(Channel.Alpha);
        }

        private async void SrcAG(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Alpha, Channel.Green);
            AlphaCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Alpha));
            UpdateSourceGrid(Channel.Alpha);
        }

        private async void SrcAB(object sender, RoutedEventArgs e)
        {
            merge.SetChannelSource(Channel.Alpha, Channel.Blue);
            AlphaCh.SetImageThumbnail(await merge.GetChannelThumbnailAsync(Channel.Alpha));
            UpdateSourceGrid(Channel.Alpha);
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
            RedCh.SetImageThumbnail(merge.GetChannelThumbnail(Channel.Red));
            GreenCh.SetImageThumbnail(merge.GetChannelThumbnail(Channel.Green));
            UpdateSourceGrid(Channel.Red);
            UpdateSourceGrid(Channel.Green);
        }

        private void SwapGB(object sender, RoutedEventArgs e)
        {
            merge.Swap(Channel.Green, Channel.Blue);
            GreenCh.SetImageThumbnail(merge.GetChannelThumbnail(Channel.Green));
            BlueCh.SetImageThumbnail(merge.GetChannelThumbnail(Channel.Blue));
            UpdateSourceGrid(Channel.Green);
            UpdateSourceGrid(Channel.Blue);
        }

        private void SwapBA(object sender, RoutedEventArgs e)
        {
            merge.Swap(Channel.Blue, Channel.Alpha);
            BlueCh.SetImageThumbnail(merge.GetChannelThumbnail(Channel.Blue));
            AlphaCh.SetImageThumbnail(merge.GetChannelThumbnail(Channel.Alpha));
            UpdateSourceGrid(Channel.Blue);
            UpdateSourceGrid(Channel.Alpha);
        }

        private void LoadWholeImage(object sender, RoutedEventArgs e)
        {
            if (AskForImagePath(out string path) == true)
            {
                LoadToChannelAsync(Channel.Red, path);
                LoadToChannelAsync(Channel.Green, path);
                LoadToChannelAsync(Channel.Blue, path);
            }
        }

        private void RedCh_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Using only thumbnail to view image. Call merge.GetImage instead for full resolution.
            // For now we use thumbnail for performance, because
            // the merge.GetImage takes a long time to convert to .NET ImageSource.
            new ViewImage(merge.GetChannelThumbnail(Channel.Red), "Red", merge.GetSourceChannel(Channel.Red).ToString()).Show();
        }

        private void GreenCh_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new ViewImage(merge.GetChannelThumbnail(Channel.Green), "Green", merge.GetSourceChannel(Channel.Green).ToString()).Show();
        }

        private void BlueCh_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new ViewImage(merge.GetChannelThumbnail(Channel.Blue), "Blue", merge.GetSourceChannel(Channel.Blue).ToString()).Show();
        }

        private void AlphaCh_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new ViewImage(merge.GetChannelThumbnail(Channel.Alpha), "Alpha", merge.GetSourceChannel(Channel.Alpha).ToString()).Show();
        }

        private void RAInvertSelected(object sender, RoutedEventArgs e)
        {
            InvertImageAsync(Channel.Red);
        }

        private void RALevelsSelected(object sender, RoutedEventArgs e)
        {
            AutoLevelImageAsync(Channel.Red);
        }

        private void GAInvertSelected(object sender, RoutedEventArgs e)
        {
            InvertImageAsync(Channel.Green);
        }

        private void GALevelsSelected(object sender, RoutedEventArgs e)
        {
            AutoLevelImageAsync(Channel.Green);
        }

        private void BAInvertSelected(object sender, RoutedEventArgs e)
        {
            InvertImageAsync(Channel.Blue);
        }

        private void BALevelsSelected(object sender, RoutedEventArgs e)
        {
            AutoLevelImageAsync(Channel.Blue);
        }

        private void AAInvertSelected(object sender, RoutedEventArgs e)
        {
            InvertImageAsync(Channel.Alpha);
        }

        private void AALevelsSelected(object sender, RoutedEventArgs e)
        {
            AutoLevelImageAsync(Channel.Alpha);
        }

        private void RActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RActions.SelectedIndex = 0;
        }

        private void GActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GActions.SelectedIndex = 0;
        }

        private void BActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BActions.SelectedIndex = 0;
        }

        private void AActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AActions.SelectedIndex = 0;
        }
    }
}
