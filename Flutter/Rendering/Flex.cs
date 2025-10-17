using Avalonia;
using Avalonia.Controls;

namespace Flutter.Rendering;

/// How the children should be placed along the main axis in a flex layout.
///
/// See also:
///
///  * [Column], [Row], and [Flex], the flex widgets.
///  * [RenderFlex], the flex render object.
public enum MainAxisAlignment
{
    Start,
    Center,
    End,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

/// How the children should be placed along the cross axis in a flex layout.
///
/// See also:
///
///  * [Column], [Row], and [Flex], the flex widgets.
///  * [Flex.crossAxisAlignment], the property on flex widgets that
///    has this type.
///  * [RenderFlex], the flex render object.
public enum CrossAxisAlignment
{
    Start,
    Center,
    End,
    Stretch
}

public enum Axis
{
    Horizontal,
    Vertical
}

public sealed class FlexPanel : Panel
{
    public static readonly AttachedProperty<int> FlexProperty =
        AvaloniaProperty.RegisterAttached<Control, int>("Flex", typeof(FlexPanel), 0);

    public static int GetFlex(AvaloniaObject obj) => obj.GetValue(FlexProperty);
    public static void SetFlex(AvaloniaObject obj, int value) => obj.SetValue(FlexProperty, value);

    public Axis Direction { get; set; } = Axis.Horizontal;
    public MainAxisAlignment MainAxisAlignment { get; set; } = MainAxisAlignment.Start;
    public CrossAxisAlignment CrossAxisAlignment { get; set; } = CrossAxisAlignment.Center;
    public Thickness? Padding { get; set; }
    public double Spacing { get; set; } = 0;

    protected override Size MeasureOverride(Size availableSize)
    {
        var pad = Padding ?? new Thickness(0);
        var inner = availableSize.Deflate(pad);
        double mainUsed = 0, crossMax = 0;
        int totalFlex = 0;

        foreach (var c in Children)
        {
            int flex = GetFlex(c);
            if (flex > 0)
            {
                totalFlex += flex;
                continue;
            }

            c.Measure(inner);
            var s = c.DesiredSize;
            if (Direction == Axis.Horizontal)
            {
                mainUsed += s.Width;
                crossMax = Math.Max(crossMax, s.Height);
            }
            else
            {
                mainUsed += s.Height;
                crossMax = Math.Max(crossMax, s.Width);
            }
        }

        if (Spacing > 0 && Children.Count > 1)
            mainUsed += Spacing * (Children.Count - 1);

        double mainAvail = Direction == Axis.Horizontal ? inner.Width : inner.Height;
        double free = Math.Max(0, mainAvail - mainUsed);

        // Measure flex children with allocated space proportionally.
        foreach (var c in Children)
        {
            int flex = GetFlex(c);
            if (flex <= 0) continue;
            double share = totalFlex == 0 ? 0 : free * (double)flex / totalFlex;
            Size childConstraint = Direction == Axis.Horizontal
                ? new Size(share, inner.Height)
                : new Size(inner.Width, share);
            c.Measure(childConstraint);
            var s = c.DesiredSize;
            if (Direction == Axis.Horizontal) crossMax = Math.Max(crossMax, s.Height);
            else crossMax = Math.Max(crossMax, s.Width);
        }

        var result = Direction == Axis.Horizontal
            ? new Size(Math.Min(inner.Width, mainUsed + pad.Left + pad.Right), crossMax + pad.Top + pad.Bottom)
            : new Size(crossMax + pad.Left + pad.Right, Math.Min(inner.Height, mainUsed + pad.Top + pad.Bottom));

        // Ensure we report at least the used size with padding
        var used = Direction == Axis.Horizontal
            ? new Size(mainUsed + pad.Left + pad.Right, crossMax + pad.Top + pad.Bottom)
            : new Size(crossMax + pad.Left + pad.Right, mainUsed + pad.Top + pad.Bottom);

        return new Size(Math.Max(result.Width, used.Width), Math.Max(result.Height, used.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var pad = Padding ?? new Thickness(0);
        Rect inner = new Rect(finalSize).Deflate(pad);
        double mainExtent = Direction == Axis.Horizontal ? inner.Width : inner.Height;
        double crossExtent = Direction == Axis.Horizontal ? inner.Height : inner.Width;

        // Compute sizes
        double[] sizes = new double[Children.Count];
        int totalFlex = 0;
        double fixedSum = 0;
        for (int i = 0; i < Children.Count; i++)
        {
            var c = Children[i];
            int flex = GetFlex(c);
            if (flex > 0) totalFlex += flex;
            else fixedSum += Direction == Axis.Horizontal ? c.DesiredSize.Width : c.DesiredSize.Height;
        }

        if (Spacing > 0 && Children.Count > 1) fixedSum += Spacing * (Children.Count - 1);
        double free = Math.Max(0, mainExtent - fixedSum);
        for (int i = 0; i < Children.Count; i++)
        {
            var c = Children[i];
            int flex = GetFlex(c);
            if (flex > 0) sizes[i] = totalFlex == 0 ? 0 : free * (double)flex / totalFlex;
            else sizes[i] = Direction == Axis.Horizontal ? c.DesiredSize.Width : c.DesiredSize.Height;
        }

        // Main-axis positioning
        double start = inner.X;
        double ystart = inner.Y;
        double offset = 0;
        double used = sizes.Sum() + (Spacing > 0 && Children.Count > 1 ? Spacing * (Children.Count - 1) : 0);
        switch (MainAxisAlignment)
        {
            case MainAxisAlignment.Start: offset = 0; break;
            case MainAxisAlignment.Center: offset = (mainExtent - used) / 2; break;
            case MainAxisAlignment.End: offset = mainExtent - used; break;
            case MainAxisAlignment.SpaceBetween:
                offset = 0;
                if (Children.Count > 1) Spacing = (mainExtent - sizes.Sum()) / (Children.Count - 1);
                break;
            case MainAxisAlignment.SpaceAround: offset = (mainExtent - used) / (Children.Count * 2); break;
            case MainAxisAlignment.SpaceEvenly: offset = (mainExtent - used) / (Children.Count + 1); break;
        }

        double cursor = offset;
        for (int i = 0; i < Children.Count; i++)
        {
            var c = Children[i];
            double w = Direction == Axis.Horizontal ? sizes[i] : crossExtent;
            double h = Direction == Axis.Horizontal ? crossExtent : sizes[i];
            double x = Direction == Axis.Horizontal ? inner.X + cursor : inner.X;
            double y = Direction == Axis.Horizontal ? inner.Y : inner.Y + cursor;

            // Cross-axis alignment (approximation)
            switch (CrossAxisAlignment)
            {
                case CrossAxisAlignment.Start:
                    if (Direction == Axis.Horizontal) h = c.DesiredSize.Height;
                    else w = c.DesiredSize.Width;
                    break;
                case CrossAxisAlignment.Center:
                    // leave w/h as computed, center below
                    break;
                case CrossAxisAlignment.End:
                    if (Direction == Axis.Horizontal)
                    {
                        y += crossExtent - c.DesiredSize.Height;
                        h = c.DesiredSize.Height;
                    }
                    else
                    {
                        x += crossExtent - c.DesiredSize.Width;
                        w = c.DesiredSize.Width;
                    }

                    break;
                case CrossAxisAlignment.Stretch:
                    // already stretched by using crossExtent
                    break;
            }

            var rect = new Rect(x, y, w, h);
            c.Arrange(rect);
            cursor += sizes[i] + (i < Children.Count - 1 ? Spacing : 0);
        }

        return finalSize;
    }
}