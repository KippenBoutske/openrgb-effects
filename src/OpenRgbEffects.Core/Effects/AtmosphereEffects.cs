namespace OpenRgbEffects.Core.Effects;

public sealed class TwinkleEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Twinkle;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var palette = frame.Colors;
        var density = 0.12 + (frame.Speed * 0.25);
        for (var i = 0; i < leds.Length; i++)
        {
            var spark = ColorMath.Hash01(i * 97 + (int)(frame.TimeSeconds * (8 + frame.Speed * 18)));
            var phase = ColorMath.Wave01(frame.TimeSeconds + ColorMath.Hash01(i + 3) * 4, 0.4 + frame.Speed);
            var on = spark < density ? phase : phase * 0.08;
            var color = ColorMath.SamplePalette(palette, ColorMath.Hash01(i + 11));
            leds[i] = ColorMath.Scale(color, frame.Brightness * on);
        }
    }
}

public sealed class FireEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Fire;
    private float[] _heat = [];
    private float[] _next = [];

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var n = leds.Length;
        if (_heat.Length != n)
        {
            _heat = new float[n];
            _next = new float[n];
        }

        var cooling = 0.04f + (float)((1 - frame.Speed) * 0.08);
        var spark = 0.35f + (float)(frame.Speed * 0.45);
        for (var i = 0; i < n; i++)
            _heat[i] = Math.Max(0, _heat[i] - cooling * (0.5f + (float)ColorMath.Hash01(i + (int)(frame.TimeSeconds * 40))));

        if (n >= 3)
        {
            for (var i = 1; i < n - 1; i++)
                _next[i] = (_heat[i - 1] + _heat[i] + _heat[i + 1]) / 3f;
            _next[0] = (_heat[0] + _heat[1]) / 2f;
            _next[n - 1] = (_heat[n - 1] + _heat[n - 2]) / 2f;
            (_heat, _next) = (_next, _heat);
        }

        var seeds = Math.Max(1, n / 8);
        for (var s = 0; s < seeds; s++)
        {
            if (ColorMath.Hash01(s * 31 + (int)(frame.TimeSeconds * 30)) < spark)
            {
                var idx = (int)(ColorMath.Hash01(s * 17 + (int)(frame.TimeSeconds * 17)) * n);
                _heat[idx] = Math.Min(1f, _heat[idx] + 0.65f);
            }
        }

        var palette = frame.Palette is { Count: > 1 } ? frame.Palette : PalettePresets.Fire.Colors;
        for (var i = 0; i < n; i++)
            leds[i] = ColorMath.Scale(ColorMath.SamplePalette(palette, _heat[i] * 0.92), frame.Brightness * (0.35 + 0.65 * _heat[i]));
    }
}

public sealed class CandleEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Candle;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var flicker = 0.72 + (0.28 * Math.Sin(frame.TimeSeconds * (9 + frame.Speed * 14)))
                      + (0.08 * Math.Sin(frame.TimeSeconds * 27.3));
        flicker = ColorMath.Clamp01(flicker);
        var baseColor = frame.Color.R == 255 && frame.Color.G == 255 && frame.Color.B == 255
            ? new RgbColor(255, 140, 40)
            : frame.Color;
        for (var i = 0; i < leds.Length; i++)
        {
            var local = flicker - 0.08 * ColorMath.Hash01(i + (int)(frame.TimeSeconds * 8));
            leds[i] = ColorMath.Scale(baseColor, frame.Brightness * ColorMath.Clamp01(local));
        }
    }
}

public sealed class PlasmaEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Plasma;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var palette = frame.Colors;
        var t = frame.TimeSeconds * (0.4 + frame.Speed * 1.4);
        for (var i = 0; i < leds.Length; i++)
        {
            var x = i / (double)Math.Max(1, leds.Length - 1);
            var v = 0.5 + 0.5 * Math.Sin((x * 6.2) + t);
            v += 0.5 + 0.5 * Math.Sin((x * 14.1) - t * 1.3);
            v += 0.5 + 0.5 * Math.Sin(((x + t) * 8.7));
            leds[i] = ColorMath.Scale(ColorMath.SamplePalette(palette, v / 3.0), frame.Brightness);
        }
    }
}

public sealed class AuroraEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Aurora;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var palette = frame.Palette is { Count: > 1 } ? frame.Palette : PalettePresets.Neon.Colors;
        var t = frame.TimeSeconds * (0.12 + frame.Speed * 0.45);
        for (var i = 0; i < leds.Length; i++)
        {
            var x = i / (double)Math.Max(1, leds.Length);
            var curtain = 0.5 + 0.5 * Math.Sin((x * 5.5) + t);
            var veil = 0.5 + 0.5 * Math.Sin((x * 2.2) - t * 0.7);
            var color = ColorMath.Lerp(
                ColorMath.SamplePalette(palette, curtain),
                ColorMath.SamplePalette(palette, 0.35 + veil * 0.5),
                0.45);
            leds[i] = ColorMath.Scale(color, frame.Brightness * (0.45 + 0.55 * curtain));
        }
    }
}

public sealed class OceanEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Ocean;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var palette = frame.Palette is { Count: > 1 } ? frame.Palette : PalettePresets.Ocean.Colors;
        var t = frame.TimeSeconds * (0.2 + frame.Speed * 0.7);
        for (var i = 0; i < leds.Length; i++)
        {
            var x = i / (double)Math.Max(1, leds.Length);
            var wave = 0.5 + 0.5 * Math.Sin((x * 8) - t * 2);
            var swell = 0.5 + 0.5 * Math.Sin((x * 3) + t);
            leds[i] = ColorMath.Scale(ColorMath.SamplePalette(palette, (wave * 0.65) + (swell * 0.35)), frame.Brightness);
        }
    }
}

public sealed class LavaEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Lava;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var palette = frame.Palette is { Count: > 1 } ? frame.Palette : PalettePresets.Fire.Colors;
        var t = frame.TimeSeconds * (0.08 + frame.Speed * 0.25);
        for (var i = 0; i < leds.Length; i++)
        {
            var x = i * 0.37;
            var blob = 0.5 + 0.5 * Math.Sin(x + t);
            blob *= 0.5 + 0.5 * Math.Sin((x * 0.6) - t * 1.7);
            leds[i] = ColorMath.Scale(ColorMath.SamplePalette(palette, blob), frame.Brightness);
        }
    }
}

public sealed class StarfieldEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Starfield;
    private const int StarCount = 12;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        leds.Clear();
        if (leds.IsEmpty)
            return;

        var n = leds.Length;
        var color = frame.Color.R == 255 && frame.Color.G == 255 && frame.Color.B == 255
            ? RgbColor.White
            : frame.Color;
        for (var s = 0; s < StarCount; s++)
        {
            var seed = ColorMath.Hash01(s * 19 + 4);
            var z = ColorMath.Fract(seed + frame.TimeSeconds * (0.08 + frame.Speed * 0.35) * (0.4 + seed));
            var pos = seed * (n - 1);
            var brightness = 0.15 + (0.85 * z);
            var i = (int)Math.Round(pos);
            if ((uint)i < (uint)n)
                leds[i] = ColorMath.Lerp(leds[i], ColorMath.Scale(color, frame.Brightness * brightness), 0.85);
        }
    }
}

public sealed class LightningEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Lightning;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var bolt = frame.Color2.R == 0 && frame.Color2.G == 0 && frame.Color2.B == 0
            ? new RgbColor(200, 220, 255)
            : frame.Color2;
        var flashGate = ColorMath.Hash01((int)(frame.TimeSeconds * (1.2 + frame.Speed * 3)));
        var strike = flashGate > 0.92;
        var decay = strike ? 1.0 : Math.Max(0, 0.92 - ColorMath.Fract(frame.TimeSeconds * 4) * 3);
        leds.Fill(ColorMath.Scale(new RgbColor(6, 8, 16), frame.Brightness * 0.35));
        if (decay <= 0 || leds.IsEmpty)
            return;

        var start = (int)(ColorMath.Hash01((int)(frame.TimeSeconds * 7) + 3) * leds.Length);
        var length = Math.Max(2, leds.Length / 4);
        for (var i = 0; i < length; i++)
        {
            var idx = (start + i) % leds.Length;
            var fall = 1.0 - (i / (double)length);
            leds[idx] = ColorMath.Scale(bolt, frame.Brightness * decay * fall);
        }
    }
}

public sealed class MatrixRainEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.MatrixRain;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        if (leds.IsEmpty)
            return;

        leds.Fill(new RgbColor(0, 8, 0));
        var n = leds.Length;
        var drops = Math.Clamp(n / 6, 2, 8);
        var color = frame.Color.G > 40 ? frame.Color : new RgbColor(20, 255, 70);
        var speed = 6 + (frame.Speed * 16);
        for (var d = 0; d < drops; d++)
        {
            var offset = ColorMath.Hash01(d * 13) * n;
            var head = ColorMath.Fract((frame.TimeSeconds * speed / n) + (offset / n)) * n;
            if (frame.Direction < 0)
                head = n - head;
            var tail = Math.Max(3, n / 5);
            for (var t = 0; t < tail; t++)
            {
                var idx = ((int)Math.Round(head) - t + n * 4) % n;
                var fall = 1.0 - (t / (double)tail);
                leds[idx] = ColorMath.Lerp(leds[idx], ColorMath.Scale(color, frame.Brightness * fall), 0.9);
            }
        }
    }
}
