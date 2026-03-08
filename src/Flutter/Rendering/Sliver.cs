using Avalonia;
using Flutter.Widgets;

namespace Flutter.Rendering;

public readonly record struct SliverConstraints(
    Axis Axis,
    double ScrollOffset,
    double RemainingPaintExtent,
    double CrossAxisExtent,
    double ViewportMainAxisExtent);

public readonly record struct SliverGeometry(
    double ScrollExtent,
    double PaintExtent,
    double LayoutExtent,
    double MaxPaintExtent,
    bool HasVisualOverflow = false);

public sealed class SliverPhysicalParentData : ContainerBoxParentData<RenderSliver>
{
}

public abstract class RenderSliver : RenderBox
{
    private SliverConstraints? _sliverConstraints;

    public SliverConstraints ConstraintsForSliver =>
        _sliverConstraints ?? throw new InvalidOperationException("RenderSliver is not laid out.");

    public SliverGeometry Geometry { get; protected set; }

    public void LayoutWithSliverConstraints(SliverConstraints constraints)
    {
        _sliverConstraints = constraints;

        BoxConstraints layoutConstraints;
        if (constraints.Axis == Axis.Vertical)
        {
            layoutConstraints = new BoxConstraints(
                MinWidth: constraints.CrossAxisExtent,
                MaxWidth: constraints.CrossAxisExtent,
                MinHeight: 0,
                MaxHeight: constraints.ViewportMainAxisExtent);
        }
        else
        {
            layoutConstraints = new BoxConstraints(
                MinWidth: 0,
                MaxWidth: constraints.ViewportMainAxisExtent,
                MinHeight: constraints.CrossAxisExtent,
                MaxHeight: constraints.CrossAxisExtent);
        }

        Layout(layoutConstraints);
    }

    protected override void PerformLayout()
    {
        var constraints = ConstraintsForSliver;
        PerformSliverLayout(constraints);

        var mainExtent = Math.Max(0, Geometry.PaintExtent);
        Size = constraints.Axis == Axis.Vertical
            ? new Size(constraints.CrossAxisExtent, mainExtent)
            : new Size(mainExtent, constraints.CrossAxisExtent);
    }

    protected abstract void PerformSliverLayout(SliverConstraints constraints);
}

public abstract class RenderSliverSingleBoxAdapter : RenderSliver, IRenderObjectSingleChildContainer
{
    private RenderBox? _child;

    public RenderBox? Child
    {
        get => _child;
        set
        {
            if (ReferenceEquals(_child, value))
            {
                return;
            }

            if (_child != null)
            {
                DropChild(_child);
            }

            _child = value;
            if (_child != null)
            {
                AdoptChild(_child);
            }

            MarkNeedsLayout();
        }
    }

    RenderObject? IRenderObjectSingleChildContainer.Child
    {
        get => Child;
        set => Child = (RenderBox?)value;
    }

    public override void SetupParentData(RenderObject child)
    {
        if (child.parentData is not BoxParentData)
        {
            child.parentData = new BoxParentData();
        }
    }

    public override void VisitChildren(Action<RenderObject> visitor)
    {
        if (_child != null)
        {
            visitor(_child);
        }
    }

    protected static double ChildExtentForAxis(Size size, Axis axis)
    {
        return axis == Axis.Vertical ? size.Height : size.Width;
    }

    public override void Paint(PaintingContext ctx, Point offset)
    {
        if (Child == null || Geometry.PaintExtent <= 0)
        {
            return;
        }

        var childParentData = (BoxParentData)Child.parentData!;
        ctx.PaintChild(Child, offset + childParentData.offset);
    }

    protected override bool HitTestSelf(Point position)
    {
        return false;
    }

    protected override bool HitTestChildren(BoxHitTestResult result, Point position)
    {
        if (Child == null || Geometry.PaintExtent <= 0)
        {
            return false;
        }

        var childParentData = (BoxParentData)Child.parentData!;
        return Child.HitTest(result, position - childParentData.offset);
    }

    internal override void VisitChildrenForSemantics(Action<RenderObject, Point, Matrix> visitor)
    {
        if (Child == null)
        {
            return;
        }

        var childParentData = (BoxParentData)Child.parentData!;
        visitor(Child, childParentData.offset, Matrix.Identity);
    }
}

public class RenderSliverToBoxAdapter : RenderSliverSingleBoxAdapter
{
    public RenderSliverToBoxAdapter(RenderBox? child = null)
    {
        Child = child;
    }

    protected override void PerformSliverLayout(SliverConstraints constraints)
    {
        if (Child == null)
        {
            Geometry = default;
            return;
        }

        BoxConstraints childConstraints;
        if (constraints.Axis == Axis.Vertical)
        {
            childConstraints = new BoxConstraints(
                MinWidth: constraints.CrossAxisExtent,
                MaxWidth: constraints.CrossAxisExtent,
                MinHeight: 0,
                MaxHeight: double.PositiveInfinity);
        }
        else
        {
            childConstraints = new BoxConstraints(
                MinWidth: 0,
                MaxWidth: double.PositiveInfinity,
                MinHeight: constraints.CrossAxisExtent,
                MaxHeight: constraints.CrossAxisExtent);
        }

        Child.Layout(childConstraints, parentUsesSize: true);

        var childExtent = ChildExtentForAxis(Child.Size, constraints.Axis);
        var effectiveScrollOffset = Math.Clamp(constraints.ScrollOffset, 0, childExtent);
        var remaining = Math.Max(0, childExtent - effectiveScrollOffset);

        var paintedExtent = Math.Min(remaining, constraints.RemainingPaintExtent);
        var layoutExtent = Math.Min(remaining, constraints.ViewportMainAxisExtent);

        var childParentData = (BoxParentData)Child.parentData!;
        childParentData.offset = constraints.Axis == Axis.Vertical
            ? new Point(0, -effectiveScrollOffset)
            : new Point(-effectiveScrollOffset, 0);

        Geometry = new SliverGeometry(
            ScrollExtent: childExtent,
            PaintExtent: paintedExtent,
            LayoutExtent: layoutExtent,
            MaxPaintExtent: childExtent,
            HasVisualOverflow: remaining > constraints.RemainingPaintExtent);
    }
}

public sealed class RenderSliverList : RenderSliverToBoxAdapter
{
    public RenderSliverList(RenderBox? child = null) : base(child)
    {
    }
}
