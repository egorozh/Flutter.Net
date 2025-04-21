using System;

namespace Flutter.Net.Flutter.Framework;

public interface IWidget
{
    Element CreateElement() => this switch
    {
        IStatelessWidget s => s.CreateElement(),
        IStatefulWidget sf => sf.CreateElement(),
        _ => throw new NotImplementedException(),
    };
}

public interface IStatelessWidget : IWidget
{
    IWidget Build(IBuildContext context);

    new Element CreateElement() => new StatelessElement(this);
}

public interface IStatefulWidget : IWidget
{
    State CreateState();

    public new Element CreateElement() => new StatefulElement(this);
}

public abstract class State
{
    public IStatefulWidget Widget { get; private set; } = null!;
    public StatefulElement Element { get; internal set; } = null!;

    protected IBuildContext Context => Element;

    public void _Attach(IStatefulWidget widget, StatefulElement element)
    {
        widget = widget;
        Element = element;
    }

    public virtual void InitState()
    {
    }

    public virtual void Dispose()
    {
    }

    public virtual void DidUpdateIWidget(IStatefulWidget oldWidget)
    {
    }

    public abstract IWidget Build(IBuildContext context);

    public void SetState(Action update)
    {
        update();
        Element.Rebuild();
    }
}