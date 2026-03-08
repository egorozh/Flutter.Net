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
    public bool KeepAlive { get; set; }
    public bool KeptAlive { get; set; }
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
    private readonly Dictionary<int, RenderBox> _keepAliveBucket = [];
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
        SetupParentData(child);
        var childParentData = (SliverMultiBoxAdaptorParentData)child.parentData!;
        childParentData.KeptAlive = false;
        _container.Insert(child, after);
        _childManager?.DidAdoptChild(child);
    }

    public void Move(RenderBox child, RenderBox? after = null)
    {
        var childParentData = (SliverMultiBoxAdaptorParentData)child.parentData!;
        if (!childParentData.KeptAlive)
        {
            _container.Move(child, after);
            _childManager?.DidAdoptChild(child);
            MarkNeedsLayout();
            return;
        }

        if (_keepAliveBucket.TryGetValue(childParentData.Index, out var cachedChild) && ReferenceEquals(cachedChild, child))
        {
            _keepAliveBucket.Remove(childParentData.Index);
        }

        _childManager?.DidAdoptChild(child);
        _keepAliveBucket[childParentData.Index] = child;
        MarkNeedsLayout();
    }

    public void Remove(RenderBox child)
    {
        var childParentData = (SliverMultiBoxAdaptorParentData)child.parentData!;
        if (childParentData.KeptAlive)
        {
            if (_keepAliveBucket.TryGetValue(childParentData.Index, out var cachedChild) && ReferenceEquals(cachedChild, child))
            {
                _keepAliveBucket.Remove(childParentData.Index);
            }

            DropChild(child);
            childParentData.KeptAlive = false;
            return;
        }

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

        foreach (var child in _keepAliveBucket.Values)
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

    protected int IndexOf(RenderBox child)
    {
        return ((SliverMultiBoxAdaptorParentData)child.parentData!).Index;
    }

    protected double ChildScrollOffset(RenderBox child)
    {
        return ((SliverMultiBoxAdaptorParentData)child.parentData!).LayoutOffset;
    }

    protected bool AddInitialChild(int index = 0, double layoutOffset = 0)
    {
        if (FirstChild != null)
        {
            return true;
        }

        if (!CreateOrObtainChild(index, after: null) || FirstChild == null)
        {
            _childManager?.SetDidUnderflow(true);
            return false;
        }

        var firstChildParentData = (SliverMultiBoxAdaptorParentData)FirstChild.parentData!;
        firstChildParentData.LayoutOffset = layoutOffset;
        return true;
    }

    protected RenderBox? InsertAndLayoutLeadingChild(BoxConstraints childConstraints)
    {
        if (FirstChild == null)
        {
            return null;
        }

        var index = IndexOf(FirstChild) - 1;
        if (index < 0)
        {
            _childManager?.SetDidUnderflow(true);
            return null;
        }

        if (!CreateOrObtainChild(index, after: null) || FirstChild == null || IndexOf(FirstChild) != index)
        {
            _childManager?.SetDidUnderflow(true);
            return null;
        }

        FirstChild.Layout(childConstraints, parentUsesSize: true);
        return FirstChild;
    }

    protected RenderBox? InsertAndLayoutChild(BoxConstraints childConstraints, RenderBox after)
    {
        var index = IndexOf(after) + 1;
        if (!CreateOrObtainChild(index, after))
        {
            _childManager?.SetDidUnderflow(true);
            return null;
        }

        var child = ChildAfter(after);
        if (child == null || IndexOf(child) != index)
        {
            _childManager?.SetDidUnderflow(true);
            return null;
        }

        child.Layout(childConstraints, parentUsesSize: true);
        return child;
    }

    protected void CollectGarbage(int leadingGarbage, int trailingGarbage)
    {
        while (leadingGarbage > 0 && FirstChild != null)
        {
            DestroyOrCacheChild(FirstChild);
            leadingGarbage -= 1;
        }

        while (trailingGarbage > 0 && LastChild != null)
        {
            DestroyOrCacheChild(LastChild);
            trailingGarbage -= 1;
        }

        if (_childManager == null || _keepAliveBucket.Count == 0)
        {
            return;
        }

        foreach (var keepAliveChild in _keepAliveBucket.Values
                     .Where(child => !((SliverMultiBoxAdaptorParentData)child.parentData!).KeepAlive)
                     .ToArray())
        {
            _childManager.RemoveChild(keepAliveChild);
        }
    }

    private bool CreateOrObtainChild(int index, RenderBox? after)
    {
        if (index < 0)
        {
            return false;
        }

        if (_keepAliveBucket.TryGetValue(index, out var keptAliveChild))
        {
            _keepAliveBucket.Remove(index);
            var parentData = (SliverMultiBoxAdaptorParentData)keptAliveChild.parentData!;
            parentData.KeptAlive = false;
            Insert(keptAliveChild, after);
            return true;
        }

        return _childManager?.CreateChild(index, after) ?? false;
    }

    private void DestroyOrCacheChild(RenderBox child)
    {
        var childParentData = (SliverMultiBoxAdaptorParentData)child.parentData!;
        if (childParentData.KeepAlive)
        {
            Remove(child);
            _keepAliveBucket[childParentData.Index] = child;
            AdoptChild(child);
            childParentData.KeptAlive = true;
            return;
        }

        _childManager?.RemoveChild(child);
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

        if (FirstChild == null && !AddInitialChild())
        {
            Geometry = default;
            return;
        }

        var firstChild = FirstChild;
        if (firstChild == null)
        {
            Geometry = default;
            childManager.SetDidUnderflow(true);
            return;
        }

        var childConstraints = ChildConstraintsForSliver(constraints);
        var scrollOffset = Math.Max(0, constraints.ScrollOffset);
        var targetEndScrollOffset = scrollOffset + constraints.RemainingPaintExtent;

        var earliestUsefulChild = firstChild;
        while (ChildScrollOffset(earliestUsefulChild) > scrollOffset)
        {
            var oldFirstChild = earliestUsefulChild;
            var oldFirstOffset = ChildScrollOffset(oldFirstChild);

            var newLeadingChild = InsertAndLayoutLeadingChild(childConstraints);
            if (newLeadingChild == null)
            {
                break;
            }

            var newLeadingParentData = (SliverMultiBoxAdaptorParentData)newLeadingChild.parentData!;
            newLeadingParentData.LayoutOffset = oldFirstOffset - ChildMainAxisExtent(newLeadingChild, constraints.Axis);
            earliestUsefulChild = newLeadingChild;
        }

        earliestUsefulChild = FirstChild ?? earliestUsefulChild;
        earliestUsefulChild.Layout(childConstraints, parentUsesSize: true);
        var earliestUsefulParentData = (SliverMultiBoxAdaptorParentData)earliestUsefulChild.parentData!;
        earliestUsefulParentData.offset = constraints.Axis == Axis.Vertical
            ? new Point(0, earliestUsefulParentData.LayoutOffset - scrollOffset)
            : new Point(earliestUsefulParentData.LayoutOffset - scrollOffset, 0);

        var leadingGarbage = 0;
        var trailingGarbage = 0;
        var reachedEnd = false;

        RenderBox? child = earliestUsefulChild;
        var index = IndexOf(child);
        var endScrollOffset = ChildScrollOffset(child) + ChildMainAxisExtent(child, constraints.Axis);

        bool Advance()
        {
            if (child == null)
            {
                return false;
            }

            var nextChild = ChildAfter(child);
            var nextIndex = index + 1;
            if (nextChild == null || IndexOf(nextChild) != nextIndex)
            {
                nextChild = InsertAndLayoutChild(childConstraints, child);
                if (nextChild == null)
                {
                    return false;
                }
            }
            else
            {
                nextChild.Layout(childConstraints, parentUsesSize: true);
            }

            var nextChildParentData = (SliverMultiBoxAdaptorParentData)nextChild.parentData!;
            nextChildParentData.Index = nextIndex;
            nextChildParentData.LayoutOffset = endScrollOffset;
            nextChildParentData.offset = constraints.Axis == Axis.Vertical
                ? new Point(0, nextChildParentData.LayoutOffset - scrollOffset)
                : new Point(nextChildParentData.LayoutOffset - scrollOffset, 0);

            child = nextChild;
            index = nextIndex;
            endScrollOffset = nextChildParentData.LayoutOffset + ChildMainAxisExtent(nextChild, constraints.Axis);
            return true;
        }

        while (endScrollOffset < scrollOffset)
        {
            leadingGarbage += 1;
            if (!Advance())
            {
                reachedEnd = true;
                if (leadingGarbage > 0)
                {
                    leadingGarbage -= 1;
                }

                break;
            }
        }

        if (!reachedEnd)
        {
            while (endScrollOffset < targetEndScrollOffset)
            {
                if (!Advance())
                {
                    reachedEnd = true;
                    break;
                }
            }
        }

        if (child != null)
        {
            for (var trailingChild = ChildAfter(child); trailingChild != null; trailingChild = ChildAfter(trailingChild))
            {
                trailingGarbage += 1;
            }
        }

        CollectGarbage(leadingGarbage, trailingGarbage);

        firstChild = FirstChild;
        if (firstChild == null)
        {
            Geometry = default;
            return;
        }

        var firstIndex = IndexOf(firstChild);
        var leadingScrollOffset = ChildScrollOffset(firstChild);
        var estimatedMaxScrollOffset = reachedEnd
            ? endScrollOffset
            : EstimateMaxScrollOffset(
                firstIndex,
                index,
                leadingScrollOffset,
                endScrollOffset,
                childManager.ChildCount);

        var paintExtent = CalculatePaintExtent(
            from: leadingScrollOffset,
            to: endScrollOffset,
            scrollOffset: scrollOffset,
            remainingPaintExtent: constraints.RemainingPaintExtent);
        var layoutExtent = Math.Min(paintExtent, constraints.ViewportMainAxisExtent);

        Geometry = new SliverGeometry(
            ScrollExtent: estimatedMaxScrollOffset,
            PaintExtent: paintExtent,
            LayoutExtent: layoutExtent,
            MaxPaintExtent: estimatedMaxScrollOffset,
            HasVisualOverflow: endScrollOffset > targetEndScrollOffset || scrollOffset > 0);
    }

    private static double EstimateMaxScrollOffset(
        int firstIndex,
        int lastIndex,
        double leadingScrollOffset,
        double trailingScrollOffset,
        int? childCount)
    {
        if (!childCount.HasValue)
        {
            return double.PositiveInfinity;
        }

        if (lastIndex >= childCount.Value - 1)
        {
            return trailingScrollOffset;
        }

        var reifiedCount = Math.Max(1, lastIndex - firstIndex + 1);
        var averageExtent = (trailingScrollOffset - leadingScrollOffset) / reifiedCount;
        var remainingCount = Math.Max(0, childCount.Value - lastIndex - 1);
        return trailingScrollOffset + averageExtent * remainingCount;
    }

    private static double CalculatePaintExtent(
        double from,
        double to,
        double scrollOffset,
        double remainingPaintExtent)
    {
        var visibleStart = Math.Max(from, scrollOffset);
        var visibleEnd = Math.Min(to, scrollOffset + remainingPaintExtent);
        return Math.Max(0, visibleEnd - visibleStart);
    }
}
