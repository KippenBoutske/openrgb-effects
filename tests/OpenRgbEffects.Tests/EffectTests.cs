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
    public void Catalog_HasAtLeastTwentyFourEffects()
    {
        Assert.True(EffectCatalog.All.Count >= 24);
        Assert.Equal(EffectCatalog.All.Count, EffectCatalog.All.Select(item => item.Kind).Distinct().Count());
    }

    [Fact]
    public void Factory_CreatesEveryCatalogEffect()
    {
        foreach (var descriptor in EffectCatalog.All)
        {
            var effect = EffectFactory.Create(descriptor.Kind);
            Assert.Equal(descriptor.Kind, effect.Kind);
        }
    }

    [Fact]
    public void AllEffects_RenderWithoutThrowing()
    {
        var palette = PalettePresets.Sunset.Colors;
        var frame = new EffectFrame(0.8, 0.6, 0.8, new RgbColor(255, 40, 40))
        {
            Color2 = new RgbColor(20, 40, 255),
            Palette = palette,
            Direction = 1,
            Sensors = new SensorSnapshot(0.4, 0.5, 0.3, 0.2, true, 62, 70, true, true, true, new RgbColor(10, 20, 30))
        };

        foreach (var descriptor in EffectCatalog.All)
        {
            var effect = EffectFactory.Create(descriptor.Kind);
            foreach (var count in new[] { 1, 8, 32 })
            {
                var leds = new RgbColor[count];
                effect.Render(frame, leds);
            }
        }
    }

    [Fact]
    public void ColorWipe_FillsFurtherOverTime()
    {
        var effect = new ColorWipeEffect();
        var early = new RgbColor[10];
        var late = new RgbColor[10];
        var palette = new[] { new RgbColor(255, 0, 0), new RgbColor(0, 0, 255) };
        effect.Render(new EffectFrame(0.02, 0.2, 1, palette[0]) { Palette = palette }, early);
        effect.Render(new EffectFrame(1.6, 0.2, 1, palette[0]) { Palette = palette }, late);
        Assert.True(late.Count(led => led.R > 10 || led.B > 10) >= early.Count(led => led.R > 10 || led.B > 10));
    }

    [Fact]
    public void SamplePalette_WrapsAndInterpolates()
    {
        var palette = new[] { new RgbColor(255, 0, 0), new RgbColor(0, 0, 255) };
        var mid = ColorMath.SamplePalette(palette, 0.25);
        Assert.True(mid.R > 80);
        Assert.True(mid.B > 80);
        Assert.Equal(ColorMath.SamplePalette(palette, 0), ColorMath.SamplePalette(palette, 1));
    }
}
