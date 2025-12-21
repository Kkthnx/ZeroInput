using System.IO;
using System.Text.Json;
using ZeroInput.Models;

namespace ZeroInput.Services
{
    public static class ConfigService
    {
        private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZeroInput");
        private static readonly string FilePath = Path.Combine(FolderPath, "config.json");

        public static void Save(ConfigData data)
        {
            try
            {
                if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Save Failed: {ex.Message}"); }
        }

        public static ConfigData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<ConfigData>(json) ?? CreateDefaults();
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Load Failed: {ex.Message}"); }
            return CreateDefaults();
        }

        private static ConfigData CreateDefaults()
        {
            return new ConfigData
            {
                Settings = new AppSettings(),
                Rules = new List<BlockRule> {
                    new BlockRule { Name = "No WinKey", Key = System.Windows.Input.Key.LWin, IsWinKeyRequired = true },
                    new BlockRule { Name = "No Alt-Tab", Key = System.Windows.Input.Key.Tab, IsAltRequired = true }
                }
            };
        }
    }
}