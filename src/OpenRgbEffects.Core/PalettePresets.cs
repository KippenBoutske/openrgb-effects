namespace OpenRgbEffects.Core;

public sealed record PalettePreset(string Id, string Name, IReadOnlyList<RgbColor> Colors)
{
    public static PalettePreset FromHex(string id, string name, params string[] hex)
    {
        var colors = hex.Select(value =>
            RgbColor.TryParseHex(value, out var color) ? color : RgbColor.White).ToArray();
        return new PalettePreset(id, name, colors);
    }
}

public static class PalettePresets
{
    public static PalettePreset Rainbow { get; } = PalettePreset.FromHex(
        "rainbow", "Regenboog", "#FF0000", "#FF7A00", "#FFD400", "#22C55E", "#22D3EE", "#3B82F6", "#A855F7");

    public static PalettePreset Sunset { get; } = PalettePreset.FromHex(
        "sunset", "Zonsondergang", "#FF4D6D", "#FF7A18", "#FFD166", "#FF9A76");

    public static PalettePreset Ocean { get; } = PalettePreset.FromHex(
        "ocean", "Oceaan", "#022C43", "#115173", "#1B9CFC", "#55E6C1");

    public static PalettePreset Forest { get; } = PalettePreset.FromHex(
        "forest", "Bos", "#14532D", "#22C55E", "#A3E635", "#064E3B");

    public static PalettePreset Fire { get; } = PalettePreset.FromHex(
        "fire", "Vuur", "#1C1917", "#7C2D12", "#EA580C", "#FACC15", "#FFF7ED");

    public static PalettePreset Ice { get; } = PalettePreset.FromHex(
        "ice", "IJs", "#0F172A", "#38BDF8", "#E0F2FE", "#A5B4FC");

    public static PalettePreset Neon { get; } = PalettePreset.FromHex(
        "neon", "Neon", "#22D3EE", "#A3E635", "#F472B6", "#C084FC");

    public static PalettePreset Pastel { get; } = PalettePreset.FromHex(
        "pastel", "Pastel", "#FBCFE8", "#FDE68A", "#BBF7D0", "#BFDBFE");

    public static PalettePreset Cyberpunk { get; } = PalettePreset.FromHex(
        "cyberpunk", "Cyberpunk", "#F72585", "#7209B7", "#3A0CA3", "#4CC9F0");

    public static PalettePreset Candy { get; } = PalettePreset.FromHex(
        "candy", "Snoep", "#FB7185", "#F472B6", "#C084FC", "#67E8F9");

    public static PalettePreset Mono { get; } = PalettePreset.FromHex(
        "mono", "Mono", "#0B0F14", "#6B7280", "#F8FAFC");

    public static IReadOnlyList<PalettePreset> All { get; } =
    [
        Rainbow, Sunset, Ocean, Forest, Fire, Ice, Neon, Pastel, Cyberpunk, Candy, Mono
    ];

    public static PalettePreset Get(string? id) =>
        All.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? Rainbow;
}
