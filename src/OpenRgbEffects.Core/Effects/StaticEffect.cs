namespace OpenRgbEffects.Core.Effects;

public sealed class StaticEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Static;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var color = ColorMath.Scale(frame.Color, frame.Brightness);
        leds.Fill(color);
    }
}
