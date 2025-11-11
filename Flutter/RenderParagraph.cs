using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Flutter.Rendering;

namespace Flutter;

public sealed class RenderParagraph : RenderBox
{
    public string Text { get; set; }
    public Typeface Typeface { get; set; } = new Typeface("Segoe UI");
    public double FontSize { get; set; } = 20;
    public IBrush Foreground { get; set; } = Brushes.White;
    private TextLayout? _layout;

    public RenderParagraph(string text) => Text = text;

    public override void Layout(BoxConstraints constraints)
    {
        _layout = new TextLayout(
            text: Text,
            typeface: Typeface,
            fontSize: FontSize,
            foreground: Foreground,
            maxWidth: double.IsInfinity(constraints.MaxWidth) ? 1000 : constraints.MaxWidth
        );

        Size = constraints.Constrain(new Size(_layout.Width, _layout.Height));
    }

    public override void Paint(DrawingContext ctx, Point offset)
    {
        _layout?.Draw(ctx, offset);
    }
}