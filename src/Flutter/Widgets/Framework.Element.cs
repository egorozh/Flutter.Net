using Flutter.Foundation;
using Flutter.Rendering;

namespace Flutter.Widgets;

public sealed class IndexedSlot<T>
{
    public IndexedSlot(int index, T? value)
    {
        Index = index;
        Value = value;
    }

    public int Index { get; }

    public T? Value { get; }
}

public readonly struct BuildContext
{
    internal Element Owner { get; }

    internal BuildContext(Element owner)
    {
        Owner = owner;
    }

    public T? DependOnInherited<T>() where T : InheritedWidget => Owner.DependOnInherited<T>();
}

public abstract class Element
{
    private static int _nextElementId;

    public Widget Widget { get; private set; }
    public Element? Parent { get; private set; }
    public int Depth { get; private set; }
    public object? Slot { get; private set; }
    internal int SequenceId { get; } = Interlocked.Increment(ref _nextElementId);
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

    internal void Mount(Element? parent, object? newSlot)
    {
        Parent = parent;
        Slot = newSlot;
        Depth = (parent?.Depth ?? 0) + 1;
        OnMount();
    }

    protected virtual void OnMount()
    {
    }

    internal virtual void UpdateSlot(object? newSlot)
    {
        Slot = newSlot;
    }

    protected virtual void OnUnmount()
    {
    }

    internal virtual void Unmount()
    {
        OnUnmount();
        Owner?.UnregisterElement(this);
        Parent = null;
        Slot = null;
    }

    internal abstract void Rebuild();

    internal void MarkNeedsBuild()
    {
        if (Dirty)
        {
            return;
        }

        Dirty = true;
        Owner?.ScheduleBuild(this);
    }

    internal virtual void Update(Widget newWidget)
    {
        Widget = newWidget;
    }

    internal virtual void ForgetChild(Element child)
    {
    }

    internal virtual void UpdateSlotForChild(Element child, object? newSlot)
    {
        void Visit(Element element)
        {
            element.UpdateSlot(newSlot);
            if (element.RenderObjectAttachingChild is { } descendant)
            {
                Visit(descendant);
            }
        }

        Visit(child);
    }

    internal virtual void DeactivateChild(Element child)
    {
        ForgetChild(child);
        child.Unmount();
    }

    internal Element InflateWidget(Widget newWidget, object? newSlot)
    {
        var element = newWidget.CreateElement();
        element.Attach(Owner!);
        element.Mount(this, newSlot);
        return element;
    }

    internal Element? UpdateChild(Element? child, Widget? newWidget, object? newSlot)
    {
        if (newWidget == null)
        {
            if (child != null)
            {
                DeactivateChild(child);
            }

            return null;
        }

        if (child != null)
        {
            if (ReferenceEquals(child.Widget, newWidget))
            {
                if (!Equals(child.Slot, newSlot))
                {
                    UpdateSlotForChild(child, newSlot);
                }

                return child;
            }

            if (Widget.CanUpdate(child.Widget, newWidget))
            {
                if (!Equals(child.Slot, newSlot))
                {
                    UpdateSlotForChild(child, newSlot);
                }

                child.Update(newWidget);
                return child;
            }

            DeactivateChild(child);
        }

        return InflateWidget(newWidget, newSlot);
    }

    internal List<Element> UpdateChildren(
        List<Element> oldChildren,
        IReadOnlyList<Widget> newWidgets,
        HashSet<Element>? forgottenChildren = null,
        IReadOnlyList<object?>? slots = null)
    {
        if (slots != null && slots.Count != newWidgets.Count)
        {
            throw new ArgumentException("slots and newWidgets must have the same length.");
        }

        Element? ReplaceWithNullIfForgotten(Element child)
        {
            return forgottenChildren != null && forgottenChildren.Contains(child) ? null : child;
        }

        object? SlotFor(int newChildIndex, Element? previousChild)
        {
            return slots != null
                ? slots[newChildIndex]
                : new IndexedSlot<Element?>(newChildIndex, previousChild);
        }

        int newChildrenTop = 0;
        int oldChildrenTop = 0;
        int newChildrenBottom = newWidgets.Count - 1;
        int oldChildrenBottom = oldChildren.Count - 1;

        var newChildren = new Element[newWidgets.Count];

        Element? previousChild = null;

        while (oldChildrenTop <= oldChildrenBottom && newChildrenTop <= newChildrenBottom)
        {
            var oldChild = ReplaceWithNullIfForgotten(oldChildren[oldChildrenTop]);
            var newWidget = newWidgets[newChildrenTop];
            if (oldChild == null || !Widget.CanUpdate(oldChild.Widget, newWidget))
            {
                break;
            }

            var newChild = UpdateChild(oldChild, newWidget, SlotFor(newChildrenTop, previousChild))!;
            newChildren[newChildrenTop] = newChild;
            previousChild = newChild;
            newChildrenTop += 1;
            oldChildrenTop += 1;
        }

        while (oldChildrenTop <= oldChildrenBottom && newChildrenTop <= newChildrenBottom)
        {
            var oldChild = ReplaceWithNullIfForgotten(oldChildren[oldChildrenBottom]);
            var newWidget = newWidgets[newChildrenBottom];
            if (oldChild == null || !Widget.CanUpdate(oldChild.Widget, newWidget))
            {
                break;
            }

            oldChildrenBottom -= 1;
            newChildrenBottom -= 1;
        }

        var haveOldChildren = oldChildrenTop <= oldChildrenBottom;
        Dictionary<Key, Element>? oldKeyedChildren = null;
        if (haveOldChildren)
        {
            oldKeyedChildren = [];
            while (oldChildrenTop <= oldChildrenBottom)
            {
                var oldChild = ReplaceWithNullIfForgotten(oldChildren[oldChildrenTop]);
                if (oldChild != null)
                {
                    if (oldChild.Widget.Key != null)
                    {
                        oldKeyedChildren[oldChild.Widget.Key!] = oldChild;
                    }
                    else
                    {
                        DeactivateChild(oldChild);
                    }
                }

                oldChildrenTop += 1;
            }
        }

        while (newChildrenTop <= newChildrenBottom)
        {
            Element? oldChild = null;
            var newWidget = newWidgets[newChildrenTop];

            if (haveOldChildren)
            {
                var key = newWidget.Key;
                if (key != null && oldKeyedChildren!.TryGetValue(key, out var keyedOldChild))
                {
                    if (Widget.CanUpdate(keyedOldChild.Widget, newWidget))
                    {
                        oldChild = keyedOldChild;
                        oldKeyedChildren.Remove(key);
                    }
                }
            }

            var newChild = UpdateChild(oldChild, newWidget, SlotFor(newChildrenTop, previousChild))!;
            newChildren[newChildrenTop] = newChild;
            previousChild = newChild;
            newChildrenTop += 1;
        }

        newChildrenBottom = newWidgets.Count - 1;
        oldChildrenBottom = oldChildren.Count - 1;

        while (oldChildrenTop <= oldChildrenBottom && newChildrenTop <= newChildrenBottom)
        {
            var oldChild = oldChildren[oldChildrenTop];
            if (ReplaceWithNullIfForgotten(oldChild) == null)
            {
                oldChildrenTop += 1;
                continue;
            }

            var newWidget = newWidgets[newChildrenTop];
            var newChild = UpdateChild(oldChild, newWidget, SlotFor(newChildrenTop, previousChild))!;
            newChildren[newChildrenTop] = newChild;
            previousChild = newChild;
            newChildrenTop += 1;
            oldChildrenTop += 1;
        }

        if (haveOldChildren && oldKeyedChildren!.Count > 0)
        {
            foreach (var oldChild in oldKeyedChildren.Values)
            {
                if (forgottenChildren == null || !forgottenChildren.Contains(oldChild))
                {
                    DeactivateChild(oldChild);
                }
            }
        }

        return [..newChildren];
    }

    internal virtual T? DependOnInherited<T>() where T : InheritedWidget
    {
        return Parent?.DependOnInherited<T>();
    }

    public virtual RenderObject? RenderObject => null;

    internal virtual Element? RenderObjectAttachingChild => null;
}

public sealed class StatelessElement : Element
{
    private Element? _child;

    public StatelessElement(StatelessWidget widget) : base(widget)
    {
    }

    public override RenderObject? RenderObject => _child?.RenderObject;

    internal override Element? RenderObjectAttachingChild => _child;

    protected override void OnMount()
    {
        base.OnMount();
        Rebuild();
    }

    internal override void Rebuild()
    {
        Dirty = false;
        var childWidget = ((StatelessWidget)Widget).Build(new BuildContext(this));
        _child = UpdateChild(_child, childWidget, Slot);
    }

    internal override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        MarkNeedsBuild();
    }

    internal override void ForgetChild(Element child)
    {
        if (ReferenceEquals(child, _child))
        {
            _child = null;
        }
    }

    internal override void Unmount()
    {
        if (_child != null)
        {
            DeactivateChild(_child);
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

    public override RenderObject? RenderObject => _child?.RenderObject;

    internal override Element? RenderObjectAttachingChild => _child;

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
        _child = UpdateChild(_child, widget, Slot);
    }

    internal override void Update(Widget newWidget)
    {
        var old = (StatefulWidget)Widget;
        base.Update(newWidget);
        State.DidUpdateWidget(old);
        MarkNeedsBuild();
    }

    internal override void ForgetChild(Element child)
    {
        if (ReferenceEquals(child, _child))
        {
            _child = null;
        }
    }

    internal override void Unmount()
    {
        if (_child != null)
        {
            DeactivateChild(_child);
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

    public override RenderObject? RenderObject => _child?.RenderObject;

    internal override Element? RenderObjectAttachingChild => _child;

    protected override void OnMount()
    {
        base.OnMount();
        Rebuild();
    }

    internal override void Rebuild()
    {
        Dirty = false;
        var child = ((InheritedWidget)Widget).Build(new BuildContext(this));
        _child = UpdateChild(_child, child, Slot);
    }

    internal override void Update(Widget newWidget)
    {
        var old = (InheritedWidget)Widget;
        base.Update(newWidget);
        if (((InheritedWidget)newWidget).UpdateShouldNotify(old))
        {
            Owner?.MarkSubtreeNeedsBuild(this);
        }

        MarkNeedsBuild();
    }

    internal override void ForgetChild(Element child)
    {
        if (ReferenceEquals(child, _child))
        {
            _child = null;
        }
    }

    internal override void Unmount()
    {
        if (_child != null)
        {
            DeactivateChild(_child);
            _child = null;
        }

        base.Unmount();
    }
}

public class ProxyElement : Element
{
    private Element? _child;

    public ProxyElement(ProxyWidget widget) : base(widget)
    {
    }

    public override RenderObject? RenderObject => _child?.RenderObject;

    internal override Element? RenderObjectAttachingChild => _child;

    protected override void OnMount()
    {
        base.OnMount();
        Rebuild();
    }

    internal override void Rebuild()
    {
        Dirty = false;
        _child = UpdateChild(_child, ((ProxyWidget)Widget).Child, Slot);
    }

    internal override void Update(Widget newWidget)
    {
        var old = (ProxyWidget)Widget;
        base.Update(newWidget);
        Updated(old);
        MarkNeedsBuild();
    }

    protected virtual void Updated(ProxyWidget oldWidget)
    {
    }

    internal override void ForgetChild(Element child)
    {
        if (ReferenceEquals(child, _child))
        {
            _child = null;
        }
    }

    internal override void Unmount()
    {
        if (_child != null)
        {
            DeactivateChild(_child);
            _child = null;
        }

        base.Unmount();
    }
}

internal abstract class ParentDataElementBase : ProxyElement
{
    protected ParentDataElementBase(ProxyWidget widget) : base(widget)
    {
    }

    internal abstract IParentDataWidget ParentDataWidget { get; }
}

internal sealed class ParentDataElement<T> : ParentDataElementBase where T : IParentData
{
    public ParentDataElement(ParentDataWidget<T> widget) : base(widget)
    {
    }

    internal override IParentDataWidget ParentDataWidget => (IParentDataWidget)Widget;

    internal override void Rebuild()
    {
        base.Rebuild();
        ApplyParentData((ParentDataWidget<T>)Widget);
    }

    protected override void Updated(ProxyWidget oldWidget)
    {
        ApplyParentData((ParentDataWidget<T>)Widget);
    }

    private void ApplyParentData(ParentDataWidget<T> widget)
    {
        void ApplyParentDataToChild(Element child)
        {
            if (child is RenderObjectElement renderObjectElement)
            {
                renderObjectElement.UpdateParentData(widget);
                return;
            }

            if (child.RenderObjectAttachingChild != null)
            {
                ApplyParentDataToChild(child.RenderObjectAttachingChild);
            }
        }

        if (RenderObjectAttachingChild != null)
        {
            ApplyParentDataToChild(RenderObjectAttachingChild);
        }
    }
}
