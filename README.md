# ZeroInput

**ZeroInput** is a specialized Windows utility designed to selectively block keyboard input using system-wide hooks. It is ideal for cleaning keyboards, preventing accidental gaming interruptions (Alt-Tab), or specific kiosk scenarios.

<img width="788" height="493" alt="image" src="https://github.com/user-attachments/assets/7cc56598-e1d7-41bd-8678-f1f78c17752b" />

## ⚠️ Safety & Anti-Cheat Disclaimer

**Please Read Before Use:**
This tool uses `WH_KEYBOARD_LL` (Global Windows Hooks) to intercept input. While this is a standard Windows API used by accessibility tools, it behaves similarly to macro software.

- **Anti-Cheat:** Competitive games with kernel-level anti-cheat (e.g., Valorant/Vanguard, R6 Siege/BattlEye) may flag this application or prevent the game from launching if it is active.
- **Recommendation:** Always **toggle protection OFF** using the Panic Switch before launching competitive multiplayer games.
- **Status:** This software is currently **unsigned**. Windows SmartScreen may display a warning upon first run.

## 🛡️ Features

- **Selective Blocking:** Block specific keys (WinKey, Alt-Tab) or custom combinations.
- **Low-Level Hooks:** Uses the `WH_KEYBOARD_LL` API to intercept input at the system level before most applications receive it.
- **Panic Switch (Deadman Toggle):** Instantly toggle protection ON/OFF from anywhere using a global hotkey.
- **Stealth Mode:** Can start minimized to the system tray.
- **Auto-Update:** Self-updating via Velopack.

## 🚀 Getting Started

### Installation

1. Download the latest `Setup.exe` from the [Releases Page](../../releases).
2. Run the installer. The app will launch automatically.

### Usage

1. **Add Rules:** Go to the **Rules** tab and click `+ New Rule`.
2. **Configure:** Select the trigger key and required modifiers (e.g., Block `LWin`).
3. **Activate:** Click the large **Protection: OFF** button to arm the hooks.

### ⚠️ The Panic Switch

If you accidentally block your keyboard or mouse, use the Global Toggle Hotkey to disable protection immediately.

- **Default Hotkey:** `Ctrl` + `Alt` + `F12` (or whatever you set in Settings)
- This hotkey works **globally**, even if the app is minimized or hidden.

## 🛠️ Build from Source

**Requirements:**

- .NET 9.0 SDK
- Visual Studio 2022

```powershell
# Clone the repo
git clone [https://github.com/Kkthnx/ZeroInput.git](https://github.com/Kkthnx/ZeroInput.git)

# Build and Run
dotnet build
dotnet run --project ZeroInput
