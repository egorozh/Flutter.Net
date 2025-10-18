using System;
using System.Linq;
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
        double innerWidth = double.IsInfinity(inner.Width) ? inner.Width : Math.Max(0, inner.Width);
        double innerHeight = double.IsInfinity(inner.Height) ? inner.Height : Math.Max(0, inner.Height);

        double maxMain = Direction == Axis.Horizontal ? innerWidth : innerHeight;
        bool canFlex = !double.IsInfinity(maxMain);

        double allocatedMain = 0;
        double crossMax = 0;
        int totalFlex = 0;
        int childCount = Children.Count;
        int gapCount = Math.Max(0, childCount - 1);
        double spacingTotal = gapCount > 0 ? Spacing * gapCount : 0;

        foreach (var c in Children)
        {
            int flex = GetFlex(c);
            if (flex > 0 && canFlex)
            {
                totalFlex += flex;
                continue;
            }

            c.Measure(inner);
            var s = c.DesiredSize;
            double main = Direction == Axis.Horizontal ? s.Width : s.Height;
            double cross = Direction == Axis.Horizontal ? s.Height : s.Width;
            allocatedMain += main;
            crossMax = Math.Max(crossMax, cross);
        }

        allocatedMain += spacingTotal;

        if (totalFlex > 0 && canFlex)
        {
            double freeSpace = Math.Max(0, maxMain - allocatedMain);
            foreach (var c in Children)
            {
                int flex = GetFlex(c);
                if (flex <= 0) continue;

                double share = totalFlex == 0 ? 0 : freeSpace * flex / totalFlex;
                Size constraint = Direction == Axis.Horizontal
                    ? new Size(share, innerHeight)
                    : new Size(innerWidth, share);
                c.Measure(constraint);

                var s = c.DesiredSize;
                double cross = Direction == Axis.Horizontal ? s.Height : s.Width;
                crossMax = Math.Max(crossMax, cross);
                allocatedMain += share;
            }
        }

        double usedMain = allocatedMain;
        double usedCross = crossMax;

        var constrained = Direction == Axis.Horizontal
            ? new Size(Math.Min(innerWidth, usedMain + pad.Left + pad.Right), usedCross + pad.Top + pad.Bottom)
            : new Size(usedCross + pad.Left + pad.Right, Math.Min(innerHeight, usedMain + pad.Top + pad.Bottom));

        var used = Direction == Axis.Horizontal
            ? new Size(usedMain + pad.Left + pad.Right, usedCross + pad.Top + pad.Bottom)
            : new Size(usedCross + pad.Left + pad.Right, usedMain + pad.Top + pad.Bottom);

        return new Size(Math.Max(constrained.Width, used.Width), Math.Max(constrained.Height, used.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var pad = Padding ?? new Thickness(0);
        Rect inner = new Rect(finalSize).Deflate(pad);
        double mainExtent = Direction == Axis.Horizontal ? inner.Width : inner.Height;
        double crossExtent = Direction == Axis.Horizontal ? inner.Height : inner.Width;
        bool canFlex = !double.IsInfinity(mainExtent);

        // Compute sizes
        double[] sizes = new double[Children.Count];
        int totalFlex = 0;
        double fixedSum = 0;
        for (int i = 0; i < Children.Count; i++)
        {
            var c = Children[i];
            int flex = GetFlex(c);
            double desiredMain = Direction == Axis.Horizontal ? c.DesiredSize.Width : c.DesiredSize.Height;
            if (flex > 0 && canFlex)
            {
                totalFlex += flex;
            }
            else
            {
                sizes[i] = desiredMain;
                fixedSum += desiredMain;
            }
        }

        if (Spacing > 0 && Children.Count > 1) fixedSum += Spacing * (Children.Count - 1);
        double free = canFlex ? Math.Max(0, mainExtent - fixedSum) : 0;
        for (int i = 0; i < Children.Count; i++)
        {
            var c = Children[i];
            int flex = GetFlex(c);
            if (flex > 0)
            {
                if (canFlex && totalFlex > 0)
                    sizes[i] = free * flex / totalFlex;
                else
                    sizes[i] = Direction == Axis.Horizontal ? c.DesiredSize.Width : c.DesiredSize.Height;
            }
        }

        // Main-axis positioning
        int childCount = Children.Count;
        int gapCount = Math.Max(0, childCount - 1);
        double baseSpacing = Spacing;
        double betweenSpace = baseSpacing;
        double usedMainWithoutSpacing = sizes.Sum();
        double usedWithSpacing = usedMainWithoutSpacing + betweenSpace * gapCount;
        double leadingSpace = 0;

        switch (MainAxisAlignment)
        {
            case MainAxisAlignment.Start:
                leadingSpace = 0;
                break;
            case MainAxisAlignment.Center:
                leadingSpace = Math.Max(0, (mainExtent - usedWithSpacing) / 2);
                break;
            case MainAxisAlignment.End:
                leadingSpace = Math.Max(0, mainExtent - usedWithSpacing);
                break;
            case MainAxisAlignment.SpaceBetween:
                betweenSpace = gapCount > 0
                    ? Math.Max(0, (mainExtent - usedMainWithoutSpacing) / gapCount)
                    : 0;
                usedWithSpacing = usedMainWithoutSpacing + betweenSpace * gapCount;
                leadingSpace = 0;
                break;
            case MainAxisAlignment.SpaceAround:
                betweenSpace = childCount > 0
                    ? Math.Max(0, (mainExtent - usedMainWithoutSpacing) / childCount)
                    : 0;
                usedWithSpacing = usedMainWithoutSpacing + betweenSpace * gapCount;
                leadingSpace = betweenSpace / 2;
                break;
            case MainAxisAlignment.SpaceEvenly:
                betweenSpace = childCount > 0
                    ? Math.Max(0, (mainExtent - usedMainWithoutSpacing) / (childCount + 1))
                    : 0;
                usedWithSpacing = usedMainWithoutSpacing + betweenSpace * gapCount;
                leadingSpace = betweenSpace;
                break;
        }

        double cursor = leadingSpace;
        for (int i = 0; i < Children.Count; i++)
        {
            var c = Children[i];
            double main = sizes[i];
            double crossDesired = Direction == Axis.Horizontal ? c.DesiredSize.Height : c.DesiredSize.Width;
            double crossSize = crossDesired;
            double crossOffset = 0;
            bool finiteCross = !double.IsInfinity(crossExtent);

            switch (CrossAxisAlignment)
            {
                case CrossAxisAlignment.Start:
                    crossSize = crossDesired;
                    crossOffset = 0;
                    break;
                case CrossAxisAlignment.Center:
                    crossSize = crossDesired;
                    crossOffset = finiteCross ? Math.Max(0, (crossExtent - crossSize) / 2) : 0;
                    break;
                case CrossAxisAlignment.End:
                    crossSize = crossDesired;
                    crossOffset = finiteCross ? Math.Max(0, crossExtent - crossSize) : 0;
                    break;
                case CrossAxisAlignment.Stretch:
                    crossSize = finiteCross ? crossExtent : crossDesired;
                    crossOffset = 0;
                    break;
            }

            double w, h, x, y;
            if (Direction == Axis.Horizontal)
            {
                w = main;
                h = crossSize;
                x = inner.X + cursor;
                y = inner.Y + crossOffset;
            }
            else
            {
                w = crossSize;
                h = main;
                x = inner.X + crossOffset;
                y = inner.Y + cursor;
            }

            var rect = new Rect(x, y, w, h);
            c.Arrange(rect);
            cursor += sizes[i] + (i < Children.Count - 1 ? betweenSpace : 0);
        }

        return finalSize;
    }
}