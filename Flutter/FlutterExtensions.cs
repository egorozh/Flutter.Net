using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Flutter.Widgets;

namespace Flutter;

public static class FlutterExtensions
{
    public static void Run<T>(T application, IApplicationLifetime? applicationLifetime) where T : Widget
    {
        var host = new WidgetHost
        {
            RootWidget = application
        };


        switch (applicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new Window
                {
                    Content = host
                };
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new UserControl
                {
                    Content = host
                };
                break;
        }
    }
}