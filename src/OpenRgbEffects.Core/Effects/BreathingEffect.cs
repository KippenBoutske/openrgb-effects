namespace OpenRgbEffects.Core.Effects;

public sealed class BreathingEffect : ILedEffect
{
    public EffectKind Kind => EffectKind.Breathing;

    public void Render(in EffectFrame frame, Span<RgbColor> leds)
    {
        var cyclesPerSecond = 0.15 + (frame.Speed * 0.85);
        var wave = 0.5 - (0.5 * Math.Cos(frame.TimeSeconds * cyclesPerSecond * Math.PI * 2));
        var level = 0.08 + (0.92 * wave);
        var color = ColorMath.Scale(frame.Color, frame.Brightness * level);
        leds.Fill(color);
    }
}
