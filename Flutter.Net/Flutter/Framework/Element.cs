using System;
using Avalonia.Controls;

namespace Flutter.Net.Flutter.Framework;

public interface IBuildContext
{
    Widget Widget { get; }

    bool Mounted { get; }
}

internal abstract class Element(Widget widget) : IBuildContext
{
    private Element? _parent;

    private Widget? _widget = widget;

    public Widget Widget => _widget!;

    public bool Mounted => _widget != null;

    internal Control RenderObject { get; }

    // internal List<Element> Children { get; set; } = new List<Element>();

    internal virtual void Mount(Element? parent)
    {
        _parent = parent;
    }

    internal void Unmount()
    {
        _widget = null;
    }

    internal void Update(Widget newWidget)
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

    protected virtual void Update(StatelessWidget newWidget)
    {
        throw new NotImplementedException();
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

internal abstract class ComponentElement(Widget widget) : Element(widget)
{
    Element? _child;

    internal override void Mount(Element? parent)
    {
        base.Mount(parent);
        _firstBuild();
    }

    void _firstBuild()
    {
        Rebuild();
    }

    protected override void PerformRebuild()
    {
    }

    protected abstract Widget Build();
}

internal class StatelessElement(StatelessWidget widget) : ComponentElement(widget)
{
    protected override Widget Build() => ((StatelessWidget)Widget).Build(this);

    protected override void Update(StatelessWidget newWidget)
    {
        base.Update(newWidget);

        Rebuild(force: true);
    }
}

internal class StatefulElement : ComponentElement
{
    private readonly StatefulWidget _widget;
    private readonly State _state;

    public StatefulElement(StatefulWidget widget)
    {
        Widget = widget;
        _widget = widget;
        _state = widget.CreateState();
        _state.Widget = widget;
        _state.Context = this;
        _state.InitState();
    }

    internal override bool CanUpdate(Widget newWidget) =>
        newWidget.GetType() == Widget.GetType() && Equals(newWidget.Key, Widget.Key);

    protected override void OnWidgetUpdated(Widget oldWidget)
    {
        _state.Context = this;
        _state.DidUpdateWidget((StatefulWidget)oldWidget);
    }

    internal override void PerformRebuild()
    {
        RenderObject = _state.Build(this);
        Children.Clear();
    }

    protected override void Dispose() => _state.Dispose();
}