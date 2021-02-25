using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LightGrid
{
    // original sourced from https://stackoverflow.com/a/56961180
    public class SettingsManager<T> where T : class, new()
    {
        private readonly string settingsFileName = "UserSettings.json";
        private readonly string settingsDirectoryName = "LightGrid";

        private readonly string filePath; // usersettings file path
        public readonly string directoryPath; // configuration directory path - public for use with musicmode credentials

        public SettingsManager()
        {
            filePath = GetLocalFilePath(settingsFileName);
            directoryPath = Path.GetDirectoryName(GetLocalFilePath(settingsFileName));
        }

        private string GetLocalFilePath(string fileName)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, settingsDirectoryName, fileName);
        }

        public T LoadSettings() =>
            File.Exists(filePath) ?
                JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath)) :
                new T();

        public void SaveSettings(T settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            File.WriteAllText(filePath, json);
        }
    }

    public class UserSettings
    {
        public List<ColorValues> favoriteColors { get; set; } = new List<ColorValues>();
        public string spotifyClientId { get; set; }
        public string spotifyClientSecret { get; set; }
    }

    public class ColorValues
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }
}