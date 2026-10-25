namespace OpenRgbEffects.Core.Effects;

public sealed class RainbowEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Rainbow;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var degreesPerSecond = 40 + (frame.Speed * 140);
        var hue = frame.TimeSeconds * degreesPerSecond;
        var color = ColorMath.Scale(ColorMath.FromHsv(hue, 1, 1), frame.Brightness);
        leds.Fill(color);
    }
}
