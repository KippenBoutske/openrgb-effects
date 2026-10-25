using OpenRgbEffects.Core;
using Xunit;

namespace OpenRgbEffects.Tests;

public class SettingsStoreTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsSettings()
    {
        var path = Path.Combine(Path.GetTempPath(), $"openrgb-effects-{Guid.NewGuid():N}.json");
        try
        {
            var store = new SettingsStore(path);
            var settings = new AppSettings
            {
                Host = "192.168.1.10",
                Port = 7000,
                Effect = EffectKind.Wave,
                ColorHex = "#22D3EE",
                Speed = 0.25,
                Brightness = 0.8,
                SelectedDevices = ["MSI X670E", "ENE DRAM"],
                StartWithWindows = true,
                MinimizeToTray = false,
                ResumeEffectOnStart = false,
                WasRunning = true,
                PaletteId = "ocean",
                Color2Hex = "#112233",
                Reverse = true,
                Favorites = ["Fire", "Comet"]
            };

            store.Save(settings);
            var loaded = store.Load();

            Assert.Equal("192.168.1.10", loaded.Host);
            Assert.Equal(7000, loaded.Port);
            Assert.Equal(EffectKind.Wave, loaded.Effect);
            Assert.Equal("#22D3EE", loaded.ColorHex);
            Assert.Equal(0.25, loaded.Speed);
            Assert.Equal(0.8, loaded.Brightness);
            Assert.Equal(["MSI X670E", "ENE DRAM"], loaded.SelectedDevices);
            Assert.True(loaded.StartWithWindows);
            Assert.False(loaded.MinimizeToTray);
            Assert.False(loaded.ResumeEffectOnStart);
            Assert.True(loaded.WasRunning);
            Assert.Equal("ocean", loaded.PaletteId);
            Assert.Equal("#112233", loaded.Color2Hex);
            Assert.True(loaded.Reverse);
            Assert.Equal(["Fire", "Comet"], loaded.Favorites);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaults()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");
        var settings = new SettingsStore(path).Load();
        Assert.Equal("127.0.0.1", settings.Host);
        Assert.Equal(6742, settings.Port);
        Assert.Equal(EffectKind.Static, settings.Effect);
        Assert.Equal("#FFFFFF", settings.ColorHex);
    }

    [Fact]
    public void Normalize_RepairsInvalidValues()
    {
        var settings = new AppSettings
        {
            Host = " ",
            Port = 99999,
            ColorHex = "nope",
            Speed = 4,
            Brightness = -2
        };
        settings.Normalize();
        Assert.Equal("127.0.0.1", settings.Host);
        Assert.Equal(6742, settings.Port);
        Assert.Equal("#FFFFFF", settings.ColorHex);
        Assert.Equal(1, settings.Speed);
        Assert.Equal(0, settings.Brightness);
    }
}
