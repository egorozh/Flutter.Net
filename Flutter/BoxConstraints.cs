using Avalonia;

namespace Flutter;

public readonly struct BoxConstraints
{
    public readonly double MinWidth, MaxWidth, MinHeight, MaxHeight;

    public BoxConstraints(double minWidth, double maxWidth, double minHeight, double maxHeight)
    {
        MinWidth = minWidth;
        MaxWidth = maxWidth;
        MinHeight = minHeight;
        MaxHeight = maxHeight;
    }

    public Size Constrain(Size size)
    {
        double w = Math.Clamp(size.Width, MinWidth, MaxWidth);
        double h = Math.Clamp(size.Height, MinHeight, MaxHeight);
        return new Size(w, h);
    }

    public static BoxConstraints Tight(Size s) => new BoxConstraints(s.Width, s.Width, s.Height, s.Height);
    
    public static BoxConstraints Loose(Size s) => new BoxConstraints(0, s.Width, 0, s.Height);

    public BoxConstraints Tighten(double? width = null, double? height = null) =>
        new BoxConstraints(width ?? MinWidth, width ?? MaxWidth, height ?? MinHeight, height ?? MaxHeight);
}