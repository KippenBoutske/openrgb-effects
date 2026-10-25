using System.ComponentModel;
using System.Windows;
using OpenRgbEffects.App.Services;
using OpenRgbEffects.App.ViewModels;

namespace OpenRgbEffects.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly TrayService _tray;
    private bool _initialized;
    private bool _shuttingDown;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        _viewModel.PreviewUpdated += colors => PreviewStrip.Update(colors);
        _tray = new TrayService(this);
        _tray.StartRequested += async (_, _) => await _viewModel.StartAsync();
        _tray.StopRequested += async (_, _) => await _viewModel.StopAsync();

        Loaded += OnLoaded;
        Closing += OnClosing;
        StateChanged += OnStateChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
            return;
        _initialized = true;

        await _viewModel.InitializeAsync();

        if (App.StartMinimized && _viewModel.MinimizeToTray)
        {
            _tray.HideToTray();
            _tray.ShowBalloon("OpenRGB Effects", "Draait in het systeemvak.");
        }
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized && _viewModel.MinimizeToTray)
            _tray.HideToTray();
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_viewModel.MinimizeToTray && !_tray.ExitRequested)
        {
            e.Cancel = true;
            _tray.HideToTray();
            return;
        }

        if (_shuttingDown)
            return;

        e.Cancel = true;
        _shuttingDown = true;
        await _viewModel.ShutdownAsync();
        _viewModel.Dispose();
        _tray.Dispose();
        Close();
    }
}
