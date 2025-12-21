# ZeroInput

**ZeroInput** is a specialized Windows utility designed to selectively block keyboard input at the kernel level. It is ideal for cleaning keyboards, preventing accidental gaming interruptions (Alt-Tab), or specific kiosk scenarios.

![ZeroInput Screenshot](https://via.placeholder.com/800x450?text=App+Screenshot+Here)

## 🛡️ Features

- **Selective Blocking:** Block specific keys (WinKey, Alt-Tab) or custom combinations.
- **Kernel-Level Hooks:** Uses `WH_KEYBOARD_LL` for robust interception that works over most applications.
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
git clone [https://github.com/YourUsername/ZeroInput.git](https://github.com/YourUsername/ZeroInput.git)

# Build and Run
dotnet build
dotnet run --project ZeroInput
