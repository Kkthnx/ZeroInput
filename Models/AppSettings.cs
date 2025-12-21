using System.Windows.Input;

namespace ZeroInput.Models;

public class AppSettings
{
    public bool RunAtStartup { get; set; } = false;
    public bool StartMinimized { get; set; } = false;

    // NEW: Global Toggle Hotkey Configuration
    public Key ToggleKey { get; set; } = Key.F12; // Default to F12 or something safe
    public bool ToggleCtrl { get; set; } = true;
    public bool ToggleAlt { get; set; } = true;
    public bool ToggleShift { get; set; } = false;
    public bool ToggleWin { get; set; } = false;
}