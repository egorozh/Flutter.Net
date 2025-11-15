using Avalonia;
using Avalonia.Markup.Xaml;

namespace Flutter.Net;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        FlutterExtensions.Run(new CounterApp(), ApplicationLifetime);

        base.OnFrameworkInitializationCompleted();
    }
}