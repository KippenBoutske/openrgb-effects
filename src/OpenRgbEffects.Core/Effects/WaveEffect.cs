namespace OpenRgbEffects.Core.Effects;

public sealed class WaveEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Wave;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        if (leds.IsEmpty)
            return;

        var degreesPerSecond = 50 + (frame.Speed * 160);
        var travel = frame.TimeSeconds * degreesPerSecond * (frame.Direction < 0 ? -1 : 1);

        if (leds.Length == 1)
        {
            leds[0] = ColorMath.Scale(ColorMath.FromHsv(travel, 1, 1), frame.Brightness);
            return;
        }

        for (var i = 0; i < leds.Length; i++)
        {
            var hue = travel + (360.0 * i / leds.Length);
            leds[i] = ColorMath.Scale(ColorMath.FromHsv(hue, 1, 1), frame.Brightness);
        }
    }
}
