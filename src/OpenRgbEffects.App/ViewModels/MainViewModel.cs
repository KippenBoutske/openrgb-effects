using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenRgbEffects.App.Services;
using OpenRgbEffects.Core;

namespace OpenRgbEffects.App.ViewModels;

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected
}

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IRgbController _controller;
    private readonly EffectEngine _engine;
    private readonly SettingsStore _settingsStore;
    private readonly DispatcherTimer _saveTimer;
    private readonly DispatcherTimer _reconnectTimer;
    private bool _suppressSave;
    private bool _userRequestedConnect = true;

    public MainViewModel()
        : this(new OpenRgbController(), new SettingsStore())
    {
    }

    public MainViewModel(IRgbController controller, SettingsStore settingsStore)
    {
        _controller = controller;
        _settingsStore = settingsStore;
        _engine = new EffectEngine(_controller);
        _engine.Failed += OnEngineFailed;
        _controller.DeviceListUpdated += OnDeviceListUpdated;

        Devices = [];
        Settings = _settingsStore.Load();

        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _saveTimer.Tick += (_, _) =>
        {
            _saveTimer.Stop();
            Persist();
        };

        _reconnectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _reconnectTimer.Tick += async (_, _) =>
        {
            if (!IsConnected && !IsBusy && _userRequestedConnect)
                await ConnectAsync();
        };

        ApplySettings(Settings);
        PropertyChanged += (_, e) =>
        {
            if (_suppressSave)
                return;
            if (e.PropertyName is nameof(StatusText) or nameof(IsBusy) or nameof(IsConnected)
                or nameof(ConnectionStatus) or nameof(IsRunning) or nameof(CanStart))
                return;
            QueueSave();
            PushEffectState();
        };
    }

    public AppSettings Settings { get; private set; }

    public ObservableCollection<DeviceItemViewModel> Devices { get; }

    [ObservableProperty]
    private string _host = "127.0.0.1";

    [ObservableProperty]
    private string _portText = "6742";

    [ObservableProperty]
    private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(ConnectLabel))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    private bool _connectedFlag;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusText = "Niet verbonden. Start OpenRGB als administrator en zet de SDK-server aan.";

    [ObservableProperty]
    private EffectKind _effect = EffectKind.Static;

    [ObservableProperty]
    private Color _selectedColor = Colors.White;

    [ObservableProperty]
    private double _speed = 50;

    [ObservableProperty]
    private double _brightness = 100;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private bool _resumeEffectOnStart = true;

    public bool IsConnected => ConnectedFlag;

    public string ConnectLabel => IsConnected ? "Opnieuw verbinden" : "Verbinden";

    public bool CanConnect => !IsBusy;

    public bool CanStart => IsConnected && !IsBusy && !IsRunning;

    public bool CanStop => IsRunning && !IsBusy;

    public bool CanRefresh => IsConnected && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanConnect))]
    public async Task ConnectAsync()
    {
        _userRequestedConnect = true;
        IsBusy = true;
        ConnectionStatus = ConnectionStatus.Connecting;
        StatusText = $"Bezig met verbinden met {Host}:{PortText}…";
        try
        {
            if (!int.TryParse(PortText, out var port) || port is < 1 or > 65535)
                throw new InvalidOperationException("Poort moet een getal tussen 1 en 65535 zijn.");

            await Task.Run(() =>
            {
                if (_controller.Connected)
                    _controller.Disconnect();
                _controller.Connect(Host.Trim(), port);
            }).ConfigureAwait(true);

            ConnectedFlag = true;
            ConnectionStatus = ConnectionStatus.Connected;
            RefreshDevices();
            StatusText = $"Verbonden met {Host.Trim()}:{port} — {Devices.Count} apparaat{(Devices.Count == 1 ? "" : "en")}.";
            _reconnectTimer.Stop();
        }
        catch (Exception ex)
        {
            ConnectedFlag = false;
            ConnectionStatus = ConnectionStatus.Disconnected;
            StatusText = $"Niet verbonden: {FriendlyError(ex)}";
            if (!_reconnectTimer.IsEnabled)
                _reconnectTimer.Start();
        }
        finally
        {
            IsBusy = false;
            StartCommand.NotifyCanExecuteChanged();
            RefreshCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    public void Refresh()
    {
        if (!IsConnected)
            return;
        try
        {
            RefreshDevices();
            StatusText = $"Apparaten vernieuwd — {Devices.Count} apparaat{(Devices.Count == 1 ? "" : "en")}.";
        }
        catch (Exception ex)
        {
            HandleDisconnect(ex);
        }
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    public async Task StartAsync()
    {
        if (!IsConnected)
            await ConnectAsync();
        if (!IsConnected)
            return;

        var selected = SelectedDevices();
        if (selected.Count == 0)
        {
            StatusText = "Selecteer minstens één apparaat voor het effect.";
            return;
        }

        PushEffectState();
        try
        {
            _engine.Start(selected);
            IsRunning = true;
            Settings.WasRunning = true;
            Persist();
            StatusText = $"Effect actief: {DeviceLabels.Effect(Effect)}.";
        }
        catch (Exception ex)
        {
            HandleDisconnect(ex);
        }

        StopCommand.NotifyCanExecuteChanged();
        StartCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    public async Task StopAsync()
    {
        IsBusy = true;
        try
        {
            await _engine.StopAsync(restoreWhite: true).ConfigureAwait(true);
            IsRunning = false;
            Settings.WasRunning = false;
            Persist();
            StatusText = "Gestopt. Statisch wit hersteld.";
        }
        catch (Exception ex)
        {
            StatusText = $"Stoppen mislukt: {FriendlyError(ex)}";
        }
        finally
        {
            IsBusy = false;
            StopCommand.NotifyCanExecuteChanged();
            StartCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    public void SelectAllDevices()
    {
        foreach (var device in Devices)
            device.IsSelected = true;
    }

    [RelayCommand]
    public void SelectNoDevices()
    {
        foreach (var device in Devices)
            device.IsSelected = false;
    }

    public async Task InitializeAsync()
    {
        if (StartWithWindows != StartupManager.IsEnabled())
            StartupManager.SetEnabled(StartWithWindows);

        await ConnectAsync();
        if (IsConnected && ResumeEffectOnStart && Settings.WasRunning)
            await StartAsync();
    }

    public async Task ShutdownAsync()
    {
        _reconnectTimer.Stop();
        _saveTimer.Stop();
        var resume = IsRunning;
        if (IsRunning)
        {
            await _engine.StopAsync(restoreWhite: true).ConfigureAwait(true);
            IsRunning = false;
        }
        Settings.WasRunning = resume;
        Persist();
        _controller.Disconnect();
    }

    public void Dispose()
    {
        _reconnectTimer.Stop();
        _saveTimer.Stop();
        _engine.Failed -= OnEngineFailed;
        _controller.DeviceListUpdated -= OnDeviceListUpdated;
        _engine.Dispose();
        _controller.Dispose();
        GC.SuppressFinalize(this);
    }

    partial void OnStartWithWindowsChanged(bool value)
    {
        try
        {
            StartupManager.SetEnabled(value);
        }
        catch (Exception ex)
        {
            StatusText = $"Starten met Windows mislukt: {FriendlyError(ex)}";
        }
    }

    private void ApplySettings(AppSettings settings)
    {
        _suppressSave = true;
        Host = settings.Host;
        PortText = settings.Port.ToString();
        Effect = settings.Effect;
        SelectedColor = ToMedia(settings.Color);
        Speed = settings.Speed * 100;
        Brightness = settings.Brightness * 100;
        StartWithWindows = settings.StartWithWindows;
        MinimizeToTray = settings.MinimizeToTray;
        ResumeEffectOnStart = settings.ResumeEffectOnStart;
        _suppressSave = false;
    }

    private void RefreshDevices()
    {
        var hadList = Devices.Count > 0;
        var selectedNames = hadList
            ? Devices.Where(device => device.IsSelected).Select(device => device.Name).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : Settings.SelectedDevices.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var selectAllMissing = !hadList && selectedNames.Count == 0;

        var devices = _controller.GetDevices();
        foreach (var existing in Devices)
            existing.SelectionChanged -= OnDeviceSelectionChanged;
        Devices.Clear();

        foreach (var device in devices)
        {
            var selected = selectAllMissing || selectedNames.Contains(device.Name);
            var item = new DeviceItemViewModel(device, selected);
            item.SelectionChanged += OnDeviceSelectionChanged;
            Devices.Add(item);
        }

        PushEffectState();
    }

    private void OnDeviceSelectionChanged(object? sender, EventArgs e)
    {
        QueueSave();
        PushEffectState();
        StartCommand.NotifyCanExecuteChanged();
    }

    private void PushEffectState()
    {
        var color = new RgbColor(SelectedColor.R, SelectedColor.G, SelectedColor.B);
        var ids = Devices.Where(device => device.IsSelected).Select(device => device.Device.Index);
        _engine.State.Update(Effect, color, Speed / 100.0, Brightness / 100.0, ids);
    }

    private IReadOnlyList<RgbDeviceInfo> SelectedDevices() =>
        Devices.Where(device => device.IsSelected).Select(device => device.Device).ToArray();

    private void QueueSave()
    {
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void Persist()
    {
        Settings.Host = string.IsNullOrWhiteSpace(Host) ? "127.0.0.1" : Host.Trim();
        Settings.Port = int.TryParse(PortText, out var port) ? port : 6742;
        Settings.Effect = Effect;
        Settings.ColorHex = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
        Settings.Speed = Speed / 100.0;
        Settings.Brightness = Brightness / 100.0;
        Settings.SelectedDevices = Devices.Where(device => device.IsSelected).Select(device => device.Name).ToList();
        Settings.StartWithWindows = StartWithWindows;
        Settings.MinimizeToTray = MinimizeToTray;
        Settings.ResumeEffectOnStart = ResumeEffectOnStart;
        _settingsStore.Save(Settings);
    }

    private void OnEngineFailed(object? sender, string message)
    {
        RunOnUi(() =>
        {
            IsRunning = false;
            HandleDisconnect(new InvalidOperationException(message));
        });
    }

    private void OnDeviceListUpdated(object? sender, EventArgs e)
    {
        RunOnUi(() =>
        {
            if (!IsConnected)
                return;
            try
            {
                RefreshDevices();
                StatusText = "OpenRGB heeft de apparaatlijst bijgewerkt.";
            }
            catch (Exception ex)
            {
                HandleDisconnect(ex);
            }
        });
    }

    private void HandleDisconnect(Exception ex)
    {
        ConnectedFlag = false;
        ConnectionStatus = ConnectionStatus.Disconnected;
        IsRunning = false;
        StatusText = $"Verbinding verbroken: {FriendlyError(ex)}";
        if (!_reconnectTimer.IsEnabled)
            _reconnectTimer.Start();
    }

    private static void RunOnUi(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
            action();
        else
            dispatcher.Invoke(action);
    }

    private static Color ToMedia(RgbColor color) => Color.FromRgb(color.R, color.G, color.B);

    private static string FriendlyError(Exception ex)
    {
        var text = ex.InnerException?.Message ?? ex.Message;
        if (text.Contains("actively refused", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("No connection", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("forcibly closed", StringComparison.OrdinalIgnoreCase))
        {
            return "OpenRGB is niet bereikbaar. Controleer of de SDK-server op poort 6742 actief is.";
        }

        return text;
    }
}
