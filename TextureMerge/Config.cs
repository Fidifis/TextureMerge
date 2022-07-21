using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TextureMerge
{
    [Serializable]
    public class Config
    {
        public static Config Current { get; private set; } = new Config();

        // Config related
        public string Redirect { get; set; } = "config.xml";
        public bool UseLastWindowSize { get; set; } = true;
        public bool UseLastPathToSave { get; set; } = true;
        public bool UseLastSaveImageName { get; set; } = true;
        public bool CheckForUpdates { get; set; } = true;
        public string SkipVersion { get; set; } = "";

        // State related
        public int WindowWidth { get; set; } = -1;
        public int WindowHeight { get; set; } = -1;
        public string PathToSave { get; set; } = @"%UserProfile%\Documents";
        public string SaveImageName { get; set; } = "Pack.png";
        public bool DefaultColor { get; set; } = false;

        public static void ApplyConfig(Config config) => Current = config;

        public Config Copy()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;
                return (Config)formatter.Deserialize(ms);
            }
        }

        public static void Load()
        {
            string lastRedirect = Current.Redirect;
            int redirected = 0;
            do
            {
                if (!File.Exists(lastRedirect.Expand()))
                    return;

                var serializer = new XmlSerializer(typeof(Config));

                var stream = new FileStream(lastRedirect.Expand(), FileMode.Open);
                var config = (Config)serializer.Deserialize(stream);
                stream.Close();

                if (config.Redirect == lastRedirect)
                {
                    Current = config;
                    return;
                }

                else
                {
                    lastRedirect = config.Redirect;
                    redirected++;
                }

            } while (redirected < 2);
        }

        public static void Save()
        {
            if (!Current.Redirect.Contains('\\') || Directory.Exists(Path.GetDirectoryName(Current.Redirect.Expand())))
            {
                var stream = new FileStream(Current.Redirect.Expand(), FileMode.Create);
                var serializer = new XmlSerializer(typeof(Config));
                serializer.Serialize(stream, Current);
            }
        }
    }
}
