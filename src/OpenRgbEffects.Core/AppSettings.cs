namespace OpenRgbEffects.Core;

public sealed class AppSettings
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 6742;
    public EffectKind Effect { get; set; } = EffectKind.Static;
    public string ColorHex { get; set; } = "#FFFFFF";
    public double Speed { get; set; } = 0.5;
    public double Brightness { get; set; } = 1.0;
    public List<string> SelectedDevices { get; set; } = [];
    public bool StartWithWindows { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool ResumeEffectOnStart { get; set; } = true;
    public bool WasRunning { get; set; }

    public RgbColor Color =>
        RgbColor.TryParseHex(ColorHex, out var color) ? color : RgbColor.White;

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
        Speed = ColorMath.Clamp01(Speed);
        Brightness = ColorMath.Clamp01(Brightness);
        SelectedDevices ??= [];
    }
}
