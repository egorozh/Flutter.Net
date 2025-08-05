using Avalonia.Controls;
using Avalonia.Layout;

namespace Flutter.Widgets;

public readonly record struct Column(IReadOnlyList<IWidget> Children) : IWidget
{
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