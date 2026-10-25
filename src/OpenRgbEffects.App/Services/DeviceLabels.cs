using OpenRgbEffects.Core;

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

    public static string Effect(EffectKind kind) => EffectCatalog.Get(kind).Name;
}
