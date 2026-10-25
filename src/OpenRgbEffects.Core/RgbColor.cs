using System.Globalization;

namespace OpenRgbEffects.Core;

public readonly record struct RgbColor(byte R, byte G, byte B)
{
    public static RgbColor White { get; } = new(255, 255, 255);
    public static RgbColor Black { get; } = new(0, 0, 0);

    public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";

    public static bool TryParseHex(string? value, out RgbColor color)
    {
        color = White;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var hex = value.Trim();
        if (hex.StartsWith('#'))
            hex = hex[1..];
        if (hex.Length != 6)
            return false;
        if (!byte.TryParse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r))
            return false;
        if (!byte.TryParse(hex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g))
            return false;
        if (!byte.TryParse(hex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
            return false;

        color = new RgbColor(r, g, b);
        return true;
    }
}
