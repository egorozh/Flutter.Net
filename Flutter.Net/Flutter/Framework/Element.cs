using System;
using Avalonia.Controls;

namespace Flutter.Net.Flutter.Framework;

public interface IBuildContext
{
    Widget Widget { get; }

    bool Mounted { get; }
}

public abstract class Element(Widget widget) : IBuildContext
{
    private Element? _parent;

    protected Widget? _widget = widget;

    public Widget Widget => _widget!;

    public bool Mounted => _widget != null;

    internal virtual Control RenderObject { get; }

    // internal List<Element> Children { get; set; } = new List<Element>();

    internal virtual void Mount(Element? parent)
    {
        _parent = parent;
    }

    internal virtual void Unmount()
    {
        _widget = null;
    }

    protected internal virtual void Update(Widget newWidget)
    {
        _widget = newWidget;

        // if (!CanUpdate(newWidget))
        // {
        //     var newElement = newWidget.CreateElement();
        //     newElement.Mount(Parent);
        //     int idx = Parent.Children.IndexOf(this);
        //     Parent.Children[idx] = newElement;
        //     ((Panel)Parent.RenderObject).Children.MutateReplace(idx, newElement.RenderObject);
        //     Dispose();
        // }
        // else
        // {
        //     var oldWidget = Widget;
        //     Widget = newWidget;
        //     OnWidgetUpdated(oldWidget);
        //     PerformRebuild();
        // }
    }

    // IBuildContext
    // void IBuildContext.Rebuild() => PerformRebuild();
    // internal void Rebuild() => PerformRebuild();
    //
    // internal abstract void PerformRebuild();
    // internal abstract bool CanUpdate(Widget newWidget);
    //
    // protected virtual void OnWidgetUpdated(Widget oldWidget)
    // {
    // }
    //
    // protected virtual void Dispose()
    // {
    // }

    private bool _dirty = true;

    public void MarkNeedsBuild()
    {
        if (_dirty)
            return;

        _dirty = true;
    }

    protected void Rebuild(bool force = false)
    {
        if (!_dirty && !force)
            return;

        PerformRebuild();
    }

    protected virtual void PerformRebuild()
    {
        _dirty = false;
    }

    protected Element? UpdateChild(Element? child, Widget? newWidget)
    {
        if (newWidget == null)
        {
            // if (child != null) DeactivateChild(child);
            //
            // return null;
        }

        return null;
    }


    // internal override bool CanUpdate(Widget newWidget) =>
    //     newWidget.GetType() == Widget.GetType() && Equals(newWidget.Key, Widget.Key);
    //
    // internal override void PerformRebuild()
    // {
    //     RenderObject = _widget.Build(this);
    //     Children.Clear();
    // }
}

public abstract class ComponentElement(Widget widget) : Element(widget)
{
    private Element? _child;

    internal override void Mount(Element? parent)
    {
        base.Mount(parent);
        FirstBuild();
    }

    protected virtual void FirstBuild()
    {
        Rebuild();
    }

    protected override void PerformRebuild()
    {
        var built = Build();
        _child = UpdateChild(_child, built);
    }


    protected abstract Widget Build();
}

public class StatelessElement(StatelessWidget widget) : ComponentElement(widget)
{
    protected override Widget Build() => ((StatelessWidget)Widget).Build(this);

    protected internal override void Update(Widget newWidget)
    {
        base.Update(newWidget);

        Rebuild(force: true);
    }
}

public class StatefulElement : ComponentElement
{
    private State _state;

    public State State => _state;

    public StatefulElement(StatefulWidget widget) : base(widget)
    {
        _state = widget.CreateState();
        _state._element = this;
        _state._widget = widget;
    }

    protected override Widget Build() => _state.Build(this);

    protected override void FirstBuild()
    {
        _state.InitState();

        _state.DidChangeDependencies();

        base.FirstBuild();
    }

    protected internal override void Update(Widget newWidget)
    {
        base.Update(newWidget);

        var oldWidget = _state._widget!;

        _state._widget = _widget as StatefulWidget;

        _state.DidUpdateWidget(oldWidget);

        Rebuild(force: true);
    }

    internal override void Unmount()
    {
        base.Unmount();
        _state.Dispose();
        _state._element = null;
        _state = null!;
    }
    // internal override bool CanUpdate(Widget newWidget) =>
    //     newWidget.GetType() == Widget.GetType() && Equals(newWidget.Key, Widget.Key);
    //
    // protected override void OnWidgetUpdated(Widget oldWidget)
    // {
    //     _state.Context = this;
    //     _state.DidUpdateWidget((StatefulWidget)oldWidget);
    // }
    //
    // internal override void PerformRebuild()
    // {
    //     RenderObject = _state.Build(this);
    //     Children.Clear();
    // }

    //
    // protected override void Dispose() => _state.Dispose();
}