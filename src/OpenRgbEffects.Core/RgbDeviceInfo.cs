namespace OpenRgbEffects.Core;

public sealed record RgbZoneInfo(
    int Index,
    string Name,
    string Type,
    int LedCount);

public sealed record RgbDeviceInfo(
    int Index,
    string Name,
    string Type,
    string? Vendor,
    string Description,
    int LedCount,
    IReadOnlyList<RgbZoneInfo> Zones)
{
    public string DisplayName =>
        string.IsNullOrWhiteSpace(Vendor) ? Name : $"{Name} ({Vendor})";
}
