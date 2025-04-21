using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Flutter.Net.Flutter.Framework;

public abstract class Widget
{
    public abstract Element CreateElement();
}

public abstract class Element(Widget widget) : IBuildContext
{
    public Widget Widget { get; protected set; } = widget;
    public Control? RenderObject { get; protected set; }

    public abstract void Mount(Control parent);
    
    public abstract void Update(Widget newWidget);

    public virtual void Dispose()
    {
    }
}

public interface IBuildContext
{
}

// StatelessWidget
public abstract class StatelessWidget : Widget
{
    public abstract Widget Build(IBuildContext context);

    public override Element CreateElement() => new StatelessElement(this);
}

public class StatelessElement(StatelessWidget widget) : Element(widget)
{
    private Element? _child;

    public new StatelessWidget Widget
    {
        get => (StatelessWidget)base.Widget;
        set => base.Widget = value;
    }

    public override void Mount(Control parent)
    {
        var built = Widget.Build(this);
        _child = built.CreateElement();
        _child.Mount(parent);
        RenderObject = _child.RenderObject;
    }

    public override void Update(Widget newWidget)
    {
        if (newWidget is StatelessWidget newStateless)
        {
            Widget = newStateless;
            var newChild = newStateless.Build(this).CreateElement();
            _child?.Update(newChild.Widget);
        }
    }
}

// StatefulWidget
public abstract class StatefulWidget : Widget
{
    public abstract IState CreateState();

    public override Element CreateElement() => new StatefulElement(this);
}

public interface IState
{
    void _Attach(StatefulWidget widget, StatefulElement element);
    void InitState();
    void Dispose();
    void DidUpdateWidget(StatefulWidget oldWidget);
    Widget Build(IBuildContext context);
    void SetState(Action update);
}

public abstract class State : IState
{
    public StatefulWidget Widget { get; private set; } = null!;
    public StatefulElement Element { get; internal set; } = null!;

    protected IBuildContext Context => Element;

    public void _Attach(StatefulWidget widget, StatefulElement element)
    {
        Widget = widget;
        Element = element;
    }

    public virtual void InitState()
    {
    }

    public virtual void Dispose()
    {
    }

    public virtual void DidUpdateWidget(StatefulWidget oldWidget)
    {
    }

    public abstract Widget Build(IBuildContext context);

    public void SetState(Action update)
    {
        update();
        Element.Rebuild();
    }
}

public class StatefulElement : Element
{
    private Element? _child;
    private IState _state;

    public new StatefulWidget Widget
    {
        get => (StatefulWidget)base.Widget;
        set => base.Widget = value;
    }

    public StatefulElement(StatefulWidget widget) : base(widget)
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

    public override void Update(Widget newWidget)
    {
        if (newWidget is StatefulWidget newStateful)
        {
            var oldWidget = Widget;
            Widget = newStateful;
            _state._Attach(newStateful, this);
            _state.DidUpdateWidget(oldWidget);
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

// Примитивные виджеты
public class Text(string value) : Widget
{
    public string Value { get; } = value;

    public override Element CreateElement() => new TextElement(this);
}

public class TextElement(Text widget) : Element(widget)
{
    public new Text Widget
    {
        get => (Text)base.Widget;
        set => base.Widget = value;
    }

    public override void Mount(Control parent)
    {
        RenderObject = new TextBlock { Text = Widget.Value };
        if (parent is Panel panel)
            panel.Children.Add(RenderObject);
    }

    public override void Update(Widget newWidget)
    {
        if (newWidget is Text newText)
        {
            Widget = newText;
            ((TextBlock)RenderObject).Text = newText.Value;
        }
    }
}

public class Column(IEnumerable<Widget> children) : Widget
{
    public IReadOnlyList<Widget> Children { get; } = children.ToList();

    public override Element CreateElement() => new ColumnElement(this);
}

public class ColumnElement(Column widget) : Element(widget)
{
    private readonly List<Element> _children = new();

    public new Column Widget => (Column)base.Widget;

    public override void Mount(Control parent)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical };
        foreach (var childWidget in Widget.Children)
        {
            var childElement = childWidget.CreateElement();
            childElement.Mount(panel);
            _children.Add(childElement);
        }

        RenderObject = panel;
        if (parent is Panel parentPanel)
            parentPanel.Children.Add(panel);
    }

    public override void Update(Widget newWidget)
    {
        // На этом этапе без диффинга
    }
}

public class Button(string text, Action onPressed) : Widget
{
    public string Text { get; } = text;
    public Action OnPressed { get; } = onPressed;

    public override Element CreateElement() => new ButtonElement(this);
}

public class ButtonElement(Button widget) : Element(widget)
{
    public new Button Widget
    {
        get => (Button)base.Widget;
        set => base.Widget = value;
    }


    public override void Mount(Control parent)
    {
        var button = new Avalonia.Controls.Button { Content = Widget.Text };
        button.Click += (_, _) => Widget.OnPressed();
        RenderObject = button;

        if (parent is Panel panel)
            panel.Children.Add(button);
    }

    public override void Update(Widget newWidget)
    {
        if (newWidget is Button newButton)
        {
            Widget = newButton;
            if (RenderObject is Avalonia.Controls.Button btn)
            {
                btn.Content = newButton.Text;
                btn.Click += (_, _) => newButton.OnPressed();
            }
        }
    }
}