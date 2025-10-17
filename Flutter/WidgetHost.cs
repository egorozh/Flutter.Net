using System.Diagnostics;
using Avalonia.Controls;
using Flutter.Widgets;

namespace Flutter;

/// <summary>
/// WidgetHost — root control to embed in Avalonia app/window
/// </summary>
public sealed class WidgetHost : ContentControl
{
    private BuildOwner? _owner;
    private Element? _rootElement;
    private Widget? _rootWidget;

    public Widget? RootWidget
    {
        get => _rootWidget;
        set
        {
            if (_rootWidget == value) return;
            _rootWidget = value;
            InitializeOrUpdate();
        }
    }

    private void InitializeOrUpdate()
    {
        if (_rootWidget is null)
        {
            Content = null;
            return;
        }

        if (_owner is null)
        {
            _owner = new BuildOwner();
            _rootElement = _rootWidget.CreateElement();
            _rootElement.Attach(_owner);
            _rootElement.Mount(parent: null);
            // find topmost control from the element tree
            Content = _rootElement.Control;
        }
        else
        {
            Debug.Assert(_rootElement != null);
            _rootElement.Update(_rootWidget);
            _rootElement.MarkNeedsBuild();
        }
    }
}