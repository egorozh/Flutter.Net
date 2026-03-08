using Avalonia;
using Avalonia.Media;

namespace Flutter.Rendering;

public sealed class RenderViewport : RenderBox, IRenderObjectContainer
{
    private readonly RenderBoxContainerDefaultsMixin<RenderSliver, SliverPhysicalParentData> _container;
    private Axis _axis;
    private double _offsetPixels;
    private double _maxScrollExtent;
    private RenderSliverToBoxAdapter? _legacyChildSliver;

    public RenderViewport(
        Axis axis = Axis.Vertical,
        double offsetPixels = 0.0,
        Action<double, double, double>? onViewportMetricsChanged = null,
        RenderBox? child = null)
    {
        _container = new RenderBoxContainerDefaultsMixin<RenderSliver, SliverPhysicalParentData>(this);
        _axis = axis;
        _offsetPixels = offsetPixels;
        OnViewportMetricsChanged = onViewportMetricsChanged;

        if (child != null)
        {
            Child = child;
        }
    }

    public Axis Axis
    {
        get => _axis;
        set
        {
            if (_axis == value)
            {
                return;
            }

            _axis = value;
            MarkNeedsLayout();
        }
    }

    public double OffsetPixels
    {
        get => _offsetPixels;
        set
        {
            if (Math.Abs(_offsetPixels - value) < 0.0001)
            {
                return;
            }

            _offsetPixels = value;
            MarkNeedsLayout();
        }
    }

    public Action<double, double, double>? OnViewportMetricsChanged { get; set; }

    // Backward-compatible single child API used by existing tests/widgets.
    public RenderBox? Child
    {
        get => _legacyChildSliver?.Child;
        set
        {
            if (ReferenceEquals(_legacyChildSliver?.Child, value))
            {
                return;
            }

            if (_legacyChildSliver != null)
            {
                Remove(_legacyChildSliver);
                _legacyChildSliver = null;
            }

            if (value != null)
            {
                _legacyChildSliver = new RenderSliverToBoxAdapter(value);
                Insert(_legacyChildSliver, after: LastChild);
            }
        }
    }

    public int ChildCount => _container.ChildCount;

    public RenderSliver? FirstChild => _container.FirstChild;

    public RenderSliver? LastChild => _container.LastChild;

    public void Insert(RenderSliver child, RenderSliver? after = null)
    {
        _container.Insert(child, after);
    }

    public void Move(RenderSliver child, RenderSliver? after = null)
    {
        _container.Move(child, after);
    }

    public void Remove(RenderSliver child)
    {
        if (ReferenceEquals(child, _legacyChildSliver))
        {
            _legacyChildSliver = null;
        }

        _container.Remove(child);
    }

    void IRenderObjectContainer.Insert(RenderObject child, RenderObject? after)
    {
        Insert((RenderSliver)child, (RenderSliver?)after);
    }

    void IRenderObjectContainer.Move(RenderObject child, RenderObject? after)
    {
        Move((RenderSliver)child, (RenderSliver?)after);
    }

    void IRenderObjectContainer.Remove(RenderObject child)
    {
        Remove((RenderSliver)child);
    }

    public override void SetupParentData(RenderObject child)
    {
        if (child.parentData is not SliverPhysicalParentData)
        {
            child.parentData = new SliverPhysicalParentData();
        }
    }

    public override void VisitChildren(Action<RenderObject> visitor)
    {
        for (var child = FirstChild; child != null; child = _container.ChildAfter(child))
        {
            visitor(child);
        }
    }

    protected override void PerformLayout()
    {
        Size = Constraints.Constrain(Constraints.Biggest);

        var viewportMainAxisExtent = Axis == Axis.Vertical ? Size.Height : Size.Width;
        var crossAxisExtent = Axis == Axis.Vertical ? Size.Width : Size.Height;

        var (totalScrollExtent, _) = LayoutChildren(
            scrollOffset: _offsetPixels,
            viewportMainAxisExtent: viewportMainAxisExtent,
            crossAxisExtent: crossAxisExtent);

        var clampedOffset = Math.Clamp(_offsetPixels, 0, Math.Max(0, totalScrollExtent - viewportMainAxisExtent));
        if (Math.Abs(clampedOffset - _offsetPixels) > 0.0001)
        {
            _offsetPixels = clampedOffset;
            (totalScrollExtent, _) = LayoutChildren(
                scrollOffset: _offsetPixels,
                viewportMainAxisExtent: viewportMainAxisExtent,
                crossAxisExtent: crossAxisExtent);
        }

        _maxScrollExtent = Math.Max(0, totalScrollExtent - viewportMainAxisExtent);
        OnViewportMetricsChanged?.Invoke(viewportMainAxisExtent, 0, _maxScrollExtent);
    }

    public override void Paint(PaintingContext ctx, Point offset)
    {
        if (Size.Width <= 0 || Size.Height <= 0)
        {
            return;
        }

        var clipRect = new Rect(offset, Size);
        ctx.PushClipRect(clipRect, clippedContext => _container.DefaultPaint(clippedContext, offset));
    }

    protected override bool HitTestChildren(BoxHitTestResult result, Point position)
    {
        if (position.X < 0 || position.Y < 0 || position.X > Size.Width || position.Y > Size.Height)
        {
            return false;
        }

        return _container.DefaultHitTestChildren(result, position);
    }

    protected override Rect? DescribeApproximatePaintClip(RenderObject? child)
    {
        return new Rect(new Point(0, 0), Size);
    }

    internal override void VisitChildrenForSemantics(Action<RenderObject, Point, Matrix> visitor)
    {
        for (var child = FirstChild; child != null; child = _container.ChildAfter(child))
        {
            var parentData = (SliverPhysicalParentData)child.parentData!;
            visitor(child, parentData.offset, Matrix.Identity);
        }
    }

    private (double totalScrollExtent, double paintedExtent) LayoutChildren(
        double scrollOffset,
        double viewportMainAxisExtent,
        double crossAxisExtent)
    {
        var precedingScrollExtent = 0.0;
        var paintedExtent = 0.0;

        for (var child = FirstChild; child != null; child = _container.ChildAfter(child))
        {
            var localScrollOffset = Math.Max(0, scrollOffset - precedingScrollExtent);
            var remainingPaintExtent = Math.Max(0, viewportMainAxisExtent - paintedExtent);

            child.LayoutWithSliverConstraints(new SliverConstraints(
                Axis,
                localScrollOffset,
                remainingPaintExtent,
                crossAxisExtent,
                viewportMainAxisExtent));

            var parentData = (SliverPhysicalParentData)child.parentData!;
            parentData.offset = Axis == Axis.Vertical
                ? new Point(0, paintedExtent)
                : new Point(paintedExtent, 0);

            precedingScrollExtent += child.Geometry.ScrollExtent;
            paintedExtent += child.Geometry.PaintExtent;
        }

        return (precedingScrollExtent, paintedExtent);
    }
}
