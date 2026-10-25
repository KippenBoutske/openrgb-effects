using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenRgbEffects.App.Services;
using OpenRgbEffects.Core;

namespace OpenRgbEffects.App.ViewModels;

public sealed class ZoneItemViewModel
{
    public ZoneItemViewModel(RgbZoneInfo zone)
    {
        Name = zone.Name;
        TypeLabel = DeviceLabels.ZoneType(zone.Type);
        LedCount = zone.LedCount;
        Summary = $"{zone.Name} · {TypeLabel} · {zone.LedCount} LED{(zone.LedCount == 1 ? "" : "s")}";
    }

    public string Name { get; }
    public string TypeLabel { get; }
    public int LedCount { get; }
    public string Summary { get; }
}

public partial class DeviceItemViewModel : ObservableObject
{
    public DeviceItemViewModel(RgbDeviceInfo device, bool selected)
    {
        Device = device;
        TypeLabel = DeviceLabels.DeviceType(device.Type);
        Details = string.IsNullOrWhiteSpace(device.Vendor)
            ? $"{TypeLabel} · {device.LedCount} LED{(device.LedCount == 1 ? "" : "s")}"
            : $"{TypeLabel} · {device.Vendor} · {device.LedCount} LED{(device.LedCount == 1 ? "" : "s")}";
        Zones = new ObservableCollection<ZoneItemViewModel>(device.Zones.Select(zone => new ZoneItemViewModel(zone)));
        _isSelected = selected;
    }

    public RgbDeviceInfo Device { get; }
    public string Name => Device.Name;
    public string TypeLabel { get; }
    public string Details { get; }
    public ObservableCollection<ZoneItemViewModel> Zones { get; }

    [ObservableProperty]
    private bool _isSelected;

    public event EventHandler? SelectionChanged;

    partial void OnIsSelectedChanged(bool value) => SelectionChanged?.Invoke(this, EventArgs.Empty);
}
