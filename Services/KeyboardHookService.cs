using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using ZeroInput.Models;

namespace ZeroInput.Services;

public class KeyboardHookService : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private readonly LowLevelKeyboardProc _proc;
    private IntPtr _hookID = IntPtr.Zero;

    // State Flags
    private volatile BlockRule[] _activeRules = Array.Empty<BlockRule>();
    private volatile bool _isBlockingActive = false;

    // Toggle Hotkey State
    private volatile Key _toggleKey = Key.None;
    private volatile bool _toggleCtrl;
    private volatile bool _toggleAlt;
    private volatile bool _toggleShift;
    private volatile bool _toggleWin;

    public event Action? ProtectionToggled;

    public KeyboardHookService()
    {
        _proc = HookCallback;
        // The hook should ideally start when the service is created/app starts
        // But we can let the ViewModel call StartHook() explicitly.
    }

    public void InstallHook()
    {
        if (_hookID == IntPtr.Zero) _hookID = SetHook(_proc);
    }

    public void UninstallHook()
    {
        if (_hookID != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
        }
    }

    // Configure the "Magic Key" that flips the switch
    public void SetToggleHotkey(Key key, bool ctrl, bool alt, bool shift, bool win)
    {
        _toggleKey = key;
        _toggleCtrl = ctrl;
        _toggleAlt = alt;
        _toggleShift = shift;
        _toggleWin = win;
    }

    // Updates the blocking rules (Called when user edits rules)
    public void UpdateRules(IEnumerable<BlockRule> rules)
    {
        _activeRules = rules.Where(r => r.IsActive).ToArray();
    }

    // Sets the "Protection Mode" without removing the hook
    public void SetBlockingState(bool isBlocking)
    {
        _isBlockingActive = isBlocking;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Key key = KeyInterop.KeyFromVirtualKey(vkCode);

            // 1. Modifier Check
            bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            bool isAlt = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;
            bool isShift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool isWin = key == Key.LWin || key == Key.RWin;

            // 2. GLOBAL TOGGLE CHECK (High Priority)
            // Checks if the pressed combo matches the configured Toggle Hotkey
            if (key == _toggleKey &&
                isCtrl == _toggleCtrl &&
                isAlt == _toggleAlt &&
                isShift == _toggleShift)
            {
                // Fire event to UI
                ProtectionToggled?.Invoke();
                return (IntPtr)1; // Swallow the toggle key so it doesn't type/act in other apps
            }

            // 3. Blocking Logic (Only if Protection is ON)
            if (_isBlockingActive)
            {
                var currentRules = _activeRules;
                for (int i = 0; i < currentRules.Length; i++)
                {
                    var rule = currentRules[i];
                    if (rule.Key == key)
                    {
                        if (isWin && rule.IsWinKeyRequired) return (IntPtr)1;

                        if (rule.IsAltRequired == isAlt &&
                            rule.IsCtrlRequired == isCtrl &&
                            rule.IsShiftRequired == isShift)
                        {
                            return (IntPtr)1; // Block
                        }
                    }
                }
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    // ... P/Invoke Definitions (Same as before) ...
    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    }
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern IntPtr GetModuleHandle(string? lpModuleName);
    public void Dispose() { UninstallHook(); GC.SuppressFinalize(this); }
}