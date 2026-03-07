using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;

namespace Flutter.Widgets;

public sealed class Text : LeafRenderObjectWidget
{
    public Text(string data, double? fontSize = null, Color? color = null, Key? key = null) : base(key)
    {
        Data = data;
        FontSize = fontSize;
        Color = color;
    }

    public string Data { get; }

    public double? FontSize { get; }

    public Color? Color { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderParagraph(Data)
        {
            FontSize = FontSize ?? 14,
            Foreground = new SolidColorBrush(Color ?? Colors.Black)
        };
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        var paragraph = (RenderParagraph)renderObject;
        paragraph.Text = Data;
        paragraph.FontSize = FontSize ?? 14;
        paragraph.Foreground = new SolidColorBrush(Color ?? Colors.Black);
        paragraph.MarkNeedsLayout();
    }
}
