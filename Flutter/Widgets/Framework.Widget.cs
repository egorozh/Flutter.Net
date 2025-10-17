using Flutter.Foundation;

namespace Flutter.Widgets;

/// <summary>
/// Describes the configuration for an [Element].
///
/// Widgets are the central class hierarchy in the Flutter framework. A widget
/// is an immutable description of part of a user interface. Widgets can be
/// inflated into elements, which manage the underlying render tree.
///
/// </summary>
public abstract class Widget(Key? key = null)
{
    public Key? Key { get; } = key;

    internal abstract Element CreateElement();
}

public abstract class StatelessWidget : Widget
{
    protected StatelessWidget(Key? key = null) : base(key)
    {
    }

    public abstract Widget Build(BuildContext context);
    internal override Element CreateElement() => new StatelessElement(this);
}

public abstract class StatefulWidget : Widget
{
    protected StatefulWidget(Key? key = null) : base(key)
    {
    }

    public abstract State CreateState();
    internal override Element CreateElement() => new StatefulElement(this);
}

public abstract class State
{
    internal StatefulElement Element = null!;
    public BuildContext Context => new(Element);

    public virtual void InitState()
    {
    }

    public virtual void DidUpdateWidget(StatefulWidget oldWidget)
    {
    }

    public virtual void Dispose()
    {
    }

    public abstract Widget Build(BuildContext context);

    protected void SetState(Action updater)
    {
        updater();
        Element.MarkNeedsBuild();
    }

    // helper for external callers
    public void InvokeSetState(Action updater) => SetState(updater);
}

// Inherited widgets
public abstract class InheritedWidget : Widget
{
    protected InheritedWidget(Key? key = null) : base(key)
    {
    }

    public abstract Widget Build(BuildContext context);
    
    protected internal abstract bool UpdateShouldNotify(InheritedWidget oldWidget);
    
    internal override Element CreateElement() => new InheritedElement(this);
}