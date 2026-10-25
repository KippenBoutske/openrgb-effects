using OpenRgbEffects.Core.Effects;

namespace OpenRgbEffects.Core;

public sealed class EffectState
{
    private readonly object _gate = new();
    private EffectKind _kind = EffectKind.Static;
    private RgbColor _color = RgbColor.White;
    private double _speed = 0.5;
    private double _brightness = 1.0;
    private int[] _deviceIds = [];

    public void Update(EffectKind kind, RgbColor color, double speed, double brightness, IEnumerable<int> deviceIds)
    {
        lock (_gate)
        {
            _kind = kind;
            _color = color;
            _speed = ColorMath.Clamp01(speed);
            _brightness = ColorMath.Clamp01(brightness);
            _deviceIds = deviceIds.Distinct().ToArray();
        }
    }

    public Snapshot Capture()
    {
        lock (_gate)
        {
            return new Snapshot(_kind, _color, _speed, _brightness, _deviceIds.ToArray());
        }
    }

    public readonly record struct Snapshot(
        EffectKind Kind,
        RgbColor Color,
        double Speed,
        double Brightness,
        int[] DeviceIds);
}

public sealed class EffectEngine : IDisposable
{
    public const int FramesPerSecond = 30;

    private readonly IRgbController _controller;
    private readonly object _loopGate = new();
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public EffectEngine(IRgbController controller)
    {
        _controller = controller;
        State = new EffectState();
    }

    public EffectState State { get; }

    public bool IsRunning
    {
        get
        {
            lock (_loopGate)
                return _loop is { IsCompleted: false };
        }
    }

    public event EventHandler<string>? Failed;

    public void Start(IReadOnlyList<RgbDeviceInfo> devices)
    {
        ArgumentNullException.ThrowIfNull(devices);
        lock (_loopGate)
        {
            if (_loop is { IsCompleted: false })
                return;

            foreach (var deviceId in State.Capture().DeviceIds)
            {
                try
                {
                    _controller.PrepareForEffects(deviceId);
                }
                catch (Exception)
                {
                    // Keep starting; the frame loop reports a hard failure.
                }
            }

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            var snapshot = devices.ToDictionary(device => device.Index);
            _loop = Task.Run(() => RunLoop(snapshot, token), token);
        }
    }

    public async Task StopAsync(bool restoreWhite = true)
    {
        Task? loop;
        CancellationTokenSource? cts;
        lock (_loopGate)
        {
            loop = _loop;
            cts = _cts;
            _loop = null;
            _cts = null;
        }

        if (cts is not null)
        {
            await cts.CancelAsync().ConfigureAwait(false);
        }

        if (loop is not null)
        {
            try
            {
                await loop.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
            }
        }

        cts?.Dispose();

        if (restoreWhite)
            RestoreWhite();
    }

    public void Dispose()
    {
        StopAsync(restoreWhite: true).GetAwaiter().GetResult();
    }

    private void RunLoop(Dictionary<int, RgbDeviceInfo> devices, CancellationToken token)
    {
        var delay = TimeSpan.FromMilliseconds(1000.0 / FramesPerSecond);
        var clock = System.Diagnostics.Stopwatch.StartNew();
        ILedEffect effect = new StaticEffect();
        var currentKind = EffectKind.Static;
        var buffers = new Dictionary<int, RgbColor[]>();

        try
        {
            while (!token.IsCancellationRequested)
            {
                var snapshot = State.Capture();
                if (snapshot.Kind != currentKind)
                {
                    effect = EffectFactory.Create(snapshot.Kind);
                    currentKind = snapshot.Kind;
                }

                var frame = new EffectFrame(clock.Elapsed.TotalSeconds, snapshot.Speed, snapshot.Brightness, snapshot.Color);
                foreach (var deviceId in snapshot.DeviceIds)
                {
                    if (!devices.TryGetValue(deviceId, out var device) || device.LedCount <= 0)
                        continue;

                    if (!buffers.TryGetValue(deviceId, out var buffer) || buffer.Length != device.LedCount)
                    {
                        buffer = new RgbColor[device.LedCount];
                        buffers[deviceId] = buffer;
                    }

                    effect.Render(frame, buffer);
                    _controller.UpdateLeds(deviceId, buffer);
                }

                if (!token.WaitHandle.WaitOne(delay))
                    continue;
                break;
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Failed?.Invoke(this, ex.Message);
        }
    }

    private void RestoreWhite()
    {
        var snapshot = State.Capture();
        foreach (var deviceId in snapshot.DeviceIds)
        {
            try
            {
                var devices = _controller.GetDevices();
                var device = devices.FirstOrDefault(item => item.Index == deviceId);
                if (device is null || device.LedCount <= 0)
                    continue;

                var white = new RgbColor[device.LedCount];
                Array.Fill(white, RgbColor.White);
                _controller.PrepareForEffects(deviceId);
                _controller.UpdateLeds(deviceId, white);
            }
            catch (Exception)
            {
            }
        }
    }
}
