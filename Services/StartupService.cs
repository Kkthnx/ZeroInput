using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace ZeroInput.Services
{
    [SupportedOSPlatform("windows")]
    public static class StartupService
    {
        private const string AppName = "ZeroInput";
        private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public static void SetStartup(bool enable)
        {
            try
            {
                string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath)) return;

                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
                {
                    if (key == null) return;
                    if (enable) key.SetValue(AppName, $"\"{exePath}\"");
                    else if (key.GetValue(AppName) != null) key.DeleteValue(AppName);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Registry Error: {ex.Message}"); }
        }
    }
}