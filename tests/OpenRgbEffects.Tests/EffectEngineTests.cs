using OpenRgbEffects.Core;
using Xunit;

namespace OpenRgbEffects.Tests;

public class EffectEngineTests
{
    [Fact]
    public async Task Stop_RestoresStaticWhite()
    {
        var device = new RgbDeviceInfo(0, "ENE DRAM", "Dram", "ENE", "DRAM RGB", 4, []);
        var fake = new FakeRgbController(device);
        using var engine = new EffectEngine(fake);
        engine.State.Update(EffectKind.Static, new RgbColor(255, 0, 0), 0.5, 1, [0]);
        engine.Start([device]);

        await WaitUntil(() => fake.Updates.Count > 0);
        await engine.StopAsync(restoreWhite: true);

        Assert.Contains(0, fake.Prepared);
        var last = fake.Updates.Last();
        Assert.Equal(0, last.DeviceId);
        Assert.All(last.Colors, color => Assert.Equal(RgbColor.White, color));
    }

    [Fact]
    public async Task Failed_EventFiresWhenSdkUpdateThrows()
    {
        var device = new RgbDeviceInfo(1, "MSI X670E", "Motherboard", "MSI", "Mystic Light", 8, []);
        var fake = new FakeRgbController(device) { ThrowOnUpdate = true };
        using var engine = new EffectEngine(fake);
        engine.State.Update(EffectKind.Rainbow, RgbColor.White, 0.5, 1, [1]);

        string? error = null;
        engine.Failed += (_, message) => error = message;
        engine.Start([device]);
        await WaitUntil(() => error is not null);

        Assert.Equal("SDK-fout", error);
        await engine.StopAsync(restoreWhite: false);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        var start = DateTime.UtcNow;
        while (!condition())
        {
            if (DateTime.UtcNow - start > TimeSpan.FromSeconds(3))
                throw new TimeoutException("Voorwaarde werd niet op tijd waar.");
            await Task.Delay(15);
        }
    }
}
