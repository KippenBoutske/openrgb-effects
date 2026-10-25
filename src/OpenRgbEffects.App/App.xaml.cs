using System.Windows;

namespace OpenRgbEffects.App;

public partial class App : Application
{
    public static bool StartMinimized { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        StartMinimized = e.Args.Any(argument =>
            string.Equals(argument, "--minimized", StringComparison.OrdinalIgnoreCase));
        base.OnStartup(e);
    }
}
