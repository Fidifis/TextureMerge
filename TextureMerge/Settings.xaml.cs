using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TextureMerge
{
    /// <summary>
    /// Interakční logika pro Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Config SavedConfig { get; private set; }

        private Config config;
        public Settings(Config config)
        {
            this.config = config;
            InitializeComponent();
            GenerateVersionString();

            CustomPathToSaveBox.Text = config.PathToSave;
            CustomSaveImageNameBox.Text = config.SaveImageName;
            CheckForUpdates.IsChecked = config.CheckForUpdates;
            UseLastWindowSize.IsChecked = config.UseLastWindowSize;
            if (config.UseLastPathToSave)
                UseLastPathToSave.IsChecked = true;
            else CustomPathToSave.IsChecked = true;
            if (config.UseLastSaveImageName)
                UseLastSaveImageName.IsChecked = true;
            else CustomSaveImageName.IsChecked = true;

            if (config.EnableSendTo)
            {
                EnableSendTo.IsChecked = true;
                EnableSendTo.Visibility = Visibility.Visible;
            }
        }

        private void GenerateVersionString()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            versionString.Content = "Version: v" + version;
        }

        private void CustomPathToSaveChanged(object sender, TextChangedEventArgs e)
        {
            UseLastPathToSave.IsChecked = false;
            CustomPathToSave.IsChecked = true;
        }

        private void CustomSaveImageNameChanged(object sender, TextChangedEventArgs e)
        {
            UseLastSaveImageName.IsChecked = false;
            CustomSaveImageName.IsChecked = true;
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveButton(object sender, RoutedEventArgs e)
        {
            SavedConfig = config.Copy();
            
            if (UseLastPathToSave.IsChecked == true)
                SavedConfig.UseLastPathToSave = true;
            else
            {
                SavedConfig.UseLastPathToSave = false;
                SavedConfig.PathToSave = CustomPathToSaveBox.Text;
            }

            if (UseLastSaveImageName.IsChecked == true)
                SavedConfig.UseLastSaveImageName = true;
            else
            {
                SavedConfig.UseLastSaveImageName = false;
                SavedConfig.SaveImageName = CustomSaveImageNameBox.Text;
            }

            if (EnableSendTo.IsChecked == true)
                SavedConfig.EnableSendTo = true;
            else
            {
                SavedConfig.EnableSendTo = false;
                DeleteFromSendTo();
            }

            SavedConfig.CheckForUpdates = CheckForUpdates.IsChecked == true;
            SavedConfig.UseLastWindowSize = UseLastWindowSize.IsChecked == true;
            

            DialogResult = true;
            Close();
        }

        private void CheckUpdatesButton(object sender, RoutedEventArgs e)
        {
            UpdateAvailable.ShowUpdateIfAvailable(forced: true);
        }

        private void ReportButton(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Fidifis/TextureMerge/issues");
        }

        private void DeleteFromSendTo()
        {
            try
            {
                string sendtoPath = Environment.GetFolderPath(Environment.SpecialFolder.SendTo);
                string filelnk = sendtoPath + "Texture Merge.lnk";
                if (File.Exists(filelnk))
                    File.Delete(filelnk);
                else
                    throw new FileNotFoundException("No shortcut file found. It could be renamed or deleted.\n" +
                        "If you want to manually remove it from context menu, go to SendTo folder and remove the shortcut\n" +
                        sendtoPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to remove from context menu\nMessage:\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
