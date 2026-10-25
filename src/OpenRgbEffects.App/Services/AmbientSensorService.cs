using System.Runtime.InteropServices;
using OpenRgbEffects.Core;
using Drawing = System.Drawing;

namespace OpenRgbEffects.App.Services;

public sealed class AmbientSensorService
{
    private const int SrcCopy = 0x00CC0020;
    private readonly object _gate = new();
    private RgbColor _color = RgbColor.Black;
    private bool _available;

    public void Refresh()
    {
        try
        {
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            if (screen is null)
                return;

            var bounds = screen.Bounds;
            using var small = new Drawing.Bitmap(32, 18, Drawing.Imaging.PixelFormat.Format32bppRgb);
            using var graphics = Drawing.Graphics.FromImage(small);
            var dest = graphics.GetHdc();
            var src = GetDC(IntPtr.Zero);
            try
            {
                StretchBlt(dest, 0, 0, 32, 18, src, bounds.Left, bounds.Top, bounds.Width, bounds.Height, SrcCopy);
            }
            finally
            {
                graphics.ReleaseHdc(dest);
                ReleaseDC(IntPtr.Zero, src);
            }

            long r = 0, g = 0, b = 0;
            for (var y = 0; y < 18; y++)
            {
                for (var x = 0; x < 32; x++)
                {
                    var pixel = small.GetPixel(x, y);
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                }
            }

            lock (_gate)
            {
                _color = new RgbColor((byte)(r / (32 * 18)), (byte)(g / (32 * 18)), (byte)(b / (32 * 18)));
                _available = true;
            }
        }
        catch (Exception)
        {
            lock (_gate)
                _available = false;
        }
    }

    public void CaptureInto(ref SensorSnapshot snapshot)
    {
        lock (_gate)
        {
            snapshot = snapshot with { HasAmbient = _available, Ambient = _color };
        }
    }

    [DllImport("gdi32.dll")]
    private static extern bool StretchBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
        IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, int rop);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
}
