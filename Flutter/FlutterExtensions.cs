using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Flutter.Widgets;

namespace Flutter;

public static class FlutterExtensions
{
    public static void Run<T>(T application, IApplicationLifetime? applicationLifetime) where T : IWidget
    {
        var rootPanel = new StackPanel();

        var appElement = application.CreateElement();

        appElement.Mount(rootPanel);

        switch (applicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new Window
                {
                    Content = rootPanel
                };
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new UserControl
                {
                    Content = rootPanel
                };
                break;
        }
    }
}