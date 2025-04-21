using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Flutter.Net.Flutter.Framework;

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

// Примитивные виджеты
public class Text(string value) : IWidget
{
    public string Value { get; } = value;

    public Element CreateElement() => new TextElement(this);
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

    public override void Update(IWidget newIWidget)
    {
        if (newIWidget is Text newText)
        {
            Widget = newText;
            ((TextBlock)RenderObject).Text = newText.Value;
        }
    }
}

public class Column(IEnumerable<IWidget> children) : IWidget
{
    public IReadOnlyList<IWidget> Children { get; } = children.ToList();

    public Element CreateElement() => new ColumnElement(this);
}

public class ColumnElement(Column widget) : Element(widget)
{
    private readonly List<Element> _children = new();

    public new Column Widget => (Column)base.Widget;

    public override void Mount(Control parent)
    {
        var panel = new StackPanel { Orientation = Orientation.Vertical };
        foreach (var childIWidget in Widget.Children)
        {
            var childElement = childIWidget.CreateElement();
            childElement.Mount(panel);
            _children.Add(childElement);
        }

        RenderObject = panel;
        if (parent is Panel parentPanel)
            parentPanel.Children.Add(panel);
    }

    public override void Update(IWidget newIWidget)
    {
        // На этом этапе без диффинга
    }
}

public class Button(string text, Action onPressed) : IWidget
{
    public string Text { get; } = text;
    public Action OnPressed { get; } = onPressed;

    public Element CreateElement() => new ButtonElement(this);
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

    public override void Update(IWidget newIWidget)
    {
        if (newIWidget is Button newButton)
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