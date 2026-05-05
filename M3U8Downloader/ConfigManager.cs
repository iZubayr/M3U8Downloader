using System.IO;
using System.Text.Json;

namespace M3U8Downloader
{
    public class AppConfig
    {
        public string OutputFolder { get; set; } =
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        public bool AutoPaste  { get; set; } = true;
        public bool AutoTitle  { get; set; } = true;
        public bool SetupDone  { get; set; } = false;
    }

    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "M3U8Downloader", "config.json");

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                    return JsonSerializer.Deserialize<AppConfig>(
                        File.ReadAllText(ConfigPath)) ?? new AppConfig();
            }
            catch { }
            return new AppConfig();
        }

        public static void Save(AppConfig config)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
                File.WriteAllText(ConfigPath,
                    JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }
    }
}
