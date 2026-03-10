using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Flutter.Rendering;
using Flutter.UI;

// Dart parity source (reference): flutter/packages/flutter/lib/src/rendering/paragraph.dart (approximate)

namespace Flutter;

public sealed class RenderParagraph : RenderBox
{
    public string Text { get; set; }
    public Typeface Typeface { get; set; } = new Typeface("Segoe UI");
    public double FontSize { get; set; } = 20;
    public IBrush Foreground { get; set; } = Brushes.White;
    private TextLayout? _layout;

    public RenderParagraph(string text) => Text = text;

    protected override void PerformLayout()
    {
        var maxWidth = double.IsInfinity(Constraints.MaxWidth) ? 1000 : Constraints.MaxWidth;

        try
        {
            _layout = new TextLayout(
                text: Text,
                typeface: Typeface,
                fontSize: FontSize,
                foreground: Foreground,
                maxWidth: maxWidth
            );

            Size = Constraints.Constrain(new Size(_layout.Width, _layout.Height));
        }
        catch (Exception exception) when (TextLayoutFallback.IsMissingFontManager(exception))
        {
            _layout = null;
            Size = Constraints.Constrain(TextLayoutFallback.EstimateTextSize(Text, FontSize, maxWidth));
        }
    }

    public override void Paint(PaintingContext ctx, Point offset)
    {
        if (_layout != null)
        {
            ctx.DrawTextLayout(_layout, offset);
        }
    }

    protected override void DescribeSemanticsConfiguration(SemanticsConfiguration configuration)
    {
        configuration.IsSemanticBoundary = true;
        configuration.Label = Text;
    }
}
