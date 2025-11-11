using Avalonia;

namespace Flutter.Rendering;

/// <summary>
/// Immutable layout constraints for [RenderBox] layout.
/// </summary>
/// <param name="MinWidth">The minimum width that satisfies the constraints.</param>
/// <param name="MaxWidth">The maximum width that satisfies the constraints. Might be [double.PositiveInfinity].</param>
/// <param name="MinHeight">The minimum height that satisfies the constraints.</param>
/// <param name="MaxHeight">The maximum height that satisfies the constraints. Might be [double.PositiveInfinity].</param>
public readonly record struct BoxConstraints(
    double MinWidth = 0.0,
    double MaxWidth = double.PositiveInfinity,
    double MinHeight = 0.0,
    double MaxHeight = double.PositiveInfinity)
    : IConstraints
{
    /// Whether there is exactly one width value that satisfies the constraints.
    public bool HasTightWidth => MinWidth >= MaxWidth;

    /// Whether there is exactly one height value that satisfies the constraints.
    public bool HasTightHeight => MinHeight >= MaxHeight;

    public bool IsTight => HasTightWidth && HasTightHeight;

    public bool IsNormalized => MinWidth >= 0.0 && MinWidth <= MaxWidth && MinHeight >= 0.0 && MinHeight <= MaxHeight;


    public Size Constrain(Size size)
    {
        double w = Math.Clamp(size.Width, MinWidth, MaxWidth);
        double h = Math.Clamp(size.Height, MinHeight, MaxHeight);
        return new Size(w, h);
    }

    /// <summary>
    /// Creates box constraints that is respected only by the given size.
    /// </summary>
    public static BoxConstraints Tight(Size s) => new BoxConstraints(s.Width, s.Width, s.Height, s.Height);

    public static BoxConstraints Loose(Size s) => new BoxConstraints(0, s.Width, 0, s.Height);

    public BoxConstraints Tighten(double? width = null, double? height = null) =>
        new BoxConstraints(width ?? MinWidth, width ?? MaxWidth, height ?? MinHeight, height ?? MaxHeight);
}