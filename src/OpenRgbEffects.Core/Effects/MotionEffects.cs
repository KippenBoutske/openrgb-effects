namespace OpenRgbEffects.Core.Effects;

public sealed class ColorWipeEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.ColorWipe;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        if (leds.IsEmpty)
            return;

        var palette = frame.Colors;
        var rate = 0.35 + (frame.Speed * 1.4);
        var cycle = frame.TimeSeconds * rate;
        var colorIndex = ((int)Math.Floor(cycle) % palette.Count + palette.Count) % palette.Count;
        var fill = ColorMath.Fract(cycle);
        var color = ColorMath.Scale(palette[colorIndex], frame.Brightness);
        var prev = ColorMath.Scale(palette[(colorIndex - 1 + palette.Count) % palette.Count], frame.Brightness);
        var count = leds.Length;
        var filled = (int)Math.Floor(fill * (count + 1));
        for (var i = 0; i < count; i++)
        {
            var index = ColorMath.DirectedIndex(i, count, frame.Direction);
            leds[index] = i < filled ? color : prev;
        }
    }
}

public sealed class TheaterChaseEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.TheaterChase;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var palette = frame.Colors;
        var step = (int)Math.Floor(frame.TimeSeconds * (2 + frame.Speed * 10));
        if (frame.Direction < 0)
            step = -step;
        var on = ColorMath.Scale(ColorMath.SamplePalette(palette, frame.TimeSeconds * 0.15), frame.Brightness);
        var off = ColorMath.Scale(on, 0.06);
        for (var i = 0; i < leds.Length; i++)
            leds[i] = ((i + step) % 3 == 0) ? on : off;
    }
}

public sealed class CometEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Comet;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        if (leds.IsEmpty)
            return;

        var n = leds.Length;
        var speed = 8 + (frame.Speed * 28);
        var pos = ColorMath.Fract(frame.TimeSeconds * speed / Math.Max(1, n)) * n;
        if (frame.Direction < 0)
            pos = n - pos;
        var head = ColorMath.Scale(frame.Color, frame.Brightness);
        var tail = Math.Max(4, n / 3);
        for (var i = 0; i < n; i++)
        {
            var dist = Math.Abs(i - pos);
            dist = Math.Min(dist, n - dist);
            var fall = Math.Exp(-dist * (4.5 / tail));
            leds[i] = ColorMath.Scale(head, fall);
        }
    }
}

public sealed class ScannerEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Scanner;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        if (leds.IsEmpty)
            return;

        var n = leds.Length;
        var period = 1.6 / (0.25 + frame.Speed);
        var t = ColorMath.Wave01(frame.TimeSeconds, 1.0 / period);
        var pos = t * Math.Max(0, n - 1);
        var head = ColorMath.Scale(frame.Color, frame.Brightness);
        var width = Math.Max(1.2, n / 10.0);
        for (var i = 0; i < n; i++)
        {
            var fall = Math.Exp(-Math.Abs(i - pos) / width);
            leds[i] = ColorMath.Scale(head, fall);
        }
    }
}

public sealed class RainbowChaseEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.RainbowChase;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var travel = frame.TimeSeconds * (50 + frame.Speed * 160) * (frame.Direction < 0 ? -1 : 1);
        if (leds.Length == 1)
        {
            leds[0] = ColorMath.Scale(ColorMath.FromHsv(travel, 1, 1), frame.Brightness);
            return;
        }

        for (var i = 0; i < leds.Length; i++)
        {
            var hue = travel + (360.0 * i / leds.Length);
            leds[i] = ColorMath.Scale(ColorMath.FromHsv(hue, 1, 1), frame.Brightness);
        }
    }
}

public sealed class BouncingBallsEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.BouncingBalls;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        leds.Clear();
        if (leds.IsEmpty)
            return;

        var n = leds.Length;
        var palette = frame.Colors;
        var balls = Math.Clamp(2 + (n / 16), 2, 5);
        for (var ball = 0; ball < balls; ball++)
        {
            var phase = (frame.TimeSeconds * (0.55 + ball * 0.17) * (0.4 + frame.Speed)) + (ball * 0.37);
            var bounce = Math.Abs(Math.Sin(phase * Math.PI));
            var height = Math.Pow(bounce, 0.65);
            var pos = height * (n - 1);
            var color = ColorMath.Scale(palette[ball % palette.Count], frame.Brightness);
            for (var i = 0; i < n; i++)
            {
                var fall = Math.Exp(-Math.Abs(i - pos) * 2.4);
                var mixed = ColorMath.Lerp(leds[i], color, fall);
                leds[i] = mixed;
            }
        }
    }
}

public sealed class GradientSpinEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.GradientSpin;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var palette = frame.Colors;
        var offset = frame.TimeSeconds * (0.15 + frame.Speed * 0.7) * (frame.Direction < 0 ? -1 : 1);
        for (var i = 0; i < leds.Length; i++)
        {
            var t = (i / (double)Math.Max(1, leds.Length)) + offset;
            leds[i] = ColorMath.Scale(ColorMath.SamplePalette(palette, t), frame.Brightness);
        }
    }
}
