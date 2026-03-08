using Avalonia;

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

public abstract class RenderSliver : RenderObject
{
    private SliverConstraints? _sliverConstraints;

    public SliverConstraints ConstraintsForSliver =>
        _sliverConstraints ?? throw new InvalidOperationException("RenderSliver is not laid out.");

    public SliverGeometry Geometry { get; protected set; }

    public void Layout(SliverConstraints constraints)
    {
        _sliverConstraints = constraints;
        PerformSliverLayout(constraints);
        MarkNeedsPaint();
    }

    protected abstract void PerformSliverLayout(SliverConstraints constraints);

    public override void Paint(PaintingContext ctx, Point offset)
    {
    }
}

public abstract class RenderSliverSingleBoxAdapter : RenderSliver
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
        var paintedExtent = Math.Max(0, Math.Min(childExtent - constraints.ScrollOffset, constraints.RemainingPaintExtent));
        var layoutExtent = Math.Max(0, Math.Min(childExtent - constraints.ScrollOffset, constraints.ViewportMainAxisExtent));

        Geometry = new SliverGeometry(
            ScrollExtent: childExtent,
            PaintExtent: paintedExtent,
            LayoutExtent: layoutExtent,
            MaxPaintExtent: childExtent,
            HasVisualOverflow: childExtent > constraints.RemainingPaintExtent);
    }
}

public sealed class RenderSliverList : RenderSliverToBoxAdapter
{
    public RenderSliverList(RenderBox? child = null) : base(child)
    {
    }
}
