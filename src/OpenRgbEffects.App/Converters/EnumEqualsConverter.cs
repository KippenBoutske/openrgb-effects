using System.Globalization;
using System.Windows.Data;

namespace OpenRgbEffects.App.Converters;

public sealed class EnumEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
            return false;
        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.Ordinal);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not true || parameter is null)
            return Binding.DoNothing;

        var name = parameter.ToString();
        if (string.IsNullOrWhiteSpace(name))
            return Binding.DoNothing;

        var enumType = targetType;
        if (enumType == typeof(object) || enumType is null || !enumType.IsEnum)
            enumType = typeof(OpenRgbEffects.Core.EffectKind);

        return Enum.Parse(enumType, name);
    }
}
