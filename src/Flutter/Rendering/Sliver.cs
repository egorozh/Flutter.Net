using Avalonia;
using Avalonia.Media;
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

public interface IRenderSliverBoxChildManager
{
    int? ChildCount { get; }
    bool CreateChild(int index, RenderBox? after);
    void RemoveChild(RenderBox child);
    void DidAdoptChild(RenderBox child);
    void SetDidUnderflow(bool value);
}

public sealed class SliverPhysicalParentData : ContainerBoxParentData<RenderSliver>
{
}

public sealed class SliverMultiBoxAdaptorParentData : ContainerBoxParentData<RenderBox>
{
    public int Index { get; set; }
    public double LayoutOffset { get; set; }
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
        var scrollAwareMainAxisExtent = constraints.ViewportMainAxisExtent + Math.Max(0, constraints.ScrollOffset);

        BoxConstraints layoutConstraints;
        if (constraints.Axis == Axis.Vertical)
        {
            layoutConstraints = new BoxConstraints(
                MinWidth: constraints.CrossAxisExtent,
                MaxWidth: constraints.CrossAxisExtent,
                MinHeight: 0,
                MaxHeight: scrollAwareMainAxisExtent);
        }
        else
        {
            layoutConstraints = new BoxConstraints(
                MinWidth: 0,
                MaxWidth: scrollAwareMainAxisExtent,
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

public abstract class RenderSliverMultiBoxAdaptor : RenderSliver,
    IRenderBoxContainerDefaultsMixin<RenderBox, SliverMultiBoxAdaptorParentData>,
    IRenderObjectContainer
{
    private readonly RenderBoxContainerDefaultsMixin<RenderBox, SliverMultiBoxAdaptorParentData> _container;
    private IRenderSliverBoxChildManager? _childManager;

    protected RenderSliverMultiBoxAdaptor(IRenderSliverBoxChildManager? childManager = null)
    {
        _container = new RenderBoxContainerDefaultsMixin<RenderBox, SliverMultiBoxAdaptorParentData>(this);
        _childManager = childManager;
    }

    public IRenderSliverBoxChildManager? ChildManager
    {
        get => _childManager;
        set
        {
            if (ReferenceEquals(_childManager, value))
            {
                return;
            }

            _childManager = value;
            MarkNeedsLayout();
        }
    }

    public int ChildCount => _container.ChildCount;

    public RenderBox? FirstChild => _container.FirstChild;

    public RenderBox? LastChild => _container.LastChild;

    public void Insert(RenderBox child, RenderBox? after = null)
    {
        _container.Insert(child, after);
        _childManager?.DidAdoptChild(child);
    }

    public void Move(RenderBox child, RenderBox? after = null)
    {
        _container.Move(child, after);
    }

    public void Remove(RenderBox child)
    {
        _container.Remove(child);
    }

    void IRenderObjectContainer.Insert(RenderObject child, RenderObject? after)
    {
        Insert((RenderBox)child, (RenderBox?)after);
    }

    void IRenderObjectContainer.Move(RenderObject child, RenderObject? after)
    {
        Move((RenderBox)child, (RenderBox?)after);
    }

    void IRenderObjectContainer.Remove(RenderObject child)
    {
        Remove((RenderBox)child);
    }

    public RenderBox? ChildAfter(RenderBox child)
    {
        return _container.ChildAfter(child);
    }

    public RenderBox? ChildBefore(RenderBox child)
    {
        return _container.ChildBefore(child);
    }

    public void AddAll(List<RenderBox> children)
    {
        _container.AddAll(children);
    }

    public override void SetupParentData(RenderObject child)
    {
        if (child.parentData is not SliverMultiBoxAdaptorParentData)
        {
            child.parentData = new SliverMultiBoxAdaptorParentData();
        }
    }

    public override void VisitChildren(Action<RenderObject> visitor)
    {
        for (var child = FirstChild; child != null; child = ChildAfter(child))
        {
            visitor(child);
        }
    }

    public override void Paint(PaintingContext ctx, Point offset)
    {
        _container.DefaultPaint(ctx, offset);
    }

    protected override bool HitTestChildren(BoxHitTestResult result, Point position)
    {
        return _container.DefaultHitTestChildren(result, position);
    }

    internal override void VisitChildrenForSemantics(Action<RenderObject, Point, Matrix> visitor)
    {
        for (var child = FirstChild; child != null; child = ChildAfter(child))
        {
            var childParentData = (SliverMultiBoxAdaptorParentData)child.parentData!;
            visitor(child, childParentData.offset, Matrix.Identity);
        }
    }

    protected BoxConstraints ChildConstraintsForSliver(SliverConstraints constraints)
    {
        if (constraints.Axis == Axis.Vertical)
        {
            return new BoxConstraints(
                MinWidth: constraints.CrossAxisExtent,
                MaxWidth: constraints.CrossAxisExtent,
                MinHeight: 0,
                MaxHeight: double.PositiveInfinity);
        }

        return new BoxConstraints(
            MinWidth: 0,
            MaxWidth: double.PositiveInfinity,
            MinHeight: constraints.CrossAxisExtent,
            MaxHeight: constraints.CrossAxisExtent);
    }

    protected static double ChildMainAxisExtent(RenderBox child, Axis axis)
    {
        return axis == Axis.Vertical ? child.Size.Height : child.Size.Width;
    }

    public void DefaultPaint(PaintingContext ctx, Point offset)
    {
        _container.DefaultPaint(ctx, offset);
    }

    public bool DefaultHitTestChildren(BoxHitTestResult result, Point position)
    {
        return _container.DefaultHitTestChildren(result, position);
    }
}

public sealed class RenderSliverList : RenderSliverMultiBoxAdaptor
{
    public RenderSliverList(IRenderSliverBoxChildManager? childManager = null) : base(childManager)
    {
    }

    protected override void PerformSliverLayout(SliverConstraints constraints)
    {
        var childManager = ChildManager;
        if (childManager == null)
        {
            Geometry = default;
            return;
        }

        childManager.SetDidUnderflow(false);

        if (FirstChild == null)
        {
            if (!childManager.CreateChild(0, after: null) || FirstChild == null)
            {
                Geometry = default;
                childManager.SetDidUnderflow(true);
                return;
            }
        }

        var childConstraints = ChildConstraintsForSliver(constraints);
        var targetEndScrollOffset = constraints.ScrollOffset + constraints.RemainingPaintExtent;

        var index = 0;
        var layoutOffset = 0.0;
        RenderBox? previousChild = null;
        var child = FirstChild;

        while (true)
        {
            if (child == null)
            {
                var count = childManager.ChildCount;
                if (count.HasValue && index >= count.Value)
                {
                    childManager.SetDidUnderflow(true);
                    break;
                }

                if (!childManager.CreateChild(index, previousChild))
                {
                    childManager.SetDidUnderflow(true);
                    break;
                }

                child = previousChild == null ? FirstChild : ChildAfter(previousChild);
                if (child == null)
                {
                    childManager.SetDidUnderflow(true);
                    break;
                }
            }

            child.Layout(childConstraints, parentUsesSize: true);
            var childParentData = (SliverMultiBoxAdaptorParentData)child.parentData!;
            childParentData.Index = index;
            childParentData.LayoutOffset = layoutOffset;
            childParentData.offset = constraints.Axis == Axis.Vertical
                ? new Point(0, layoutOffset - constraints.ScrollOffset)
                : new Point(layoutOffset - constraints.ScrollOffset, 0);

            var childExtent = ChildMainAxisExtent(child, constraints.Axis);
            layoutOffset += childExtent;

            if (layoutOffset >= targetEndScrollOffset)
            {
                var trailingChild = ChildAfter(child);
                while (trailingChild != null)
                {
                    var nextTrailing = ChildAfter(trailingChild);
                    childManager.RemoveChild(trailingChild);
                    trailingChild = nextTrailing;
                }

                break;
            }

            previousChild = child;
            child = ChildAfter(child);
            index += 1;
        }

        var maxScrollExtent = layoutOffset;
        var estimatedChildCount = childManager.ChildCount;
        var laidOutCount = Math.Max(1, index + 1);
        if (estimatedChildCount.HasValue && estimatedChildCount.Value > laidOutCount)
        {
            var averageExtent = layoutOffset / laidOutCount;
            maxScrollExtent += (estimatedChildCount.Value - laidOutCount) * averageExtent;
        }

        var remaining = Math.Max(0, maxScrollExtent - constraints.ScrollOffset);
        var paintExtent = Math.Min(remaining, constraints.RemainingPaintExtent);
        var layoutExtent = Math.Min(remaining, constraints.ViewportMainAxisExtent);

        Geometry = new SliverGeometry(
            ScrollExtent: maxScrollExtent,
            PaintExtent: paintExtent,
            LayoutExtent: layoutExtent,
            MaxPaintExtent: maxScrollExtent,
            HasVisualOverflow: constraints.ScrollOffset > 0 || remaining > constraints.RemainingPaintExtent);
    }
}
