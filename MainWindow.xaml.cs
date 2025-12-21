using System.ComponentModel;
using System.Windows;
using ZeroInput.ViewModels;

namespace ZeroInput;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private System.Windows.Forms.NotifyIcon? _notifyIcon;

    // Constructor Injection via DI
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        InitializeSystemTray();

        if (_viewModel.StartMinimized)
        {
            // Defer hiding until loaded to ensure handle creation
            Loaded += (s, e) =>
            {
                Hide();
                // Ensure protection is active if starting minimized
                if (!_viewModel.IsProtectionActive)
                {
                    if (_viewModel.ToggleProtectionCommand.CanExecute(null))
                        _viewModel.ToggleProtectionCommand.Execute(null);
                }
            };
        }
    }

    private void InitializeSystemTray()
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            // Fallback icon first
            Icon = System.Drawing.SystemIcons.Shield,
            Visible = true,
            Text = "ZeroInput"
        };

        try
        {
            // Explicitly use System.Windows.Application to avoid WinForms conflict
            var resourceInfo = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/ZeroInput;component/ZeroInput.ico"));

            if (resourceInfo != null)
            {
                _notifyIcon.Icon = new System.Drawing.Icon(resourceInfo.Stream);
            }
        }
        catch
        {
            // If the icon fails to load, we silently keep the Shield icon set above
        }

        _notifyIcon.DoubleClick += (s, args) => ShowWindow();

        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add("Open ZeroInput", null, (s, e) => ShowWindow());
        contextMenu.Items.Add("-");

        // Explicitly use System.Windows.Application for Shutdown
        contextMenu.Items.Add("Exit", null, (s, e) =>
        {
            _notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        });

        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Intercept close button to minimize to tray instead
        e.Cancel = true;
        Hide();

        // Save config when hiding
        if (_viewModel.SaveConfigCommand.CanExecute(null))
            _viewModel.SaveConfigCommand.Execute(null);
    }
}