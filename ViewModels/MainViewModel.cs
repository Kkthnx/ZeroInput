using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ZeroInput.Models;
using ZeroInput.Services;

namespace ZeroInput.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly KeyboardHookService _hookService;
    private readonly ConfigData _configData;
    private readonly bool _isInitialized = false; // Prevents saving during constructor

    // App State
    [ObservableProperty]
    private bool _isProtectionActive;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _runAtStartup;

    [ObservableProperty]
    private BlockRule? _selectedRule;

    // Global Toggle Hotkey Settings
    [ObservableProperty] private Key _toggleKey;
    [ObservableProperty] private bool _toggleCtrl;
    [ObservableProperty] private bool _toggleAlt;
    [ObservableProperty] private bool _toggleShift;
    [ObservableProperty] private bool _toggleWin;

    // Collections
    public ObservableCollection<BlockRule> Rules { get; }
    public ObservableCollection<Key> AllKeys { get; }

    public MainViewModel(KeyboardHookService hookService)
    {
        _hookService = hookService;
        _configData = ConfigService.Load();

        // Initialize Collections
        Rules = new ObservableCollection<BlockRule>(_configData.Rules);
        AllKeys = new ObservableCollection<Key>(Enum.GetValues<Key>());

        // Load Application Settings
        // We set the Properties here (not fields) to satisfy MVVM Toolkit warnings.
        // The _isInitialized flag prevents this from triggering a SaveConfig() immediately.
        RunAtStartup = _configData.Settings.RunAtStartup;
        StartMinimized = _configData.Settings.StartMinimized;

        // Load Toggle Hotkey Settings
        ToggleKey = _configData.Settings.ToggleKey;
        ToggleCtrl = _configData.Settings.ToggleCtrl;
        ToggleAlt = _configData.Settings.ToggleAlt;
        ToggleShift = _configData.Settings.ToggleShift;
        ToggleWin = _configData.Settings.ToggleWin;

        // Apply initial configuration to the service
        UpdateServiceToggleConfig();

        // 1. Install Hook Immediately (Passthrough Mode)
        _hookService.InstallHook();
        _hookService.SetBlockingState(false);

        // 2. Subscribe to Global Hotkey Event
        _hookService.ProtectionToggled += OnExternalToggleRequest;

        _isInitialized = true; // Constructor done, allow saving now.
    }

    // React to changes in the UI for hotkey settings
    partial void OnToggleKeyChanged(Key value) => UpdateServiceToggleConfig();
    partial void OnToggleCtrlChanged(bool value) => UpdateServiceToggleConfig();
    partial void OnToggleAltChanged(bool value) => UpdateServiceToggleConfig();
    partial void OnToggleShiftChanged(bool value) => UpdateServiceToggleConfig();
    partial void OnToggleWinChanged(bool value) => UpdateServiceToggleConfig();

    private void UpdateServiceToggleConfig()
    {
        // FIX: Use Public Properties to resolve MVVMTK0034
        _hookService.SetToggleHotkey(ToggleKey, ToggleCtrl, ToggleAlt, ToggleShift, ToggleWin);

        if (_isInitialized)
        {
            SaveConfig();
        }
    }

    private void OnExternalToggleRequest()
    {
        // FIX: Explicitly use System.Windows.Application to resolve ambiguity
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (ToggleProtectionCommand.CanExecute(null))
            {
                ToggleProtectionCommand.Execute(null);
            }
        });
    }

    [RelayCommand]
    private void ToggleProtection()
    {
        IsProtectionActive = !IsProtectionActive;

        if (IsProtectionActive)
        {
            _hookService.UpdateRules(Rules);

            using var process = System.Diagnostics.Process.GetCurrentProcess();
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
        }
        else
        {
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
        }

        _hookService.SetBlockingState(IsProtectionActive);
    }

    [RelayCommand]
    private void AddRule()
    {
        var rule = new BlockRule { Name = "New Rule", Key = Key.None };
        Rules.Add(rule);
        SelectedRule = rule;
        SaveConfig();
    }

    [RelayCommand]
    private void RemoveRule(BlockRule? rule)
    {
        rule ??= SelectedRule;
        if (rule == null) return;

        Rules.Remove(rule);

        if (IsProtectionActive)
        {
            _hookService.UpdateRules(Rules);
        }
        SaveConfig();
    }

    [RelayCommand]
    public void SaveConfig()
    {
        // Persist Rules
        _configData.Rules = Rules.ToList();

        // Persist App Settings
        // FIX: Use Public Properties to resolve MVVMTK0034
        _configData.Settings.RunAtStartup = RunAtStartup;
        _configData.Settings.StartMinimized = StartMinimized;

        // Persist Toggle Settings
        _configData.Settings.ToggleKey = ToggleKey;
        _configData.Settings.ToggleCtrl = ToggleCtrl;
        _configData.Settings.ToggleAlt = ToggleAlt;
        _configData.Settings.ToggleShift = ToggleShift;
        _configData.Settings.ToggleWin = ToggleWin;

        StartupService.SetStartup(RunAtStartup);

        ConfigService.Save(_configData);
    }
}