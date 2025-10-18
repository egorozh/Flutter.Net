using System;
using System.Collections.Generic;
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

        double availableMain = Direction == Axis.Horizontal ? innerWidth : innerHeight;
        double availableCross = Direction == Axis.Horizontal ? innerHeight : innerWidth;

        bool hasBoundedMain = !double.IsInfinity(availableMain);
        double crossLimit = double.IsInfinity(availableCross) ? double.PositiveInfinity : Math.Max(0, availableCross);
        double nonFlexMainLimit = hasBoundedMain ? Math.Max(0, availableMain) : double.PositiveInfinity;

        double allocatedNonFlex = 0;
        double maxCross = 0;
        int totalFlex = 0;
        var flexChildren = new List<Control>();

        foreach (var child in Children)
        {
            int flex = GetFlex(child);
            if (flex > 0)
            {
                totalFlex += flex;
                flexChildren.Add(child);
                continue;
            }

            var constraint = CreateChildConstraints(nonFlexMainLimit, crossLimit);
            child.Measure(constraint);
            var size = child.DesiredSize;
            allocatedNonFlex += GetMain(size);
            maxCross = Math.Max(maxCross, GetCross(size));
        }

        int childCount = Children.Count;
        int gapCount = Math.Max(0, childCount - 1);
        double spacingTotal = Spacing * gapCount;

        double allocatedMain = allocatedNonFlex + spacingTotal;

        if (totalFlex > 0)
        {
            if (hasBoundedMain)
            {
                double flexSpace = Math.Max(0, availableMain - allocatedMain);
                double distributed = 0;
                for (int i = 0; i < flexChildren.Count; i++)
                {
                    var child = flexChildren[i];
                    int flex = GetFlex(child);
                    double share = flexSpace * flex / totalFlex;
                    if (i == flexChildren.Count - 1)
                    {
                        share = flexSpace - distributed;
                    }
                    share = Math.Max(0, share);
                    distributed += share;

                    var constraint = CreateChildConstraints(share, crossLimit);
                    child.Measure(constraint);
                    maxCross = Math.Max(maxCross, GetCross(child.DesiredSize));
                }

                allocatedMain += flexSpace;
            }
            else
            {
                foreach (var child in flexChildren)
                {
                    var constraint = CreateChildConstraints(double.PositiveInfinity, crossLimit);
                    child.Measure(constraint);
                    var size = child.DesiredSize;
                    allocatedMain += GetMain(size);
                    maxCross = Math.Max(maxCross, GetCross(size));
                }
            }
        }

        double desiredMain = hasBoundedMain ? Math.Min(availableMain, allocatedMain) : allocatedMain;
        double desiredCross = double.IsInfinity(availableCross) ? maxCross : Math.Min(maxCross, availableCross);

        double width = Direction == Axis.Horizontal ? desiredMain : desiredCross;
        double height = Direction == Axis.Horizontal ? desiredCross : desiredMain;

        width = AddPadding(width, pad.Left, pad.Right);
        height = AddPadding(height, pad.Top, pad.Bottom);

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var pad = Padding ?? new Thickness(0);
        var inner = new Rect(finalSize).Deflate(pad);

        double mainExtent = Direction == Axis.Horizontal ? inner.Width : inner.Height;
        double crossExtent = Direction == Axis.Horizontal ? inner.Height : inner.Width;

        bool hasBoundedMain = !double.IsInfinity(mainExtent);
        bool hasBoundedCross = !double.IsInfinity(crossExtent);

        int childCount = Children.Count;
        int gapCount = Math.Max(0, childCount - 1);
        double baseSpacing = Spacing;

        var mainSizes = new double[childCount];
        double allocatedChildren = 0;
        int totalFlex = 0;
        var flexIndices = new List<int>();

        for (int i = 0; i < childCount; i++)
        {
            var child = Children[i];
            int flex = GetFlex(child);
            double desiredMain = GetMain(child.DesiredSize);

            if (flex > 0 && hasBoundedMain)
            {
                totalFlex += flex;
                flexIndices.Add(i);
            }
            else
            {
                mainSizes[i] = desiredMain;
                allocatedChildren += desiredMain;
            }
        }

        if (!hasBoundedMain)
        {
            foreach (int index in flexIndices)
            {
                var child = Children[index];
                double desiredMain = GetMain(child.DesiredSize);
                mainSizes[index] = desiredMain;
                allocatedChildren += desiredMain;
            }
            totalFlex = 0;
        }

        double totalBaseSpacing = baseSpacing * gapCount;
        double availableForFlex = hasBoundedMain ? mainExtent - totalBaseSpacing - allocatedChildren : 0;
        if (hasBoundedMain && totalFlex > 0)
        {
            double flexSpace = Math.Max(0, availableForFlex);
            double distributed = 0;
            for (int i = 0; i < flexIndices.Count; i++)
            {
                int index = flexIndices[i];
                int flex = GetFlex(Children[index]);
                double share = flexSpace * flex / totalFlex;
                if (i == flexIndices.Count - 1)
                {
                    share = flexSpace - distributed;
                }
                share = Math.Max(0, share);
                distributed += share;
                mainSizes[index] = share;
            }

            allocatedChildren += distributed;
        }
        else if (totalFlex > 0)
        {
            foreach (int index in flexIndices)
            {
                var child = Children[index];
                double desiredMain = GetMain(child.DesiredSize);
                mainSizes[index] = desiredMain;
                allocatedChildren += desiredMain;
            }
        }

        double usedMainWithBase = allocatedChildren + totalBaseSpacing;
        double remainingSpace = Math.Max(0, mainExtent - usedMainWithBase);

        double betweenSpace = baseSpacing;
        double leadingSpace = 0;

        switch (MainAxisAlignment)
        {
            case MainAxisAlignment.Start:
                leadingSpace = 0;
                break;
            case MainAxisAlignment.Center:
                leadingSpace = remainingSpace / 2;
                break;
            case MainAxisAlignment.End:
                leadingSpace = remainingSpace;
                break;
            case MainAxisAlignment.SpaceBetween:
                betweenSpace = baseSpacing + (gapCount > 0 ? remainingSpace / gapCount : 0);
                break;
            case MainAxisAlignment.SpaceAround:
                if (gapCount > 0)
                {
                    double add = remainingSpace / gapCount;
                    betweenSpace = baseSpacing + add;
                    leadingSpace = add / 2;
                }
                else
                {
                    leadingSpace = remainingSpace / 2;
                }
                break;
            case MainAxisAlignment.SpaceEvenly:
                if (gapCount > 0)
                {
                    double add = remainingSpace / (gapCount + 1);
                    betweenSpace = baseSpacing + add;
                    leadingSpace = add;
                }
                else
                {
                    leadingSpace = remainingSpace / (childCount > 0 ? 2 : 1);
                }
                break;
        }

        double cursor = leadingSpace;
        for (int i = 0; i < childCount; i++)
        {
            var child = Children[i];
            double main = Math.Max(0, mainSizes[i]);
            double crossDesired = GetCross(child.DesiredSize);
            double crossSize = crossDesired;
            double crossOffset = 0;

            switch (CrossAxisAlignment)
            {
                case CrossAxisAlignment.Start:
                    crossOffset = 0;
                    break;
                case CrossAxisAlignment.Center:
                    if (hasBoundedCross)
                    {
                        crossOffset = Math.Max(0, (crossExtent - crossSize) / 2);
                    }
                    break;
                case CrossAxisAlignment.End:
                    if (hasBoundedCross)
                    {
                        crossOffset = Math.Max(0, crossExtent - crossSize);
                    }
                    break;
                case CrossAxisAlignment.Stretch:
                    if (hasBoundedCross)
                    {
                        crossSize = crossExtent;
                        crossOffset = 0;
                    }
                    break;
            }

            double width = Direction == Axis.Horizontal ? main : crossSize;
            double height = Direction == Axis.Horizontal ? crossSize : main;
            double x = Direction == Axis.Horizontal ? inner.X + cursor : inner.X + crossOffset;
            double y = Direction == Axis.Horizontal ? inner.Y + crossOffset : inner.Y + cursor;

            child.Arrange(new Rect(x, y, width, height));

            cursor += main;
            if (i < childCount - 1)
            {
                cursor += betweenSpace;
            }
        }

        return finalSize;
    }

    private Size CreateChildConstraints(double main, double cross)
    {
        double mainConstraint = double.IsInfinity(main) ? double.PositiveInfinity : Math.Max(0, main);
        double crossConstraint = double.IsInfinity(cross) ? double.PositiveInfinity : Math.Max(0, cross);

        return Direction == Axis.Horizontal
            ? new Size(mainConstraint, crossConstraint)
            : new Size(crossConstraint, mainConstraint);
    }

    private double GetMain(Size size) => Direction == Axis.Horizontal ? size.Width : size.Height;

    private double GetCross(Size size) => Direction == Axis.Horizontal ? size.Height : size.Width;

    private static double AddPadding(double value, double start, double end)
    {
        if (double.IsInfinity(value))
            return double.PositiveInfinity;
        return value + Math.Max(0, start) + Math.Max(0, end);
    }
}
