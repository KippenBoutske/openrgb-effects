namespace OpenRgbEffects.Core.Effects;

public static class EffectFactory
{
    public static ILedEffect Create(EffectKind kind) => kind switch
    {
        EffectKind.Static => new StaticEffect(),
        EffectKind.Breathing => new BreathingEffect(),
        EffectKind.Rainbow => new RainbowEffect(),
        EffectKind.Wave => new WaveEffect(),
        _ => new StaticEffect()
    };
}
