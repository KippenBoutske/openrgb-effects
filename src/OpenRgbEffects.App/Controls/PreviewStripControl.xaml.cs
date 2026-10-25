using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenRgbEffects.Core;

namespace OpenRgbEffects.App.Controls;

public partial class PreviewStripControl : UserControl
{
    private WriteableBitmap? _bitmap;
    private byte[] _pixels = [];

    public PreviewStripControl()
    {
        InitializeComponent();
    }

    public void Update(ReadOnlySpan<RgbColor> leds)
    {
        if (leds.IsEmpty)
            return;

        if (_bitmap is null || _bitmap.PixelWidth != leds.Length)
        {
            _bitmap = new WriteableBitmap(leds.Length, 1, 96, 96, PixelFormats.Bgra32, null);
            Image.Source = _bitmap;
        }

        var needed = leds.Length * 4;
        if (_pixels.Length != needed)
            _pixels = new byte[needed];
        for (var i = 0; i < leds.Length; i++)
        {
            var color = leds[i];
            var o = i * 4;
            _pixels[o] = color.B;
            _pixels[o + 1] = color.G;
            _pixels[o + 2] = color.R;
            _pixels[o + 3] = 255;
        }

        _bitmap.WritePixels(new Int32Rect(0, 0, leds.Length, 1), _pixels, leds.Length * 4, 0);
    }
}
