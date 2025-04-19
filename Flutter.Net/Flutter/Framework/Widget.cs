using System;

namespace Flutter.Net.Flutter.Framework;

public abstract class Widget(Key? key = null)
{
    public Key? Key { get; } = key;

    protected internal abstract Element CreateElement();
}

public abstract class StatelessWidget(Key? key = null) : Widget(key)
{
    protected internal override Element CreateElement() => new StatelessElement(this);

    internal abstract Widget Build(IBuildContext context);
}

public abstract class StatefulWidget(Key? key = null) : Widget(key)
{
    protected internal override Element CreateElement() => new StatefulElement(this);

    public abstract State CreateState();
}

public abstract class State
{
    internal StatefulElement? _element;
    internal StatefulWidget? _widget;

    public IBuildContext Context => _element!;

    public bool Mounted => _element != null;


    public StatefulWidget Widget => _widget!;

    protected internal virtual void DidUpdateWidget(StatefulWidget oldWidget)
    {
    }

    protected internal virtual void DidChangeDependencies()
    {
    }

    protected internal virtual void InitState()
    {
    }

    protected internal virtual void Dispose()
    {
    }

    protected void SetState(Action action)
    {
        action();
        _element!.MarkNeedsBuild();
    }

    internal abstract Widget Build(IBuildContext context);
}

