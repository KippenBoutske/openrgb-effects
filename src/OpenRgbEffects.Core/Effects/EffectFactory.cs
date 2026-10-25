namespace OpenRgbEffects.Core.Effects;

public static class EffectFactory
{
    public static ILedEffect Create(EffectKind kind) => kind switch
    {
        EffectKind.Static => new StaticEffect(),
        EffectKind.Breathing => new BreathingEffect(),
        EffectKind.Rainbow => new RainbowEffect(),
        EffectKind.Wave => new WaveEffect(),
        EffectKind.ColorWipe => new ColorWipeEffect(),
        EffectKind.TheaterChase => new TheaterChaseEffect(),
        EffectKind.Comet => new CometEffect(),
        EffectKind.Scanner => new ScannerEffect(),
        EffectKind.Twinkle => new TwinkleEffect(),
        EffectKind.Fire => new FireEffect(),
        EffectKind.Candle => new CandleEffect(),
        EffectKind.Plasma => new PlasmaEffect(),
        EffectKind.Aurora => new AuroraEffect(),
        EffectKind.Ocean => new OceanEffect(),
        EffectKind.Lava => new LavaEffect(),
        EffectKind.Strobe => new StrobeEffect(),
        EffectKind.Police => new PoliceEffect(),
        EffectKind.Heartbeat => new HeartbeatEffect(),
        EffectKind.ColorFade => new ColorFadeEffect(),
        EffectKind.RandomLeds => new RandomLedsEffect(),
        EffectKind.RainbowChase => new RainbowChaseEffect(),
        EffectKind.BouncingBalls => new BouncingBallsEffect(),
        EffectKind.GradientSpin => new GradientSpinEffect(),
        EffectKind.Starfield => new StarfieldEffect(),
        EffectKind.Lightning => new LightningEffect(),
        EffectKind.MatrixRain => new MatrixRainEffect(),
        EffectKind.BreathingMulti => new BreathingMultiEffect(),
        EffectKind.MusicVu => new MusicVuEffect(),
        EffectKind.MusicBeat => new MusicBeatEffect(),
        EffectKind.MusicSpectrum => new MusicSpectrumEffect(),
        EffectKind.Temperature => new TemperatureEffect(),
        EffectKind.ScreenAmbient => new ScreenAmbientEffect(),
        _ => new StaticEffect()
    };
}
