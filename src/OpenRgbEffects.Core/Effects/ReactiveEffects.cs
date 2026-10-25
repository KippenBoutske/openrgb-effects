namespace OpenRgbEffects.Core.Effects;

public sealed class MusicVuEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.MusicVu;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        if (leds.IsEmpty)
            return;

        var level = frame.Sensors.HasAudio
            ? ColorMath.Clamp01(frame.Sensors.AudioLevel * 1.35)
            : ColorMath.Wave01(frame.TimeSeconds, 0.7) * 0.45;
        var filled = level * leds.Length;
        var palette = frame.Colors;
        for (var i = 0; i < leds.Length; i++)
        {
            var on = ColorMath.Clamp01(filled - i);
            var color = ColorMath.SamplePalette(palette, i / (double)Math.Max(1, leds.Length - 1));
            leds[i] = ColorMath.Scale(color, frame.Brightness * on);
        }
    }
}

public sealed class MusicBeatEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.MusicBeat;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var pulse = frame.Sensors.HasAudio
            ? (frame.Sensors.Beat ? 1.0 : 0.12 + frame.Sensors.Bass * 0.55)
            : ColorMath.Wave01(frame.TimeSeconds, 1.2);
        leds.Fill(ColorMath.Scale(frame.Color, frame.Brightness * ColorMath.Clamp01(pulse)));
    }
}

public sealed class MusicSpectrumEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.MusicSpectrum;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        if (leds.IsEmpty)
            return;

        var bass = frame.Sensors.HasAudio ? frame.Sensors.Bass : ColorMath.Wave01(frame.TimeSeconds, 0.8);
        var mid = frame.Sensors.HasAudio ? frame.Sensors.Mid : ColorMath.Wave01(frame.TimeSeconds + 0.3, 0.6);
        var treble = frame.Sensors.HasAudio ? frame.Sensors.Treble : ColorMath.Wave01(frame.TimeSeconds + 0.6, 1.1);
        var palette = frame.Colors;
        var n = leds.Length;
        for (var i = 0; i < n; i++)
        {
            var t = i / (double)Math.Max(1, n - 1);
            var energy = t < 0.33
                ? bass
                : t < 0.66
                    ? ColorMath.Lerp(new RgbColor((byte)(bass * 255), 0, 0), new RgbColor((byte)(mid * 255), 0, 0), (t - 0.33) / 0.33).R / 255.0
                    : ColorMath.Lerp(new RgbColor((byte)(mid * 255), 0, 0), new RgbColor((byte)(treble * 255), 0, 0), (t - 0.66) / 0.34).R / 255.0;
            var color = ColorMath.SamplePalette(palette, t);
            leds[i] = ColorMath.Scale(color, frame.Brightness * ColorMath.Clamp01(0.08 + energy));
        }
    }
}

public sealed class TemperatureEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Temperature;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        double temp;
        if (frame.Sensors.HasTemperature)
            temp = Math.Max(frame.Sensors.CpuTempC, frame.Sensors.GpuTempC);
        else
            temp = 42 + (8 * Math.Sin(frame.TimeSeconds * 0.15));

        var t = ColorMath.Clamp01((temp - 35) / 55.0);
        var cold = new RgbColor(40, 140, 255);
        var warm = new RgbColor(255, 200, 40);
        var hot = new RgbColor(255, 32, 24);
        var color = t < 0.5
            ? ColorMath.Lerp(cold, warm, t * 2)
            : ColorMath.Lerp(warm, hot, (t - 0.5) * 2);
        leds.Fill(ColorMath.Scale(color, frame.Brightness));
    }
}

public sealed class ScreenAmbientEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.ScreenAmbient;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var color = frame.Sensors.HasAmbient && (frame.Sensors.Ambient.R | frame.Sensors.Ambient.G | frame.Sensors.Ambient.B) != 0
            ? frame.Sensors.Ambient
            : ColorMath.FromHsv(frame.TimeSeconds * 12, 0.35, 0.55);
        var breathe = 0.82 + (0.18 * Math.Sin(frame.TimeSeconds * 1.4));
        leds.Fill(ColorMath.Scale(color, frame.Brightness * breathe));
    }
}
