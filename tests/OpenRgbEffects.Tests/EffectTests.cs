using OpenRgbEffects.Core;
using OpenRgbEffects.Core.Effects;
using Xunit;

namespace OpenRgbEffects.Tests;

public class EffectTests
{
    [Fact]
    public void Static_FillsAllLedsWithScaledColor()
    {
        var leds = new RgbColor[4];
        var frame = new EffectFrame(1.5, 0.8, 0.5, new RgbColor(200, 100, 0));
        new StaticEffect().Render(frame, leds);

        foreach (var led in leds)
        {
            Assert.Equal(100, led.R);
            Assert.Equal(50, led.G);
            Assert.Equal(0, led.B);
        }
    }

    [Fact]
    public void Breathing_OscillatesBetweenDimAndBright()
    {
        var effect = new BreathingEffect();
        var color = new RgbColor(255, 0, 0);
        var dim = new RgbColor[1];
        var bright = new RgbColor[1];

        effect.Render(new EffectFrame(0, 1, 1, color), dim);
        effect.Render(new EffectFrame(0.5, 1, 1, color), bright);

        Assert.True(dim[0].R < 40);
        Assert.True(bright[0].R > 240);
    }

    [Fact]
    public void Rainbow_UsesSameColorOnEveryLed()
    {
        var leds = new RgbColor[6];
        new RainbowEffect().Render(new EffectFrame(0, 0.5, 1, RgbColor.White), leds);
        Assert.All(leds, led => Assert.Equal(leds[0], led));
        Assert.Equal(255, leds[0].R);
        Assert.Equal(0, leds[0].G);
        Assert.Equal(0, leds[0].B);
    }

    [Fact]
    public void Wave_SpreadsHueAcrossLeds()
    {
        var leds = new RgbColor[3];
        new WaveEffect().Render(new EffectFrame(0, 0, 1, RgbColor.White), leds);

        Assert.Equal(new RgbColor(255, 0, 0), leds[0]);
        Assert.Equal(new RgbColor(0, 255, 0), leds[1]);
        Assert.Equal(new RgbColor(0, 0, 255), leds[2]);
    }

    [Fact]
    public void Factory_CreatesRequestedEffect()
    {
        Assert.Equal(EffectKind.Static, EffectFactory.Create(EffectKind.Static).Kind);
        Assert.Equal(EffectKind.Breathing, EffectFactory.Create(EffectKind.Breathing).Kind);
        Assert.Equal(EffectKind.Rainbow, EffectFactory.Create(EffectKind.Rainbow).Kind);
        Assert.Equal(EffectKind.Wave, EffectFactory.Create(EffectKind.Wave).Kind);
    }
}
