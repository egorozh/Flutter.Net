using Avalonia.Controls;

namespace Flutter.Widgets;

public interface IBuildContext;

public abstract class Element(IWidget widget) : IBuildContext
{
    public IWidget Widget { get; protected set; } = widget;
    public Control? RenderObject { get; protected set; }

    public abstract void Mount(Control parent);

    public abstract void Update(IWidget newIWidget);

    public virtual void Dispose()
    {
    }
}

public class StatelessElement(IStatelessWidget widget) : Element(widget)
{
    private Element? _child;

    public new IStatelessWidget Widget
    {
        get => (IStatelessWidget)base.Widget;
        set => base.Widget = value;
    }

    public override void Mount(Control parent)
    {
        var built = Widget.Build(this);
        _child = built.CreateElement();
        _child.Mount(parent);
        RenderObject = _child.RenderObject;
    }

    public override void Update(IWidget newIWidget)
    {
        if (newIWidget is IStatelessWidget newStateless)
        {
            Widget = newStateless;
            var newChild = newStateless.Build(this).CreateElement();
            _child?.Update(newChild.Widget);
        }
    }
}

public class StatefulElement : Element
{
    private Element? _child;
    private State _state;

    public new IStatefulWidget Widget
    {
        get => (IStatefulWidget)base.Widget;
        set => base.Widget = value;
    }

    public StatefulElement(IStatefulWidget widget) : base(widget)
    {
        _state = widget.CreateState();
        _state._Attach(widget, this);
        _state.InitState();
    }

    public override void Mount(Control parent)
    {
        var built = _state.Build(this);
        _child = built.CreateElement();
        _child.Mount(parent);
        RenderObject = _child.RenderObject;
    }

    public override void Update(IWidget newIWidget)
    {
        if (newIWidget is IStatefulWidget newStateful)
        {
            var oldIWidget = Widget;
            Widget = newStateful;
            _state._Attach(newStateful, this);
            _state.DidUpdateIWidget(oldIWidget);
            Rebuild();
        }
    }

    public void Rebuild()
    {
        if (_child != null && _child.RenderObject?.Parent is Panel parent)
        {
            parent.Children.Remove(_child.RenderObject);
            var newChild = _state.Build(this).CreateElement();
            newChild.Mount(parent);
            _child = newChild;
            RenderObject = newChild.RenderObject;
        }
    }

    public override void Dispose()
    {
        _state.Dispose();
    }
}