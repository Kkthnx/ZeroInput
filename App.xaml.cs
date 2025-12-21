using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading; // Required for Mutex
using System.Windows;
using Velopack;
using ZeroInput.Services;
using ZeroInput.ViewModels;

namespace ZeroInput;

public partial class App : System.Windows.Application
{
    public static IHost? AppHost { get; private set; }

    // NEW: Unique Mutex Name (Global\ allows it to work across sessions)
    private const string UniqueMutexName = "Global\\ZeroInput_Kkthnx_Mutex";
    private static Mutex? _mutex;

    public App()
    {
        // 1. Initialize Velopack (Must run first for install hooks)
        VelopackApp.Build().Run();

        // 2. Configure Dependency Injection
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                // Core Services
                services.AddSingleton<KeyboardHookService>();

                // Views & ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // 3. SINGLE INSTANCE CHECK (The Fix)
        // We try to create a new Mutex. If "isNewInstance" is false, the app is already running.
        bool isNewInstance;
        _mutex = new Mutex(true, UniqueMutexName, out isNewInstance);

        if (!isNewInstance)
        {
            // App is already open. Close this new one silently.
            Shutdown();
            return;
        }

        // 4. Start the Host
        await AppHost!.StartAsync();

        // 5. Resolve Main Window & ViewModel
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        var viewModel = AppHost.Services.GetRequiredService<MainViewModel>();

        // 6. Handle "Start Minimized" Logic
        // If the user checked "Start Minimized" in settings, we load the window but hide it.
        // This ensures the Tray Icon loads, but the big window doesn't pop up.
        if (viewModel.StartMinimized)
        {
            // Set state to minimized so it knows its state
            mainWindow.WindowState = WindowState.Minimized;

            // "Show" creates the window handle (needed for hooks/tray), "Hide" keeps it invisible
            mainWindow.Show();
            mainWindow.Hide();
        }
        else
        {
            mainWindow.Show();
        }

        base.OnStartup(e);

        // 7. Check for Updates in Background
        _ = UpdateMyApp();
    }

    private async Task UpdateMyApp()
    {
        try
        {
            // UPDATE THIS: Point to your GitHub Releases URL
            string updateSource = "https://github.com/Kkthnx/ZeroInput/releases";

            var mgr = new UpdateManager(updateSource);

            var newVersion = await mgr.CheckForUpdatesAsync();
            if (newVersion == null) return;

            await mgr.DownloadUpdatesAsync(newVersion);
            // Updates will apply on the next restart automatically
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Update Failed: {ex.Message}");
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // Dispose Host
        if (AppHost != null)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
        }

        // Release Mutex (Critical cleanup)
        if (_mutex != null)
        {
            // Only release if we actually own it (handled by try/catch in case of weird state)
            try { _mutex.ReleaseMutex(); } catch { }
            _mutex.Dispose();
        }

        base.OnExit(e);
    }
}