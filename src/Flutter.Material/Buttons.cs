using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.Widgets;

namespace Flutter.Material;

// Dart parity source (reference): flutter/packages/flutter/lib/src/material/text_button.dart; flutter/packages/flutter/lib/src/material/elevated_button.dart; flutter/packages/flutter/lib/src/material/outlined_button.dart (approximate)

public sealed class TextButton : StatelessWidget
{
    public TextButton(
        Widget child,
        Action? onPressed,
        Color? foregroundColor = null,
        Color? backgroundColor = null,
        Thickness? padding = null,
        BorderRadius? borderRadius = null,
        double minHeight = 40,
        Key? key = null) : base(key)
    {
        Child = child;
        OnPressed = onPressed;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Padding = padding;
        BorderRadius = borderRadius;
        MinHeight = minHeight;
    }

    public Widget Child { get; }

    public Action? OnPressed { get; }

    public Color? ForegroundColor { get; }

    public Color? BackgroundColor { get; }

    public Thickness? Padding { get; }

    public BorderRadius? BorderRadius { get; }

    public double MinHeight { get; }

    public override Widget Build(BuildContext context)
    {
        var theme = Theme.Of(context);
        var foreground = ForegroundColor ?? theme.PrimaryColor;

        return new MaterialButtonCore(
            child: Child,
            onPressed: OnPressed,
            foregroundColor: foreground,
            backgroundColor: BackgroundColor,
            border: null,
            padding: Padding ?? new Thickness(12, 8),
            borderRadius: BorderRadius ?? Flutter.Rendering.BorderRadius.Circular(8),
            minHeight: MinHeight);
    }
}

public sealed class ElevatedButton : StatelessWidget
{
    public ElevatedButton(
        Widget child,
        Action? onPressed,
        Color? foregroundColor = null,
        Color? backgroundColor = null,
        Thickness? padding = null,
        BorderRadius? borderRadius = null,
        double minHeight = 40,
        Key? key = null) : base(key)
    {
        Child = child;
        OnPressed = onPressed;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Padding = padding;
        BorderRadius = borderRadius;
        MinHeight = minHeight;
    }

    public Widget Child { get; }

    public Action? OnPressed { get; }

    public Color? ForegroundColor { get; }

    public Color? BackgroundColor { get; }

    public Thickness? Padding { get; }

    public BorderRadius? BorderRadius { get; }

    public double MinHeight { get; }

    public override Widget Build(BuildContext context)
    {
        var theme = Theme.Of(context);

        return new MaterialButtonCore(
            child: Child,
            onPressed: OnPressed,
            foregroundColor: ForegroundColor ?? theme.OnPrimaryColor,
            backgroundColor: BackgroundColor ?? theme.PrimaryColor,
            border: null,
            padding: Padding ?? new Thickness(16, 10),
            borderRadius: BorderRadius ?? Flutter.Rendering.BorderRadius.Circular(10),
            minHeight: MinHeight);
    }
}

public sealed class OutlinedButton : StatelessWidget
{
    public OutlinedButton(
        Widget child,
        Action? onPressed,
        Color? foregroundColor = null,
        Color? borderColor = null,
        double borderWidth = 1,
        Color? backgroundColor = null,
        Thickness? padding = null,
        BorderRadius? borderRadius = null,
        double minHeight = 40,
        Key? key = null) : base(key)
    {
        if (double.IsNaN(borderWidth) || double.IsInfinity(borderWidth) || borderWidth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(borderWidth), "Border width must be non-negative and finite.");
        }

        Child = child;
        OnPressed = onPressed;
        ForegroundColor = foregroundColor;
        BorderColor = borderColor;
        BorderWidth = borderWidth;
        BackgroundColor = backgroundColor;
        Padding = padding;
        BorderRadius = borderRadius;
        MinHeight = minHeight;
    }

    public Widget Child { get; }

    public Action? OnPressed { get; }

    public Color? ForegroundColor { get; }

    public Color? BorderColor { get; }

    public double BorderWidth { get; }

    public Color? BackgroundColor { get; }

    public Thickness? Padding { get; }

    public BorderRadius? BorderRadius { get; }

    public double MinHeight { get; }

    public override Widget Build(BuildContext context)
    {
        var theme = Theme.Of(context);
        var resolvedBorderColor = BorderColor ?? theme.PrimaryColor;

        return new MaterialButtonCore(
            child: Child,
            onPressed: OnPressed,
            foregroundColor: ForegroundColor ?? resolvedBorderColor,
            backgroundColor: BackgroundColor,
            border: new BorderSide(resolvedBorderColor, BorderWidth),
            padding: Padding ?? new Thickness(16, 10),
            borderRadius: BorderRadius ?? Flutter.Rendering.BorderRadius.Circular(10),
            minHeight: MinHeight);
    }
}

internal sealed class MaterialButtonCore : StatelessWidget
{
    public MaterialButtonCore(
        Widget child,
        Action? onPressed,
        Color foregroundColor,
        Color? backgroundColor,
        BorderSide? border,
        Thickness padding,
        BorderRadius borderRadius,
        double minHeight,
        Key? key = null) : base(key)
    {
        if (double.IsNaN(minHeight) || double.IsInfinity(minHeight) || minHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minHeight), "Minimum height must be positive and finite.");
        }

        Child = child;
        OnPressed = onPressed;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Border = border;
        Padding = padding;
        BorderRadius = borderRadius;
        MinHeight = minHeight;
    }

    public Widget Child { get; }

    public Action? OnPressed { get; }

    public Color ForegroundColor { get; }

    public Color? BackgroundColor { get; }

    public BorderSide? Border { get; }

    public Thickness Padding { get; }

    public BorderRadius BorderRadius { get; }

    public double MinHeight { get; }

    public override Widget Build(BuildContext context)
    {
        var enabled = OnPressed != null;
        var foreground = enabled
            ? ForegroundColor
            : ReduceAlpha(ForegroundColor, 0.38);
        var background = ResolveBackgroundColor(enabled);
        var border = ResolveBorder(enabled);

        Widget content = new DefaultTextStyle(
            style: new TextStyle(
                Color: foreground,
                FontSize: 14,
                FontWeight: FontWeight.Medium),
            child: new Center(child: Child));

        content = new Container(
            padding: Padding,
            child: content);

        content = new ConstrainedBox(
            constraints: new BoxConstraints(
                MinWidth: 0,
                MaxWidth: double.PositiveInfinity,
                MinHeight: MinHeight,
                MaxHeight: double.PositiveInfinity),
            child: content);

        content = new DecoratedBox(
            decoration: new BoxDecoration(
                Color: background,
                Border: border,
                BorderRadius: BorderRadius),
            child: content);

        if (!enabled)
        {
            return content;
        }

        return new GestureDetector(
            behavior: HitTestBehavior.Opaque,
            onTap: OnPressed,
            child: content);
    }

    private Color? ResolveBackgroundColor(bool enabled)
    {
        if (!BackgroundColor.HasValue)
        {
            return null;
        }

        var color = BackgroundColor.Value;
        if (!enabled)
        {
            return ReduceAlpha(color, 0.12);
        }

        return color;
    }

    private BorderSide? ResolveBorder(bool enabled)
    {
        if (!Border.HasValue)
        {
            return null;
        }

        var border = Border.Value;
        if (enabled)
        {
            return border;
        }

        return new BorderSide(
            color: ReduceAlpha(border.Color, 0.24),
            width: border.Width);
    }

    private static Color ReduceAlpha(Color color, double factor)
    {
        var alpha = (byte)Math.Clamp((int)(color.A * factor), 0, 255);
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }
}
