using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Flutter.Rendering;
using Flutter.UI;

// Dart parity source (reference): flutter/packages/flutter/lib/src/rendering/paragraph.dart (approximate)

namespace Flutter;

public sealed class RenderParagraph : RenderBox
{
    private string _text;
    private Typeface _typeface = new Typeface("Segoe UI");
    private double _fontSize = 20;
    private IBrush _foreground = Brushes.White;
    private TextAlign _textAlign = TextAlign.Start;
    private TextDirection _textDirection = TextDirection.Ltr;
    private bool _softWrap = true;
    private int? _maxLines;
    private TextOverflow _overflow = TextOverflow.Clip;
    private TextLayout? _layout;

    public RenderParagraph(string text)
    {
        _text = text ?? string.Empty;
    }

    public string Text
    {
        get => _text;
        set
        {
            var next = value ?? string.Empty;
            if (string.Equals(_text, next, StringComparison.Ordinal))
            {
                return;
            }

            _text = next;
            MarkNeedsLayout();
            MarkNeedsSemanticsUpdate();
        }
    }

    public Typeface Typeface
    {
        get => _typeface;
        set
        {
            var next = value;
            if (Equals(_typeface, next))
            {
                return;
            }

            _typeface = next;
            MarkNeedsLayout();
        }
    }

    public double FontSize
    {
        get => _fontSize;
        set
        {
            if (Math.Abs(_fontSize - value) < 0.01)
            {
                return;
            }

            _fontSize = value;
            MarkNeedsLayout();
        }
    }

    public IBrush Foreground
    {
        get => _foreground;
        set
        {
            var next = value ?? Brushes.White;
            if (Equals(_foreground, next))
            {
                return;
            }

            _foreground = next;
            MarkNeedsPaint();
        }
    }

    public TextAlign TextAlign
    {
        get => _textAlign;
        set
        {
            if (_textAlign == value)
            {
                return;
            }

            _textAlign = value;
            MarkNeedsLayout();
        }
    }

    public TextDirection TextDirection
    {
        get => _textDirection;
        set
        {
            if (_textDirection == value)
            {
                return;
            }

            _textDirection = value;
            MarkNeedsLayout();
        }
    }

    public bool SoftWrap
    {
        get => _softWrap;
        set
        {
            if (_softWrap == value)
            {
                return;
            }

            _softWrap = value;
            MarkNeedsLayout();
        }
    }

    public int? MaxLines
    {
        get => _maxLines;
        set
        {
            if (_maxLines == value)
            {
                return;
            }

            _maxLines = value;
            MarkNeedsLayout();
        }
    }

    public TextOverflow Overflow
    {
        get => _overflow;
        set
        {
            if (_overflow == value)
            {
                return;
            }

            _overflow = value;
            MarkNeedsLayout();
        }
    }

    protected override void PerformLayout()
    {
        var maxWidth = double.IsInfinity(Constraints.MaxWidth)
            ? double.PositiveInfinity
            : Math.Max(0, Constraints.MaxWidth);
        var maxHeight = double.IsInfinity(Constraints.MaxHeight)
            ? double.PositiveInfinity
            : Math.Max(0, Constraints.MaxHeight);

        try
        {
            _layout = new TextLayout(
                text: _text,
                typeface: _typeface,
                fontSize: _fontSize,
                foreground: _foreground,
                textAlignment: ResolveTextAlignment(_textAlign, _textDirection),
                textWrapping: _softWrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
                textTrimming: ResolveTextTrimming(_overflow),
                flowDirection: _textDirection == TextDirection.Rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                maxWidth: maxWidth,
                maxHeight: maxHeight,
                maxLines: _maxLines ?? 0);

            Size = Constraints.Constrain(new Size(_layout.Width, _layout.Height));
        }
        catch (Exception exception) when (TextLayoutFallback.IsMissingFontManager(exception))
        {
            _layout = null;
            Size = Constraints.Constrain(TextLayoutFallback.EstimateTextSize(_text, _fontSize, maxWidth));
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
        configuration.Label = _text;
    }

    private static TextAlignment ResolveTextAlignment(TextAlign align, TextDirection direction)
    {
        return align switch
        {
            TextAlign.Left => TextAlignment.Left,
            TextAlign.Right => TextAlignment.Right,
            TextAlign.Center => TextAlignment.Center,
            TextAlign.Justify => TextAlignment.Justify,
            TextAlign.End => direction == TextDirection.Rtl ? TextAlignment.Left : TextAlignment.Right,
            _ => direction == TextDirection.Rtl ? TextAlignment.Right : TextAlignment.Left
        };
    }

    private static TextTrimming ResolveTextTrimming(TextOverflow overflow)
    {
        return overflow switch
        {
            TextOverflow.Ellipsis => TextTrimming.CharacterEllipsis,
            _ => TextTrimming.None
        };
    }
}
