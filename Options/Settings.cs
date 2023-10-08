using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommandPalette.Options
{
    [DataContract]
    internal class Settings
    {
        [DataMember(Order = 0)]
        private string _previousCommand = "";

        public string PreviousCommand
        {
            get { return _previousCommand; }
            set {
                if (value == null) value = "";
                _previousCommand = value;
            }
        }
        private static readonly string ProgramDataFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CommandPalette");


        private static string GetSettingsFilePath()
        {
            const string name = "commandpalette.json";
            var settingsPath = Path.Combine(ProgramDataFolder, name);

            if (!File.Exists(settingsPath))
            {
                new Settings().SaveToFile(settingsPath);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(File.ReadAllText(settingsPath)))
                {
                    new Settings().SaveToFile(settingsPath);
                }
            }

            return settingsPath;
        }

        public void Save()
        {
            Directory.CreateDirectory(ProgramDataFolder);
            SaveToFile(GetSettingsFilePath());
        }

        public void SaveToFile(string path)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static Settings Load()
        {
            Directory.CreateDirectory(ProgramDataFolder);
            string filePath = GetSettingsFilePath();
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Settings>(json);
        }
    }
}
