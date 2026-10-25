using OpenRGB.NET;

namespace OpenRgbEffects.Core;

public sealed class OpenRgbController : IRgbController
{
    private readonly object _sync = new();
    private OpenRgbClient? _client;

    public bool Connected
    {
        get
        {
            lock (_sync)
                return _client?.Connected == true;
        }
    }

    public event EventHandler? DeviceListUpdated;

    public void Connect(string host, int port)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        if (port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port));

        lock (_sync)
        {
            DisconnectCore();
            var client = new OpenRgbClient(
                ip: host.Trim(),
                port: port,
                name: "OpenRGB Effects",
                autoConnect: false,
                timeoutMs: 2500);
            client.DeviceListUpdated += OnDeviceListUpdated;
            client.Connect();
            _client = client;
        }
    }

    public void Disconnect()
    {
        lock (_sync)
            DisconnectCore();
    }

    public IReadOnlyList<RgbDeviceInfo> GetDevices()
    {
        lock (_sync)
        {
            var client = RequireClient();
            return client.GetAllControllerData().Select(MapDevice).ToArray();
        }
    }

    public void PrepareForEffects(int deviceId)
    {
        lock (_sync)
        {
            var client = RequireClient();
            try
            {
                client.SetCustomMode(deviceId);
            }
            catch (Exception)
            {
                // Some controllers expose Direct instead of Custom. Ignore and keep streaming.
            }

            try
            {
                var device = client.GetControllerData(deviceId);
                var direct = device.Modes.FirstOrDefault(mode =>
                    mode.Name.Contains("Direct", StringComparison.OrdinalIgnoreCase) ||
                    mode.Name.Contains("Custom", StringComparison.OrdinalIgnoreCase));
                if (direct is not null)
                    client.UpdateMode(deviceId, direct.Index);
            }
            catch (Exception)
            {
                // Streaming UpdateLeds still works on many devices without an explicit mode switch.
            }
        }
    }

    public void UpdateLeds(int deviceId, ReadOnlySpan<RgbColor> colors)
    {
        if (colors.IsEmpty)
            return;

        lock (_sync)
        {
            var client = RequireClient();
            Span<Color> mapped = colors.Length <= 256
                ? stackalloc Color[colors.Length]
                : new Color[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                mapped[i] = new Color(color.R, color.G, color.B);
            }

            client.UpdateLeds(deviceId, mapped);
        }
    }

    public void Dispose() => Disconnect();

    private void OnDeviceListUpdated(object? sender, EventArgs e) =>
        DeviceListUpdated?.Invoke(this, EventArgs.Empty);

    private OpenRgbClient RequireClient()
    {
        if (_client is not { Connected: true })
            throw new InvalidOperationException("Niet verbonden met de OpenRGB SDK-server.");
        return _client;
    }

    private void DisconnectCore()
    {
        if (_client is null)
            return;

        _client.DeviceListUpdated -= OnDeviceListUpdated;
        _client.Dispose();
        _client = null;
    }

    private static RgbDeviceInfo MapDevice(Device device)
    {
        var zones = device.Zones.Select(zone => new RgbZoneInfo(
            zone.Index,
            zone.Name,
            zone.Type.ToString(),
            (int)zone.LedCount)).ToArray();

        return new RgbDeviceInfo(
            device.Index,
            device.Name,
            device.Type.ToString(),
            device.Vendor,
            device.Description,
            device.Leds.Length,
            zones);
    }
}
