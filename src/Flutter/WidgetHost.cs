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
    private RootElement? _rootElement;
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
            if (_rootElement != null)
            {
                _rootElement.Unmount();
                _rootElement = null;
            }

            Content = null;
            return;
        }

        _owner ??= new BuildOwner();

        if (_rootElement is null)
        {
            _rootElement = new RootElement(this, _rootWidget);
            _rootElement.Attach(_owner);
            _rootElement.Mount(parent: null);
        }
        else
        {
            _rootElement.Update(_rootWidget);
        }
    }

    private sealed class RootElement : Element
    {
        private readonly WidgetHost _host;
        private Element? _child;

        public RootElement(WidgetHost host, Widget widget) : base(widget)
        {
            _host = host;
        }

        protected override void OnMount()
        {
            base.OnMount();
            Rebuild();
        }

        internal override void Rebuild()
        {
            Dirty = false;
            _child = TreeHelpers.ReconcileSingleChild(this, _child, Widget);
        }

        internal override void Update(Widget newWidget)
        {
            base.Update(newWidget);
            MarkNeedsBuild();
        }

        internal override void InsertChildRenderObject(int index, Element child)
        {
            if (child.Control is Control control)
            {
                _host.Content = control;
            }
        }

        internal override void RemoveChildRenderObject(Element child)
        {
            if (ReferenceEquals(_host.Content, child.Control))
            {
                _host.Content = null;
            }
        }

        internal override void Unmount()
        {
            if (_child != null)
            {
                TreeHelpers.DeactivateChild(_child);
                _child = null;
            }

            base.Unmount();
        }
    }
}