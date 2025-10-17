using Avalonia.Controls;
using Flutter.Foundation;

namespace Flutter.Widgets;

public readonly struct BuildContext
{
    internal Element Owner { get; }

    internal BuildContext(Element owner)
    {
        Owner = owner;
    }

    public T? DependOnInherited<T>() where T : InheritedWidget => Owner.DependOnInherited<T>();
}

// Base Element
public abstract class Element
{
    public Widget Widget { get; private set; }
    public Element? Parent { get; private set; }
    public int Depth { get; private set; }
    internal bool Dirty { get; set; }
    internal BuildOwner? Owner { get; private set; }

    protected Element(Widget widget)
    {
        Widget = widget;
    }

    internal void Attach(BuildOwner owner)
    {
        Owner = owner;
        Owner.RegisterElement(this);
    }

    internal void Mount(Element? parent)
    {
        Parent = parent;
        Depth = (parent?.Depth ?? 0) + 1;
        OnMount();
    }

    protected virtual void OnMount()
    {
    }

    protected virtual void OnUnmount()
    {
    }

    internal virtual void Unmount()
    {
        OnUnmount();
        Owner?.UnregisterElement(this);
        Parent = null;
    }

    internal abstract void Rebuild();

    internal void MarkNeedsBuild()
    {
        if (Dirty) return;
        Dirty = true;
        Owner?.ScheduleBuild(this);
    }

    internal virtual void Update(Widget newWidget)
    {
        Widget = newWidget; // default: just replace ref
    }

    // Inherited support
    internal virtual T? DependOnInherited<T>() where T : InheritedWidget
    {
        return Parent?.DependOnInherited<T>();
    }

    // Render-control bridging
    public virtual Control? Control => null; // not every element owns a control

    internal virtual void InsertChildRenderObject(int index, Element child)
    {
        // bubble up to a parent that knows how to host visuals
        Parent?.InsertChildRenderObject(index, child);
    }

    internal virtual void RemoveChildRenderObject(Element child)
    {
        Parent?.RemoveChildRenderObject(child);
    }
}

public sealed class StatelessElement : Element
{
    private Element? _child;

    public StatelessElement(StatelessWidget widget) : base(widget)
    {
    }

    protected override void OnMount()
    {
        base.OnMount();
        Rebuild();
    }

    internal override void Rebuild()
    {
        Dirty = false;
        var childWidget = ((StatelessWidget)Widget).Build(new BuildContext(this));
        _child = TreeHelpers.ReconcileSingleChild(this, _child, childWidget);
    }

    internal override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        MarkNeedsBuild();
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

public sealed class StatefulElement : Element
{
    private Element? _child;
    public State State { get; }

    public StatefulElement(StatefulWidget widget) : base(widget)
    {
        State = widget.CreateState();
        State.Element = this;
    }

    protected override void OnMount()
    {
        base.OnMount();
        State.InitState();
        Rebuild();
    }

    internal override void Rebuild()
    {
        Dirty = false;
        var widget = State.Build(new BuildContext(this));
        _child = TreeHelpers.ReconcileSingleChild(this, _child, widget);
    }

    internal override void Update(Widget newWidget)
    {
        var old = (StatefulWidget)Widget;
        base.Update(newWidget);
        State.DidUpdateWidget((StatefulWidget)old);
        MarkNeedsBuild();
    }

    internal override void Unmount()
    {
        if (_child != null)
        {
            TreeHelpers.DeactivateChild(_child);
            _child = null;
        }

        State.Dispose();
        base.Unmount();
    }
}

public sealed class InheritedElement : Element
{
    private Element? _child;

    public InheritedElement(InheritedWidget widget) : base(widget)
    {
    }

    protected override void OnMount()
    {
        base.OnMount();
        Rebuild();
    }

    internal override void Rebuild()
    {
        Dirty = false;
        var child = ((InheritedWidget)Widget).Build(new BuildContext(this));
        _child = TreeHelpers.ReconcileSingleChild(this, _child, child);
    }

    internal override void Update(Widget newWidget)
    {
        var old = (InheritedWidget)Widget;
        base.Update(newWidget);
        if (((InheritedWidget)newWidget).UpdateShouldNotify(old))
        {
            // notify dependents by marking subtree dirty
            Owner?.MarkSubtreeNeedsBuild(this);
        }

        MarkNeedsBuild();
    }

    // internal override T? DependOnInherited<T>() where T : InheritedWidget
    // {
    //     if (Widget is T t) return t;
    //     
    //     return base.DependOnInherited<T>();
    // }
    
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

// Tree helpers (inflate/reconcile)
internal static class TreeHelpers
{
    public static Element InflateWidget(Element parent, Widget widget)
    {
        var e = widget.CreateElement();
        e.Attach(parent.Owner!);
        e.Mount(parent);
        return e;
    }

    public static Element? ReconcileSingleChild(Element parent, Element? oldChild, Widget? newWidget)
    {
        if (newWidget is null)
        {
            if (oldChild != null)
            {
                parent.RemoveChildRenderObject(oldChild);
                DeactivateChild(oldChild);
            }

            return null;
        }

        if (oldChild != null && oldChild.Widget.GetType() == newWidget.GetType() &&
            Equals(oldChild.Widget.Key, newWidget.Key))
        {
            oldChild.Update(newWidget);
            return oldChild;
        }

        // replace
        if (oldChild != null)
        {
            parent.RemoveChildRenderObject(oldChild);
            DeactivateChild(oldChild);
        }
        var newEl = InflateWidget(parent, newWidget);
        parent.InsertChildRenderObject(0, newEl);
        return newEl;
    }

    public static List<Element> ReconcileChildren(Element parent, List<Element> oldChildren,
        IReadOnlyList<Widget> newWidgets)
    {
        // Very simple keyed reconciler (O(n))
        var newChildren = new List<Element>(newWidgets.Count);
        var oldByKey = new Dictionary<Key, Element>();
        foreach (var c in oldChildren)
        {
            if (c.Widget.Key is Key k) oldByKey[k] = c;
            else
            {
                DeactivateChild(c);
            }
        }

        for (int i = 0; i < newWidgets.Count; i++)
        {
            var w = newWidgets[i];
            Element? reused = null;
            if (w.Key is Key k && oldByKey.TryGetValue(k, out reused) && reused.Widget.GetType() == w.GetType())
            {
                reused.Update(w);
                newChildren.Add(reused);
                oldByKey.Remove(k);
            }
            else
            {
                var ne = InflateWidget(parent, w);
                parent.InsertChildRenderObject(i, ne);
                newChildren.Add(ne);
            }
        }

        // remove leftovers
        foreach (var (_, leftover) in oldByKey)
        {
            parent.RemoveChildRenderObject(leftover);
            DeactivateChild(leftover);
        }

        return newChildren;
    }

    public static void DeactivateChild(Element child)
    {
        child.Unmount();
    }
}