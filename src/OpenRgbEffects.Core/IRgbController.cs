namespace OpenRgbEffects.Core;

public interface IRgbController : IDisposable
{
    bool Connected { get; }

    event EventHandler? DeviceListUpdated;

    void Connect(string host, int port);

    void Disconnect();

    IReadOnlyList<RgbDeviceInfo> GetDevices();

    void PrepareForEffects(int deviceId);

    void UpdateLeds(int deviceId, ReadOnlySpan<RgbColor> colors);
}
