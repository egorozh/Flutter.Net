using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;
using Flutter.Widgets;

namespace Flutter.Material;

// Dart parity source (reference): flutter/packages/flutter/lib/src/material/button_style.dart (approximate)

[Flags]
public enum MaterialState
{
    None = 0,
    Hovered = 1 << 0,
    Focused = 1 << 1,
    Pressed = 1 << 2,
    Disabled = 1 << 3
}

public abstract class MaterialStateProperty<T>
{
    public abstract T Resolve(MaterialState states);

    public static MaterialStateProperty<T> All(T value)
    {
        return new MaterialStatePropertyAll<T>(value);
    }

    public static MaterialStateProperty<T> ResolveWith(Func<MaterialState, T> resolver)
    {
        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

        return new MaterialStatePropertyResolver<T>(resolver);
    }
}

public sealed class MaterialStatePropertyAll<T> : MaterialStateProperty<T>
{
    public MaterialStatePropertyAll(T value)
    {
        Value = value;
    }

    public T Value { get; }

    public override T Resolve(MaterialState states)
    {
        return Value;
    }
}

internal sealed class MaterialStatePropertyResolver<T> : MaterialStateProperty<T>
{
    private readonly Func<MaterialState, T> _resolver;

    public MaterialStatePropertyResolver(Func<MaterialState, T> resolver)
    {
        _resolver = resolver;
    }

    public override T Resolve(MaterialState states)
    {
        return _resolver(states);
    }
}

public sealed record ButtonStyle(
    MaterialStateProperty<Color?>? ForegroundColor = null,
    MaterialStateProperty<Color?>? BackgroundColor = null,
    MaterialStateProperty<Color?>? OverlayColor = null,
    MaterialStateProperty<Color?>? SplashColor = null,
    MaterialStateProperty<BorderSide?>? Side = null,
    MaterialStateProperty<Thickness?>? Padding = null,
    MaterialStateProperty<BorderRadius?>? Shape = null,
    MaterialStateProperty<Size?>? MinimumSize = null,
    TextStyle? TextStyle = null)
{
    public ButtonStyle Merge(ButtonStyle? style)
    {
        if (style is null)
        {
            return this;
        }

        return this with
        {
            ForegroundColor = MergeState(ForegroundColor, style.ForegroundColor),
            BackgroundColor = MergeState(BackgroundColor, style.BackgroundColor),
            OverlayColor = MergeState(OverlayColor, style.OverlayColor),
            SplashColor = MergeState(SplashColor, style.SplashColor),
            Side = MergeState(Side, style.Side),
            Padding = MergeState(Padding, style.Padding),
            Shape = MergeState(Shape, style.Shape),
            MinimumSize = MergeState(MinimumSize, style.MinimumSize),
            TextStyle = style.TextStyle ?? TextStyle
        };
    }

    private static MaterialStateProperty<T?>? MergeState<T>(
        MaterialStateProperty<T?>? lowerPriority,
        MaterialStateProperty<T?>? higherPriority)
    {
        if (higherPriority is null)
        {
            return lowerPriority;
        }

        if (lowerPriority is null)
        {
            return higherPriority;
        }

        return MaterialStateProperty<T?>.ResolveWith(states =>
        {
            var resolved = higherPriority.Resolve(states);
            return resolved is null
                ? lowerPriority.Resolve(states)
                : resolved;
        });
    }

    internal Color? ResolveForegroundColor(MaterialState states)
    {
        return ForegroundColor?.Resolve(states);
    }

    internal Color? ResolveBackgroundColor(MaterialState states)
    {
        return BackgroundColor?.Resolve(states);
    }

    internal Color? ResolveOverlayColor(MaterialState states)
    {
        return OverlayColor?.Resolve(states);
    }

    internal Color? ResolveSplashColor(MaterialState states)
    {
        return SplashColor?.Resolve(states);
    }

    internal BorderSide? ResolveSide(MaterialState states)
    {
        return Side?.Resolve(states);
    }

    internal Thickness? ResolvePadding(MaterialState states)
    {
        return Padding?.Resolve(states);
    }

    internal BorderRadius? ResolveShape(MaterialState states)
    {
        return Shape?.Resolve(states);
    }

    internal Size? ResolveMinimumSize(MaterialState states)
    {
        return MinimumSize?.Resolve(states);
    }
}
