namespace OpenRgbEffects.App.Services;

public static class DeviceLabels
{
    public static string DeviceType(string type) => type switch
    {
        "Motherboard" => "Moederbord",
        "Dram" => "Geheugen (DRAM)",
        "Gpu" => "Videokaart",
        "Cooler" => "Koeler",
        "Ledstrip" => "LED-strip",
        "Keyboard" => "Toetsenbord",
        "Mouse" => "Muis",
        "Mousemat" => "Muismat",
        "Headset" => "Headset",
        "HeadsetStand" => "Headsetstandaard",
        "Gamepad" => "Gamepad",
        "Light" => "Verlichting",
        "Speaker" => "Luidspreker",
        "Virtual" => "Virtueel",
        _ => "Onbekend"
    };

    public static string ZoneType(string type) => type switch
    {
        "Single" => "Enkel",
        "Linear" => "Lijn",
        "Matrix" => "Matrix",
        _ => type
    };

    public static string Effect(OpenRgbEffects.Core.EffectKind kind) => kind switch
    {
        OpenRgbEffects.Core.EffectKind.Static => "Statische kleur",
        OpenRgbEffects.Core.EffectKind.Breathing => "Ademhaling",
        OpenRgbEffects.Core.EffectKind.Rainbow => "Regenboog",
        OpenRgbEffects.Core.EffectKind.Wave => "Golf",
        _ => kind.ToString()
    };
}
