using NAudio.Wave;
using OpenRgbEffects.Core;

namespace OpenRgbEffects.App.Services;

public sealed class AudioSensorService : IDisposable
{
    private readonly object _gate = new();
    private WasapiLoopbackCapture? _capture;
    private double _level;
    private double _bass;
    private double _mid;
    private double _treble;
    private double _bassAverage = 0.08;
    private bool _beat;
    private bool _available;

    public void Start()
    {
        try
        {
            var capture = new WasapiLoopbackCapture();
            capture.DataAvailable += OnData;
            capture.RecordingStopped += (_, _) => { };
            capture.StartRecording();
            _capture = capture;
            _available = true;
        }
        catch (Exception)
        {
            _available = false;
        }
    }

    public void CaptureInto(ref SensorSnapshot snapshot)
    {
        lock (_gate)
        {
            snapshot = snapshot with
            {
                HasAudio = _available,
                AudioLevel = _level,
                Bass = _bass,
                Mid = _mid,
                Treble = _treble,
                Beat = _beat
            };
            _beat = false;
        }
    }

    public void Dispose()
    {
        if (_capture is null)
            return;
        try
        {
            _capture.StopRecording();
        }
        catch (Exception)
        {
        }
        _capture.Dispose();
        _capture = null;
    }

    private void OnData(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded < 8)
            return;

        var format = _capture?.WaveFormat;
        if (format is null || format.Encoding != WaveFormatEncoding.IeeeFloat)
            return;

        var samples = e.BytesRecorded / 4;
        var floats = new float[samples];
        Buffer.BlockCopy(e.Buffer, 0, floats, 0, e.BytesRecorded);

        double sum = 0, low = 0, mid = 0, high = 0;
        var prev = 0f;
        var lowState = 0.0;
        for (var i = 0; i < floats.Length; i++)
        {
            var sample = floats[i];
            sum += sample * sample;
            lowState += 0.06 * (sample - lowState);
            low += lowState * lowState;
            var hp = sample - prev;
            prev = sample;
            high += hp * hp;
            mid += (sample - lowState) * (sample - lowState);
        }

        var count = Math.Max(1, floats.Length);
        var level = ColorMath.Clamp01(Math.Sqrt(sum / count) * 3.4);
        var bass = ColorMath.Clamp01(Math.Sqrt(low / count) * 4.2);
        var midBand = ColorMath.Clamp01(Math.Sqrt(mid / count) * 2.8);
        var treble = ColorMath.Clamp01(Math.Sqrt(high / count) * 2.2);

        lock (_gate)
        {
            _level = (_level * 0.55) + (level * 0.45);
            _bass = (_bass * 0.5) + (bass * 0.5);
            _mid = (_mid * 0.55) + (midBand * 0.45);
            _treble = (_treble * 0.55) + (treble * 0.45);
            _bassAverage = (_bassAverage * 0.92) + (_bass * 0.08);
            if (_bass > Math.Max(0.12, _bassAverage * 1.45))
                _beat = true;
        }
    }
}
