using Avalonia.Controls;

namespace Flutter.Widgets;

public readonly record struct Text(string Value) : IWidget
{
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