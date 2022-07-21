using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

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
                client.Headers.Add(HttpRequestHeader.UserAgent, "TextureMerge_webclient");
                content = await client.DownloadStringTaskAsync(new Uri("https://api.github.com/repos/Fidifis/TextureMerge/releases/latest"));
            }

            string latestVersion = GetValue(content, "tag_name");
            if (version != latestVersion && (forced || Config.Current.SkipVersion != latestVersion))
            {
                var updateDialog = new UpdateAvailable(latestVersion);
                updateDialog.ShowDialog();
                if (updateDialog.DialogResult == true)
                {
                    Process.Start("https://github.com/Fidifis/TextureMerge/releases/latest");
                }
                else if (updateDialog.Skip)
                {
                    Config.Current.SkipVersion = latestVersion;
                }
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
