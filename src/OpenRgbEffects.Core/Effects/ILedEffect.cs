namespace OpenRgbEffects.Core.Effects;

public readonly record struct EffectFrame(
    double TimeSeconds,
    double Speed,
    double Brightness,
    RgbColor Color)
{
    public RgbColor Color2 { get; init; }
    public IReadOnlyList<RgbColor> Palette { get; init; } = [];
    public int Direction { get; init; } = 1;
    public SensorSnapshot Sensors { get; init; }

    public IReadOnlyList<RgbColor> Colors => ColorMath.EffectivePalette(Palette, Color, Color2);
}

public interface ILedEffect
{
    EffectKind Kind { get; }

    void Render(in EffectFrame frame, Span<RgbColor> leds);
}
