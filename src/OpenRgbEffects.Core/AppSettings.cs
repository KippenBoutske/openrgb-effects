namespace OpenRgbEffects.Core;

public sealed class AppSettings
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 6742;
    public EffectKind Effect { get; set; } = EffectKind.Static;
    public string ColorHex { get; set; } = "#FFFFFF";
    public string Color2Hex { get; set; } = "#3B82F6";
    public string PaletteId { get; set; } = "rainbow";
    public bool Reverse { get; set; }
    public double Speed { get; set; } = 0.5;
    public double Brightness { get; set; } = 1.0;
    public List<string> SelectedDevices { get; set; } = [];
    public List<string> Favorites { get; set; } = [];
    public bool StartWithWindows { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool ResumeEffectOnStart { get; set; } = true;
    public bool WasRunning { get; set; }

    public RgbColor Color =>
        RgbColor.TryParseHex(ColorHex, out var color) ? color : RgbColor.White;

    public RgbColor Color2 =>
        RgbColor.TryParseHex(Color2Hex, out var color) ? color : new RgbColor(59, 130, 246);

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(Host))
            Host = "127.0.0.1";
        if (Port is < 1 or > 65535)
            Port = 6742;
        if (!Enum.IsDefined(Effect))
            Effect = EffectKind.Static;
        if (!RgbColor.TryParseHex(ColorHex, out _))
            ColorHex = "#FFFFFF";
        if (!RgbColor.TryParseHex(Color2Hex, out _))
            Color2Hex = "#3B82F6";
        if (PalettePresets.All.All(item => !string.Equals(item.Id, PaletteId, StringComparison.OrdinalIgnoreCase)))
            PaletteId = "rainbow";
        Speed = ColorMath.Clamp01(Speed);
        Brightness = ColorMath.Clamp01(Brightness);
        SelectedDevices ??= [];
        Favorites ??= [];
        Favorites = Favorites
            .Where(name => Enum.TryParse<EffectKind>(name, out _))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
