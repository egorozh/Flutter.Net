using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.UI;

// Dart parity source (reference): flutter/packages/flutter/lib/src/widgets/text.dart (approximate)

namespace Flutter.Widgets;

public sealed class Text : LeafRenderObjectWidget
{
    public Text(
        string data,
        double? fontSize = null,
        Color? color = null,
        TextAlign textAlign = TextAlign.Start,
        bool softWrap = true,
        int? maxLines = null,
        TextOverflow overflow = TextOverflow.Clip,
        TextDirection textDirection = TextDirection.Ltr,
        Key? key = null) : base(key)
    {
        Data = data;
        FontSize = fontSize;
        Color = color;
        TextAlign = textAlign;
        SoftWrap = softWrap;
        MaxLines = maxLines;
        Overflow = overflow;
        TextDirection = textDirection;
    }

    public string Data { get; }

    public double? FontSize { get; }

    public Color? Color { get; }

    public TextAlign TextAlign { get; }

    public bool SoftWrap { get; }

    public int? MaxLines { get; }

    public TextOverflow Overflow { get; }

    public TextDirection TextDirection { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderParagraph(Data)
        {
            FontSize = FontSize ?? 14,
            Foreground = new SolidColorBrush(Color ?? Colors.Black),
            TextAlign = TextAlign,
            SoftWrap = SoftWrap,
            MaxLines = MaxLines,
            Overflow = Overflow,
            TextDirection = TextDirection
        };
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        var paragraph = (RenderParagraph)renderObject;
        paragraph.Text = Data;
        paragraph.FontSize = FontSize ?? 14;
        paragraph.Foreground = new SolidColorBrush(Color ?? Colors.Black);
        paragraph.TextAlign = TextAlign;
        paragraph.SoftWrap = SoftWrap;
        paragraph.MaxLines = MaxLines;
        paragraph.Overflow = Overflow;
        paragraph.TextDirection = TextDirection;
    }
}
