namespace OpenRgbEffects.Core.Effects;

public sealed class StrobeEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Strobe;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var hz = 2 + (frame.Speed * 10);
        var on = ColorMath.Fract(frame.TimeSeconds * hz) < 0.45;
        var color = on ? ColorMath.Scale(frame.Color, frame.Brightness) : RgbColor.Black;
        leds.Fill(color);
    }
}

public sealed class PoliceEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Police;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var red = frame.Color.R > 20 ? frame.Color : new RgbColor(220, 8, 24);
        var blue = frame.Color2.B > 20 ? frame.Color2 : new RgbColor(12, 48, 255);
        var flip = ColorMath.Fract(frame.TimeSeconds * (1.5 + frame.Speed * 4)) < 0.5;
        var left = ColorMath.Scale(flip ? red : blue, frame.Brightness);
        var right = ColorMath.Scale(flip ? blue : red, frame.Brightness);
        var mid = leds.Length / 2;
        for (var i = 0; i < leds.Length; i++)
            leds[i] = i < Math.Max(1, mid) ? left : right;
    }
}

public sealed class HeartbeatEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Heartbeat;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var bpm = 50 + (frame.Speed * 70);
        var beat = ColorMath.Fract(frame.TimeSeconds * bpm / 60.0);
        var pulse = beat < 0.12
            ? 1 - (beat / 0.12)
            : beat is >= 0.18 and < 0.30
                ? 0.85 * (1 - ((beat - 0.18) / 0.12))
                : 0.08;
        leds.Fill(ColorMath.Scale(frame.Color, frame.Brightness * ColorMath.Clamp01(pulse)));
    }
}

public sealed class RandomLedsEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.RandomLeds;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var palette = frame.Colors;
        var hold = 0.35 + ((1 - frame.Speed) * 1.4);
        var slot = (int)Math.Floor(frame.TimeSeconds / hold);
        for (var i = 0; i < leds.Length; i++)
        {
            var a = ColorMath.SamplePalette(palette, ColorMath.Hash01(i + slot * 17));
            var b = ColorMath.SamplePalette(palette, ColorMath.Hash01(i + (slot + 1) * 17));
            var t = ColorMath.Fract(frame.TimeSeconds / hold);
            leds[i] = ColorMath.Scale(ColorMath.Lerp(a, b, t * t * (3 - 2 * t)), frame.Brightness);
        }
    }
}

public sealed class ColorFadeEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.ColorFade;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var color = ColorMath.Scale(
            ColorMath.SamplePalette(frame.Colors, frame.TimeSeconds * (0.08 + frame.Speed * 0.35)),
            frame.Brightness);
        leds.Fill(color);
    }
}

public sealed class BreathingMultiEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.BreathingMulti;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var wave = ColorMath.Wave01(frame.TimeSeconds, 0.15 + frame.Speed * 0.7);
        var level = 0.08 + (0.92 * wave);
        var color = ColorMath.SamplePalette(frame.Colors, frame.TimeSeconds * (0.05 + frame.Speed * 0.12));
        leds.Fill(ColorMath.Scale(color, frame.Brightness * level));
    }
}
