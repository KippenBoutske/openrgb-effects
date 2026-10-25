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
