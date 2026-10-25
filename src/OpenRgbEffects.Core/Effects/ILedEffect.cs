namespace OpenRgbEffects.Core.Effects;

public readonly record struct EffectFrame(
    double TimeSeconds,
    double Speed,
    double Brightness,
    RgbColor Color);

public interface ILedEffect
{
    EffectKind Kind { get; }

    void Render(in EffectFrame frame, Span<RgbColor> leds);
}
