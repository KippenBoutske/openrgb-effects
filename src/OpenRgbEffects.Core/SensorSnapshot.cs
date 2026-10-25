namespace OpenRgbEffects.Core;

public readonly record struct SensorSnapshot(
    double AudioLevel,
    double Bass,
    double Mid,
    double Treble,
    bool Beat,
    double CpuTempC,
    double GpuTempC,
    bool HasAudio,
    bool HasTemperature,
    bool HasAmbient,
    RgbColor Ambient);

public interface ISensorSource
{
    SensorSnapshot Capture();
}

public sealed class NullSensorSource : ISensorSource
{
    public static NullSensorSource Instance { get; } = new();

    public SensorSnapshot Capture() => default;
}
