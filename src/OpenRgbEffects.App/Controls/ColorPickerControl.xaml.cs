using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using OpenRgbEffects.Core;

namespace OpenRgbEffects.App.Controls;

public partial class ColorPickerControl : UserControl
{
    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        nameof(SelectedColor),
        typeof(Color),
        typeof(ColorPickerControl),
        new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

    private bool _updating;

    public ColorPickerControl()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            BuildSwatches();
            SyncFromColor(SelectedColor);
        };
    }

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorPickerControl picker)
            picker.SyncFromColor((Color)e.NewValue);
    }

    private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updating || !IsLoaded)
            return;

        var color = ColorMath.FromHsv(HueSlider.Value, SatSlider.Value, ValSlider.Value);
        SelectedColor = Color.FromRgb(color.R, color.G, color.B);
    }

    private void HexBox_OnLostFocus(object sender, RoutedEventArgs e) => ApplyHex();

    private void HexBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ApplyHex();
    }

    private void ApplyHex()
    {
        if (RgbColor.TryParseHex(HexBox.Text, out var color))
            SelectedColor = Color.FromRgb(color.R, color.G, color.B);
        else
            HexBox.Text = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
    }

    private void SyncFromColor(Color color)
    {
        _updating = true;
        var rgb = new RgbColor(color.R, color.G, color.B);
        var hsv = ColorMath.ToHsv(rgb);
        if (HueSlider is not null)
            HueSlider.Value = hsv.Hue;
        if (SatSlider is not null)
            SatSlider.Value = hsv.Saturation;
        if (ValSlider is not null)
            ValSlider.Value = hsv.Value;
        if (HexBox is not null)
            HexBox.Text = rgb.ToHex();
        _updating = false;
    }

    private void BuildSwatches()
    {
        if (Swatches.Children.Count > 0)
            return;

        Color[] colors =
        [
            Colors.White,
            Color.FromRgb(255, 214, 170),
            Color.FromRgb(255, 59, 92),
            Color.FromRgb(255, 138, 46),
            Color.FromRgb(255, 214, 10),
            Color.FromRgb(46, 230, 166),
            Color.FromRgb(34, 211, 238),
            Color.FromRgb(88, 124, 255),
            Color.FromRgb(167, 99, 255),
            Color.FromRgb(255, 92, 168)
        ];

        foreach (var color in colors)
        {
            var swatch = new Ellipse
            {
                Width = 22,
                Height = 22,
                Margin = new Thickness(0, 0, 8, 6),
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Color.FromRgb(43, 49, 64)),
                StrokeThickness = 1,
                Cursor = Cursors.Hand
            };
            swatch.MouseLeftButtonDown += (_, _) => SelectedColor = color;
            Swatches.Children.Add(swatch);
        }
    }
}
