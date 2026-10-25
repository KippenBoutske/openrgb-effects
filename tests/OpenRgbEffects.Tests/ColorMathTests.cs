using OpenRgbEffects.Core;
using Xunit;

namespace OpenRgbEffects.Tests;

public class ColorMathTests
{
    [Fact]
    public void FromHsv_PrimaryHues_MatchExpectedColors()
    {
        AssertEqual(new RgbColor(255, 0, 0), ColorMath.FromHsv(0, 1, 1));
        AssertEqual(new RgbColor(0, 255, 0), ColorMath.FromHsv(120, 1, 1));
        AssertEqual(new RgbColor(0, 0, 255), ColorMath.FromHsv(240, 1, 1));
        AssertEqual(new RgbColor(255, 255, 255), ColorMath.FromHsv(0, 0, 1));
    }

    [Fact]
    public void Scale_AppliesBrightness()
    {
        var scaled = ColorMath.Scale(RgbColor.White, 0.5);
        Assert.Equal(128, scaled.R);
        Assert.Equal(128, scaled.G);
        Assert.Equal(128, scaled.B);
    }

    [Fact]
    public void Scale_ClampsOutOfRangeBrightness()
    {
        AssertEqual(RgbColor.Black, ColorMath.Scale(RgbColor.White, -1));
        AssertEqual(RgbColor.White, ColorMath.Scale(RgbColor.White, 4));
    }

    [Fact]
    public void RgbColor_RoundTripsHex()
    {
        var original = new RgbColor(18, 52, 86);
        Assert.True(RgbColor.TryParseHex(original.ToHex(), out var parsed));
        AssertEqual(original, parsed);
    }

    private static void AssertEqual(RgbColor expected, RgbColor actual)
    {
        Assert.Equal(expected.R, actual.R);
        Assert.Equal(expected.G, actual.G);
        Assert.Equal(expected.B, actual.B);
    }
}
