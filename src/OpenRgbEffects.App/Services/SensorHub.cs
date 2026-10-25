using OpenRgbEffects.Core;

namespace OpenRgbEffects.App.Services;

public sealed class SensorHub : ISensorSource, IDisposable
{
    private readonly AudioSensorService _audio = new();
    private readonly TemperatureSensorService _temperature = new();
    private readonly AmbientSensorService _ambient = new();
    private int _ticks;

    public void Start()
    {
        _audio.Start();
        _temperature.Start();
        _ambient.Refresh();
    }

    public SensorSnapshot Capture()
    {
        _ticks++;
        if (_ticks % 12 == 0)
            _ambient.Refresh();
        if (_ticks % 30 == 0)
            _temperature.Refresh();

        var snapshot = default(SensorSnapshot);
        _audio.CaptureInto(ref snapshot);
        _temperature.CaptureInto(ref snapshot);
        _ambient.CaptureInto(ref snapshot);
        return snapshot;
    }

    public void Dispose()
    {
        _audio.Dispose();
        _temperature.Dispose();
    }
}
