using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized; // Required for CollectionChanged
using System.ComponentModel;          // Required for PropertyChanged
using System.Windows.Input;
using ZeroInput.Models;
using ZeroInput.Services;

namespace ZeroInput.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly KeyboardHookService _hookService;
    private readonly ConfigData _configData;
    private readonly bool _isInitialized = false;

    [ObservableProperty] private bool _isProtectionActive;
    [ObservableProperty] private bool _startMinimized;
    [ObservableProperty] private bool _runAtStartup;
    [ObservableProperty] private BlockRule? _selectedRule;

    // Global Hotkey Settings
    [ObservableProperty] private Key _toggleKey;
    [ObservableProperty] private bool _toggleCtrl;
    [ObservableProperty] private bool _toggleAlt;
    [ObservableProperty] private bool _toggleShift;
    [ObservableProperty] private bool _toggleWin;

    public ObservableCollection<BlockRule> Rules { get; }
    public ObservableCollection<Key> AllKeys { get; }

    public MainViewModel(KeyboardHookService hookService)
    {
        _hookService = hookService;
        _configData = ConfigService.Load();

        // 1. Initialize Rules Collection
        Rules = new ObservableCollection<BlockRule>(_configData.Rules);

        // 2. LISTENERS: Watch for changes inside the list (Add/Remove) AND inside specific items (Toggles)
        Rules.CollectionChanged += Rules_CollectionChanged;

        // Register listeners for existing rules loaded from disk
        foreach (var rule in Rules)
        {
            rule.PropertyChanged += OnRulePropertyChanged;
        }

        AllKeys = new ObservableCollection<Key>(Enum.GetValues<Key>());

        // Load Settings
        RunAtStartup = _configData.Settings.RunAtStartup;
        StartMinimized = _configData.Settings.StartMinimized;
        ToggleKey = _configData.Settings.ToggleKey;
        ToggleCtrl = _configData.Settings.ToggleCtrl;
        ToggleAlt = _configData.Settings.ToggleAlt;
        ToggleShift = _configData.Settings.ToggleShift;
        ToggleWin = _configData.Settings.ToggleWin;

        UpdateServiceToggleConfig();

        // Install Hook
        _hookService.InstallHook();
        _hookService.SetBlockingState(false);
        _hookService.ProtectionToggled += OnExternalToggleRequest;

        _isInitialized = true;
    }

    // ---- NEW LOGIC STARTS HERE ----

    // When a rule is Added or Removed, we need to attach/detach listeners
    private void Rules_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (BlockRule item in e.NewItems)
                item.PropertyChanged += OnRulePropertyChanged;
        }

        if (e.OldItems != null)
        {
            foreach (BlockRule item in e.OldItems)
                item.PropertyChanged -= OnRulePropertyChanged;
        }
    }

    // When the USER toggles a switch (IsActive changes), we update the service immediately
    private void OnRulePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BlockRule.IsActive))
        {
            // Update the live blocking list immediately
            if (IsProtectionActive)
            {
                _hookService.UpdateRules(Rules);
            }
            // Auto-save the change
            if (_isInitialized) SaveConfig();
        }
        else if (e.PropertyName == nameof(BlockRule.Key) ||
                 e.PropertyName == nameof(BlockRule.IsCtrlRequired) ||
                 e.PropertyName == nameof(BlockRule.IsAltRequired) ||
                 e.PropertyName == nameof(BlockRule.IsShiftRequired) ||
                 e.PropertyName == nameof(BlockRule.IsWinKeyRequired))
        {
            // Also save/update if they edit the key combo itself
            if (IsProtectionActive) _hookService.UpdateRules(Rules);
            if (_isInitialized) SaveConfig();
        }
    }
    // ---- NEW LOGIC ENDS HERE ----

    partial void OnToggleKeyChanged(Key value) => UpdateServiceToggleConfig();
    partial void OnToggleCtrlChanged(bool value) => UpdateServiceToggleConfig();
    partial void OnToggleAltChanged(bool value) => UpdateServiceToggleConfig();
    partial void OnToggleShiftChanged(bool value) => UpdateServiceToggleConfig();
    partial void OnToggleWinChanged(bool value) => UpdateServiceToggleConfig();

    private void UpdateServiceToggleConfig()
    {
        _hookService.SetToggleHotkey(ToggleKey, ToggleCtrl, ToggleAlt, ToggleShift, ToggleWin);
        if (_isInitialized) SaveConfig();
    }

    private void OnExternalToggleRequest()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (ToggleProtectionCommand.CanExecute(null)) ToggleProtectionCommand.Execute(null);
        });
    }

    [RelayCommand]
    private void ToggleProtection()
    {
        IsProtectionActive = !IsProtectionActive;

        if (IsProtectionActive)
        {
            // This sends ONLY the rules where IsActive == true
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
        if (IsProtectionActive) _hookService.UpdateRules(Rules);
        SaveConfig();
    }

    [RelayCommand]
    public void SaveConfig()
    {
        _configData.Rules = Rules.ToList();
        _configData.Settings.RunAtStartup = RunAtStartup;
        _configData.Settings.StartMinimized = StartMinimized;
        _configData.Settings.ToggleKey = ToggleKey;
        _configData.Settings.ToggleCtrl = ToggleCtrl;
        _configData.Settings.ToggleAlt = ToggleAlt;
        _configData.Settings.ToggleShift = ToggleShift;
        _configData.Settings.ToggleWin = ToggleWin;

        StartupService.SetStartup(RunAtStartup);
        ConfigService.Save(_configData);
    }
}