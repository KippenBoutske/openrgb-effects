namespace OpenRgbEffects.Core;

public static class ColorMath
{
    public static RgbColor Scale(RgbColor color, double brightness)
    {
        var level = Clamp01(brightness);
        return new RgbColor(
            ToByte(color.R * level),
            ToByte(color.G * level),
            ToByte(color.B * level));
    }

    public static RgbColor FromHsv(double hue, double saturation, double value)
    {
        var h = ((hue % 360) + 360) % 360;
        var s = Clamp01(saturation);
        var v = Clamp01(value);

        if (s <= 0)
        {
            var gray = ToByte(v * 255);
            return new RgbColor(gray, gray, gray);
        }

        var sector = h / 60.0;
        var index = (int)Math.Floor(sector);
        var f = sector - index;
        var p = v * (1 - s);
        var q = v * (1 - s * f);
        var t = v * (1 - s * (1 - f));

        var (r, g, b) = index switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q)
        };

        return new RgbColor(ToByte(r * 255), ToByte(g * 255), ToByte(b * 255));
    }

    public static (double Hue, double Saturation, double Value) ToHsv(RgbColor color)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        var hue = 0.0;
        if (delta > 0)
        {
            if (Math.Abs(max - r) < double.Epsilon)
                hue = 60 * (((g - b) / delta) % 6);
            else if (Math.Abs(max - g) < double.Epsilon)
                hue = 60 * (((b - r) / delta) + 2);
            else
                hue = 60 * (((r - g) / delta) + 4);
        }

        if (hue < 0)
            hue += 360;

        var saturation = max <= 0 ? 0 : delta / max;
        return (hue, saturation, max);
    }

    public static RgbColor Lerp(RgbColor a, RgbColor b, double t)
    {
        var mix = Clamp01(t);
        return new RgbColor(
            ToByte(a.R + ((b.R - a.R) * mix)),
            ToByte(a.G + ((b.G - a.G) * mix)),
            ToByte(a.B + ((b.B - a.B) * mix)));
    }

    public static RgbColor SamplePalette(IReadOnlyList<RgbColor>? palette, double t)
    {
        if (palette is null || palette.Count == 0)
            return RgbColor.White;
        if (palette.Count == 1)
            return palette[0];

        var wrapped = t - Math.Floor(t);
        if (wrapped < 0)
            wrapped += 1;
        var scaled = wrapped * palette.Count;
        var index = (int)Math.Floor(scaled) % palette.Count;
        var next = (index + 1) % palette.Count;
        return Lerp(palette[index], palette[next], scaled - Math.Floor(scaled));
    }

    public static IReadOnlyList<RgbColor> EffectivePalette(IReadOnlyList<RgbColor>? palette, RgbColor color, RgbColor color2)
    {
        if (palette is { Count: > 1 })
            return palette;
        if (color2 is { R: 0, G: 0, B: 0 } && color == RgbColor.White)
            return PalettePresets.Rainbow.Colors;
        return [color, color2.R == 0 && color2.G == 0 && color2.B == 0 ? RgbColor.White : color2];
    }

    public static int DirectedIndex(int index, int count, int direction)
    {
        if (count <= 0)
            return 0;
        return direction < 0 ? count - 1 - index : index;
    }

    public static double Fract(double value)
    {
        var f = value - Math.Floor(value);
        return f < 0 ? f + 1 : f;
    }

    public static double Wave01(double time, double cyclesPerSecond) =>
        0.5 - (0.5 * Math.Cos(time * cyclesPerSecond * Math.PI * 2));

    public static uint Hash(int value)
    {
        unchecked
        {
            var x = (uint)value;
            x ^= 2747636419u;
            x *= 2654435769u;
            x ^= x >> 16;
            x *= 2654435769u;
            x ^= x >> 16;
            return x;
        }
    }

    public static double Hash01(int value) => Hash(value) / (double)uint.MaxValue;

    public static double Clamp01(double value) => value < 0 ? 0 : value > 1 ? 1 : value;

    public static byte ToByte(double value)
    {
        if (value <= 0)
            return 0;
        if (value >= 255)
            return 255;
        return (byte)Math.Round(value, MidpointRounding.AwayFromZero);
    }
}
