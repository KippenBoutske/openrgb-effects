using System.Windows;
using System.Windows.Resources;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace OpenRgbEffects.App.Services;

public sealed class TrayService : IDisposable
{
    private readonly Window _window;
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Drawing.Icon _icon;
    private bool _exitRequested;

    public TrayService(Window window)
    {
        _window = window;
        _icon = LoadIcon();
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = _icon,
            Text = "OpenRGB Effects",
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => ShowWindow();

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Openen", null, (_, _) => ShowWindow());
        menu.Items.Add("Starten", null, (_, _) => StartRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Stoppen", null, (_, _) => StopRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Afsluiten", null, (_, _) => Exit());
        _notifyIcon.ContextMenuStrip = menu;
    }

    public event EventHandler? StartRequested;
    public event EventHandler? StopRequested;

    public bool ExitRequested => _exitRequested;

    public void ShowWindow()
    {
        _window.Show();
        _window.WindowState = WindowState.Normal;
        _window.Activate();
        _window.Topmost = true;
        _window.Topmost = false;
        _window.Focus();
    }

    public void HideToTray()
    {
        _window.Hide();
        _notifyIcon.Visible = true;
    }

    public void ShowBalloon(string title, string text)
    {
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = text;
        _notifyIcon.ShowBalloonTip(2500);
    }

    public void Exit()
    {
        _exitRequested = true;
        _notifyIcon.Visible = false;
        _window.Close();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon.Dispose();
    }

    private static Drawing.Icon LoadIcon()
    {
        var uri = new Uri("pack://application:,,,/Assets/app.ico", UriKind.Absolute);
        StreamResourceInfo? resource = System.Windows.Application.GetResourceStream(uri);
        if (resource is null)
            return Drawing.SystemIcons.Application;
        return new Drawing.Icon(resource.Stream);
    }
}
