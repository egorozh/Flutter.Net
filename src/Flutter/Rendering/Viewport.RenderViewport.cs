using Avalonia;
using Avalonia.Media;

namespace Flutter.Rendering;

public sealed class RenderViewport : RenderProxyBox
{
    private Axis _axis;
    private double _offsetPixels;
    private double _viewportExtent;
    private double _maxScrollExtent;

    public RenderViewport(
        Axis axis = Axis.Vertical,
        double offsetPixels = 0.0,
        Action<double, double, double>? onViewportMetricsChanged = null,
        RenderBox? child = null)
    {
        _axis = axis;
        _offsetPixels = offsetPixels;
        OnViewportMetricsChanged = onViewportMetricsChanged;
        Child = child;
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

    protected override void PerformLayout()
    {
        if (Child == null)
        {
            Size = Constraints.Constrain(new Size());
            _viewportExtent = Axis == Axis.Vertical ? Size.Height : Size.Width;
            _maxScrollExtent = 0;
            OnViewportMetricsChanged?.Invoke(_viewportExtent, 0, _maxScrollExtent);
            return;
        }

        BoxConstraints childConstraints;
        if (Axis == Axis.Vertical)
        {
            childConstraints = new BoxConstraints(
                MinWidth: Constraints.MinWidth,
                MaxWidth: Constraints.MaxWidth,
                MinHeight: 0,
                MaxHeight: double.PositiveInfinity);
        }
        else
        {
            childConstraints = new BoxConstraints(
                MinWidth: 0,
                MaxWidth: double.PositiveInfinity,
                MinHeight: Constraints.MinHeight,
                MaxHeight: Constraints.MaxHeight);
        }

        Child.Layout(childConstraints, parentUsesSize: true);

        var viewportSize = Constraints.Constrain(new Size(
            width: Axis == Axis.Vertical ? Child.Size.Width : Constraints.ConstrainWidth(),
            height: Axis == Axis.Vertical ? Constraints.ConstrainHeight() : Child.Size.Height));
        Size = viewportSize;

        _viewportExtent = Axis == Axis.Vertical ? Size.Height : Size.Width;
        var childExtent = Axis == Axis.Vertical ? Child.Size.Height : Child.Size.Width;
        _maxScrollExtent = Math.Max(0, childExtent - _viewportExtent);

        var effectiveOffset = Math.Clamp(_offsetPixels, 0, _maxScrollExtent);
        if (Math.Abs(effectiveOffset - _offsetPixels) > 0.0001)
        {
            _offsetPixels = effectiveOffset;
        }

        var childParentData = (BoxParentData)Child.parentData!;
        childParentData.offset = Axis == Axis.Vertical
            ? new Point(0, -effectiveOffset)
            : new Point(-effectiveOffset, 0);

        OnViewportMetricsChanged?.Invoke(_viewportExtent, 0, _maxScrollExtent);
    }

    public override void Paint(PaintingContext ctx, Point offset)
    {
        var clipRect = new Rect(offset, Size);
        ctx.PushClipRect(clipRect, clippedContext => base.Paint(clippedContext, offset));
    }

    protected override bool HitTestChildren(BoxHitTestResult result, Point position)
    {
        if (position.X < 0 || position.Y < 0 || position.X > Size.Width || position.Y > Size.Height)
        {
            return false;
        }

        return base.HitTestChildren(result, position);
    }

    protected override Rect? DescribeApproximatePaintClip(RenderObject? child)
    {
        return new Rect(new Point(0, 0), Size);
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
