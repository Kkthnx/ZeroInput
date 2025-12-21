using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using System.Xml.Linq;

namespace ZeroInput.Models;

public partial class BlockRule : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayString))]
    private string _name = "New Rule";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayString))]
    private Key _key = Key.None;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayString))]
    private bool _isAltRequired;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayString))]
    private bool _isCtrlRequired;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayString))]
    private bool _isShiftRequired;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayString))]
    private bool _isWinKeyRequired;

    [ObservableProperty]
    private bool _isActive = true;

    public string DisplayString
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Name) && Name != "New Rule") return Name;

            var parts = new List<string>(4); // Pre-size to avoid resize allocation
            if (IsCtrlRequired) parts.Add("Ctrl");
            if (IsAltRequired) parts.Add("Alt");
            if (IsShiftRequired) parts.Add("Shift");
            if (IsWinKeyRequired) parts.Add("Win");
            parts.Add(Key.ToString());

            return string.Join(" + ", parts);
        }
    }
}