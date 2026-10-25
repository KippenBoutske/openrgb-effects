using CommunityToolkit.Mvvm.ComponentModel;
using OpenRgbEffects.Core;

namespace OpenRgbEffects.App.ViewModels;

public partial class EffectItemViewModel : ObservableObject
{
    public EffectItemViewModel(EffectDescriptor descriptor, bool favorite)
    {
        Descriptor = descriptor;
        _isFavorite = favorite;
    }

    public EffectDescriptor Descriptor { get; }
    public EffectKind Kind => Descriptor.Kind;
    public string Name => Descriptor.Name;
    public string Description => Descriptor.Description;
    public string CategoryName => EffectCatalog.CategoryName(Descriptor.Category);
    public EffectCategory Category => Descriptor.Category;

    [ObservableProperty]
    private bool _isFavorite;
}

public sealed class CategoryFilterViewModel
{
    public CategoryFilterViewModel(string id, string name, EffectCategory? category = null)
    {
        Id = id;
        Name = name;
        Category = category;
    }

    public string Id { get; }
    public string Name { get; }
    public EffectCategory? Category { get; }
}

public sealed class PaletteItemViewModel
{
    public PaletteItemViewModel(PalettePreset preset)
    {
        Preset = preset;
        Swatches = preset.Colors.Select(color => Color.FromRgb(color.R, color.G, color.B)).ToArray();
    }

    public PalettePreset Preset { get; }
    public string Id => Preset.Id;
    public string Name => Preset.Name;
    public IReadOnlyList<Color> Swatches { get; }
}
