using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Flutter.Net.Flutter.App;
using Flutter.Net.Flutter.Framework;

namespace Flutter.Net;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // desktop.MainWindow = new MainWindow
            // {
            //     DataContext = new MainViewModel()
            // };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var rootPanel = new StackPanel();

            var appWidget = new MyApp();
            
            var appElement = appWidget.CreateElement();
            
            appElement.Mount(rootPanel);

            singleViewPlatform.MainView = new UserControl
            {
                Content = rootPanel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}