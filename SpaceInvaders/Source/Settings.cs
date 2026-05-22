using System;
using System.IO;
using System.Text.Json;

namespace SpaceInvaders
{
    public class Settings
    {
        public int HighScore { get; set; }
        public float Volume { get; set; } = 1f;
        public bool Fullscreen { get; set; }
        public bool ScreenShake { get; set; } = true;

        static Settings _instance;
        public static Settings Instance => _instance ??= Load();

        static string SavePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SpaceInvaders", "settings.json");

        static Settings Load()
        {
            try
            {
                if (File.Exists(SavePath))
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText(SavePath));
            }
            catch { }
            return new Settings();
        }

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(SavePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(SavePath,
                    JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }
    }
}
