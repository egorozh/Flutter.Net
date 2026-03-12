using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.UI;
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
        double minWidth = 64,
        double minHeight = 40,
        Key? key = null) : base(key)
    {
        Child = child;
        OnPressed = onPressed;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Padding = padding;
        BorderRadius = borderRadius;
        MinWidth = minWidth;
        MinHeight = minHeight;
    }

    public Widget Child { get; }

    public Action? OnPressed { get; }

    public Color? ForegroundColor { get; }

    public Color? BackgroundColor { get; }

    public Thickness? Padding { get; }

    public BorderRadius? BorderRadius { get; }

    public double MinWidth { get; }

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
            stateColor: foreground,
            disabledForegroundColor: MaterialButtonCore.ApplyOpacity(theme.OnSurfaceColor, 0.38),
            padding: Padding ?? new Thickness(12, 8),
            borderRadius: BorderRadius ?? Flutter.Rendering.BorderRadius.Circular(20),
            minWidth: MinWidth,
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
        double minWidth = 64,
        double minHeight = 40,
        Key? key = null) : base(key)
    {
        Child = child;
        OnPressed = onPressed;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Padding = padding;
        BorderRadius = borderRadius;
        MinWidth = minWidth;
        MinHeight = minHeight;
    }

    public Widget Child { get; }

    public Action? OnPressed { get; }

    public Color? ForegroundColor { get; }

    public Color? BackgroundColor { get; }

    public Thickness? Padding { get; }

    public BorderRadius? BorderRadius { get; }

    public double MinWidth { get; }

    public double MinHeight { get; }

    public override Widget Build(BuildContext context)
    {
        var theme = Theme.Of(context);
        var background = BackgroundColor ?? theme.SurfaceContainerLowColor;
        var foreground = ForegroundColor ?? theme.PrimaryColor;

        return new MaterialButtonCore(
            child: Child,
            onPressed: OnPressed,
            foregroundColor: foreground,
            backgroundColor: background,
            border: null,
            stateColor: foreground,
            disabledForegroundColor: MaterialButtonCore.ApplyOpacity(theme.OnSurfaceColor, 0.38),
            disabledBackgroundColor: MaterialButtonCore.ApplyOpacity(theme.OnSurfaceColor, 0.12),
            padding: Padding ?? new Thickness(24, 8),
            borderRadius: BorderRadius ?? Flutter.Rendering.BorderRadius.Circular(20),
            minWidth: MinWidth,
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
        double minWidth = 64,
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
        MinWidth = minWidth;
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

    public double MinWidth { get; }

    public double MinHeight { get; }

    public override Widget Build(BuildContext context)
    {
        var theme = Theme.Of(context);
        var resolvedBorderColor = BorderColor ?? theme.OutlineColor;

        return new MaterialButtonCore(
            child: Child,
            onPressed: OnPressed,
            foregroundColor: ForegroundColor ?? theme.PrimaryColor,
            backgroundColor: BackgroundColor,
            border: new BorderSide(resolvedBorderColor, BorderWidth),
            stateColor: ForegroundColor ?? theme.PrimaryColor,
            disabledForegroundColor: MaterialButtonCore.ApplyOpacity(theme.OnSurfaceColor, 0.38),
            disabledBorderColor: MaterialButtonCore.ApplyOpacity(theme.OnSurfaceColor, 0.12),
            padding: Padding ?? new Thickness(24, 8),
            borderRadius: BorderRadius ?? Flutter.Rendering.BorderRadius.Circular(20),
            minWidth: MinWidth,
            minHeight: MinHeight);
    }
}

internal sealed class MaterialButtonCore : StatefulWidget
{
    public MaterialButtonCore(
        Widget child,
        Action? onPressed,
        Color foregroundColor,
        Color? backgroundColor,
        BorderSide? border,
        Color stateColor,
        Thickness padding,
        BorderRadius borderRadius,
        double minWidth,
        double minHeight,
        Color? disabledForegroundColor = null,
        Color? disabledBackgroundColor = null,
        Color? disabledBorderColor = null,
        Key? key = null) : base(key)
    {
        if (double.IsNaN(minWidth) || double.IsInfinity(minWidth) || minWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minWidth), "Minimum width must be positive and finite.");
        }

        if (double.IsNaN(minHeight) || double.IsInfinity(minHeight) || minHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minHeight), "Minimum height must be positive and finite.");
        }

        Child = child;
        OnPressed = onPressed;
        ForegroundColor = foregroundColor;
        BackgroundColor = backgroundColor;
        Border = border;
        StateColor = stateColor;
        DisabledForegroundColor = disabledForegroundColor;
        DisabledBackgroundColor = disabledBackgroundColor;
        DisabledBorderColor = disabledBorderColor;
        Padding = padding;
        BorderRadius = borderRadius;
        MinWidth = minWidth;
        MinHeight = minHeight;
    }

    public Widget Child { get; }

    public Action? OnPressed { get; }

    public Color ForegroundColor { get; }

    public Color? BackgroundColor { get; }

    public BorderSide? Border { get; }

    public Color StateColor { get; }

    public Color? DisabledForegroundColor { get; }

    public Color? DisabledBackgroundColor { get; }

    public Color? DisabledBorderColor { get; }

    public Thickness Padding { get; }

    public BorderRadius BorderRadius { get; }

    public double MinWidth { get; }

    public double MinHeight { get; }

    public override State CreateState()
    {
        return new MaterialButtonCoreState();
    }

    private sealed class MaterialButtonCoreState : State
    {
        private bool _isPressed;
        private bool _hasFocus;
        private bool _isHovered;
        private FocusNode? _focusNode;

        private MaterialButtonCore CurrentWidget => (MaterialButtonCore)StateWidget;

        private bool Enabled => CurrentWidget.OnPressed != null;

        public override void InitState()
        {
            _focusNode = new FocusNode();
            _focusNode.AddListener(HandleFocusChanged);
            _hasFocus = _focusNode.HasFocus;
        }

        public override void DidUpdateWidget(StatefulWidget oldWidget)
        {
            if (!Enabled && _isPressed)
            {
                _isPressed = false;
            }

            if (!Enabled && _isHovered)
            {
                _isHovered = false;
            }

            if (!Enabled && _focusNode != null && _focusNode.HasFocus)
            {
                _focusNode.Unfocus();
            }
        }

        public override void Dispose()
        {
            if (_focusNode != null)
            {
                _focusNode.RemoveListener(HandleFocusChanged);
                _focusNode.Dispose();
                _focusNode = null;
            }
        }

        public override Widget Build(BuildContext context)
        {
            var widget = CurrentWidget;
            var enabled = Enabled;
            var foreground = enabled
                ? widget.ForegroundColor
                : widget.DisabledForegroundColor ?? ReduceAlpha(widget.ForegroundColor, 0.38);
            var background = ResolveBackgroundColor(enabled);
            var border = ResolveBorder(enabled);

            Widget content = new DefaultTextStyle(
                style: new TextStyle(
                    Color: foreground,
                    FontSize: 14,
                    FontWeight: FontWeight.Medium),
                child: new Align(
                    alignment: Alignment.Center,
                    widthFactor: 1,
                    heightFactor: 1,
                    child: widget.Child));

            content = new Container(
                padding: widget.Padding,
                child: content);

            content = new ConstrainedBox(
                constraints: new BoxConstraints(
                    MinWidth: widget.MinWidth,
                    MaxWidth: double.PositiveInfinity,
                    MinHeight: widget.MinHeight,
                    MaxHeight: double.PositiveInfinity),
                child: content);

            content = new DecoratedBox(
                decoration: new BoxDecoration(
                    Color: background,
                    Border: border,
                    BorderRadius: widget.BorderRadius),
                child: content);

            if (!enabled)
            {
                return content;
            }

            content = new GestureDetector(
                behavior: HitTestBehavior.Opaque,
                onTap: widget.OnPressed,
                child: content);

            content = new Listener(
                behavior: HitTestBehavior.Opaque,
                onPointerDown: _ => SetPressed(true),
                onPointerUp: _ => SetPressed(false),
                onPointerCancel: _ => SetPressed(false),
                onPointerEnter: _ => SetHovered(true),
                onPointerExit: _ => SetHovered(false),
                child: content);

            return new Focus(
                focusNode: _focusNode,
                canRequestFocus: true,
                onKeyEvent: HandleKeyEvent,
                child: content);
        }

        private KeyEventResult HandleKeyEvent(FocusNode node, KeyEvent @event)
        {
            if (!IsActivateKey(@event.Key))
            {
                return KeyEventResult.Ignored;
            }

            if (!Enabled)
            {
                return KeyEventResult.Handled;
            }

            if (@event.IsDown)
            {
                CurrentWidget.OnPressed?.Invoke();
            }

            return KeyEventResult.Handled;
        }

        private void HandleFocusChanged()
        {
            var hasFocus = _focusNode?.HasFocus ?? false;
            if (_hasFocus == hasFocus)
            {
                return;
            }

            SetState(() =>
            {
                _hasFocus = hasFocus;
            });
        }

        private void SetPressed(bool value)
        {
            if (!Enabled || _isPressed == value)
            {
                return;
            }

            SetState(() => _isPressed = value);
        }

        private void SetHovered(bool value)
        {
            if (!Enabled || _isHovered == value)
            {
                return;
            }

            SetState(() => _isHovered = value);
        }

        private Color? ResolveBackgroundColor(bool enabled)
        {
            var widget = CurrentWidget;
            var overlay = ResolveStateLayerColor();

            if (!widget.BackgroundColor.HasValue)
            {
                if (!enabled)
                {
                    return null;
                }

                return overlay;
            }

            var color = widget.BackgroundColor.Value;
            if (!enabled)
            {
                return widget.DisabledBackgroundColor ?? ReduceAlpha(color, 0.12);
            }

            return overlay.HasValue
                ? BlendColorOverlay(color, overlay.Value)
                : color;
        }

        private BorderSide? ResolveBorder(bool enabled)
        {
            var widget = CurrentWidget;
            if (!widget.Border.HasValue)
            {
                return null;
            }

            var border = widget.Border.Value;
            if (enabled)
            {
                return border;
            }

            return new BorderSide(
                color: widget.DisabledBorderColor ?? ReduceAlpha(border.Color, 0.12),
                width: border.Width);
        }

        private Color? ResolveStateLayerColor()
        {
            if (_isPressed || _hasFocus)
            {
                return ReduceAlpha(CurrentWidget.StateColor, 0.10);
            }

            if (_isHovered)
            {
                return ReduceAlpha(CurrentWidget.StateColor, 0.08);
            }

            return null;
        }

        private static bool IsActivateKey(string key)
        {
            return string.Equals(key, "Enter", StringComparison.Ordinal)
                   || string.Equals(key, "Return", StringComparison.Ordinal)
                   || string.Equals(key, "Space", StringComparison.Ordinal)
                   || string.Equals(key, "Spacebar", StringComparison.Ordinal);
        }
    }

    private static Color BlendColorOverlay(Color baseColor, Color overlayColor)
    {
        static byte Blend(byte from, byte to, double t)
        {
            return (byte)Math.Clamp((int)(from + ((to - from) * t)), 0, 255);
        }

        var clampedOpacity = Math.Clamp(overlayColor.A / 255.0, 0, 1);
        return Color.FromArgb(
            baseColor.A,
            Blend(baseColor.R, overlayColor.R, clampedOpacity),
            Blend(baseColor.G, overlayColor.G, clampedOpacity),
            Blend(baseColor.B, overlayColor.B, clampedOpacity));
    }

    private static Color ReduceAlpha(Color color, double factor)
    {
        var alpha = (byte)Math.Clamp((int)(color.A * factor), 0, 255);
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }

    internal static Color ApplyOpacity(Color color, double opacity)
    {
        var alpha = (byte)Math.Clamp((int)(255 * opacity), 0, 255);
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }
}
