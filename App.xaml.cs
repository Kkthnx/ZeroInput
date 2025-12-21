using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Velopack;
using ZeroInput.Services;
using ZeroInput.ViewModels;

namespace ZeroInput;

// Fix: Explicitly inherit from System.Windows.Application
public partial class App : System.Windows.Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        // 1. Initialize Velopack
        // This handles setup/uninstall hooks.
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
        await AppHost!.StartAsync();

        // 3. Resolve and Show Window Manually
        // Explicitly use System.Windows.Application to avoid ambiguity
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);

        // 4. Check for Updates in Background
        _ = UpdateMyApp();
    }

    private async Task UpdateMyApp()
    {
        try
        {
            // You must replace this with your actual update URL or path
            // For local testing, you can use a directory path e.g., @"C:\Releases"
            string updateSource = "https://github.com/YourUser/ZeroInput/releases";

            var mgr = new UpdateManager(updateSource);

            var newVersion = await mgr.CheckForUpdatesAsync();
            if (newVersion == null) return;

            await mgr.DownloadUpdatesAsync(newVersion);

            // Updates will be applied on next restart
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Update Failed: {ex.Message}");
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (AppHost != null)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
        }
        base.OnExit(e);
    }
}