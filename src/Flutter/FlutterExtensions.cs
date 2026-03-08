using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Flutter.Widgets;

// Dart parity source (reference): flutter/packages/flutter/lib/src/widgets/binding.dart; flutter/packages/flutter/lib/src/rendering/binding.dart (host integration, adapted)

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
                    Title = "Flutter.NET",
                    Width = 512,
                    Height = 1820,
                    Content = host
                };
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = host;
                break;
        }
    }
}
