using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace TextureMerge
{
    internal class UpdateCheck
    {
        public static async void CheckForUpdateAsync(bool forced = false)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = "v" + fvi.FileVersion;
            string content;
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Headers.Add(HttpRequestHeader.UserAgent, "TextureMerge_webclient");
                    content = await client.DownloadStringTaskAsync(new Uri("https://api.github.com/repos/Fidifis/TextureMerge/releases/latest"));
                }
                catch (WebException ex)
                {
                    // Probbably no internet connection - dont show error message if not forcced
                    if (forced)
                    {
                        MessageDialog.Show("Error when trying to check latest version. Check your internet connection." +
                            Environment.NewLine + ex.Message,
                            "Error", MessageDialog.Type.Error);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    MessageDialog.Show("Error when trying to check latest version" + Environment.NewLine + ex.Message,
                        "Error", MessageDialog.Type.Error);
                    return;
                }
            }

            string latestVersion = GetValue(content, "tag_name");
            if (version != latestVersion && (forced || Config.Current.SkipVersion != latestVersion))
            {
                var updateDialog = new UpdateAvailable(latestVersion);
                updateDialog.ShowDialog();
                if (updateDialog.DialogResult == true)
                {
                    try
                    {
                        Process.Start("https://github.com/Fidifis/TextureMerge/releases/latest");
                    }
                    catch (Exception ex)
                    {
                        MessageDialog.Show("Failed to open web browser." + Environment.NewLine + ex.Message,
                            "Error", MessageDialog.Type.Error);
                    }
                }
                else if (updateDialog.Skip)
                {
                    Config.Current.SkipVersion = latestVersion;
                }
            }
            else if (forced)
            {
                MessageDialog.Show("No updates");
            }
        }

        private static string GetValue(string content, string key)
        {
            int start, end;
            start = content.IndexOf(key, 0);
            end = content.IndexOf(",", start);
            return content.Substring(start, end - start).Split(':')[1].Trim().Replace("\"", "");
        }
    }
}
