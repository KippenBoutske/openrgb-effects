using OpenRgbEffects.Core;

namespace OpenRgbEffects.Tests;

internal sealed class FakeRgbController : IRgbController
{
    private readonly List<RgbDeviceInfo> _devices;
    public readonly List<(int DeviceId, RgbColor[] Colors)> Updates = [];
    public readonly List<int> Prepared = [];
    public bool Connected { get; set; } = true;
    public bool ThrowOnUpdate { get; set; }

    public FakeRgbController(params RgbDeviceInfo[] devices)
    {
        _devices = devices.ToList();
    }

    public event EventHandler? DeviceListUpdated;

    public void Connect(string host, int port) => Connected = true;

    public void Disconnect() => Connected = false;

    public IReadOnlyList<RgbDeviceInfo> GetDevices() => _devices;

    public void PrepareForEffects(int deviceId) => Prepared.Add(deviceId);

    public void UpdateLeds(int deviceId, ReadOnlySpan<RgbColor> colors)
    {
        if (ThrowOnUpdate)
            throw new InvalidOperationException("SDK-fout");
        Updates.Add((deviceId, colors.ToArray()));
    }

    public void Dispose()
    {
    }

    public void RaiseDeviceListUpdated() => DeviceListUpdated?.Invoke(this, EventArgs.Empty);
}
