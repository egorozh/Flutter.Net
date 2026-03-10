using Avalonia;
using Avalonia.Media;
using Flutter.UI;
using Flutter.Widgets;

// Dart parity source (reference): flutter/packages/flutter/lib/src/rendering/proxy_box.dart (approximate)

namespace Flutter.Rendering;

public abstract class RenderProxyBox : RenderBox, IRenderObjectSingleChildContainer
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

    internal override void VisitChildrenForSemantics(Action<RenderObject, Point, Matrix> visitor)
    {
        if (_child != null)
        {
            var childParentData = (BoxParentData)_child.parentData!;
            visitor(_child, childParentData.offset, Matrix.Identity);
        }
    }

    protected override void PerformLayout()
    {
        if (_child != null)
        {
            _child.Layout(Constraints, parentUsesSize: true);
            Size = Constraints.Constrain(_child.Size);
            ((BoxParentData)_child.parentData!).offset = new Point(0, 0);
        }
        else
        {
            Size = Constraints.Constrain(new Size());
        }
    }

    public override void Paint(PaintingContext ctx, Point offset)
    {
        if (_child != null)
        {
            var childParentData = (BoxParentData)_child.parentData!;
            ctx.PaintChild(_child, childParentData.offset + offset);
        }
    }

    protected override bool HitTestChildren(BoxHitTestResult result, Point position)
    {
        if (_child == null)
        {
            return false;
        }

        var childParentData = (BoxParentData)_child.parentData!;
        return _child.HitTest(result, position - childParentData.offset);
    }
}

public sealed class RenderConstrainedBox : RenderProxyBox
{
    private BoxConstraints _additionalConstraints;

    public RenderConstrainedBox(BoxConstraints additionalConstraints, RenderBox? child = null)
    {
        _additionalConstraints = additionalConstraints;
        Child = child;
    }

    public BoxConstraints AdditionalConstraints
    {
        get => _additionalConstraints;
        set
        {
            if (_additionalConstraints.Equals(value))
            {
                return;
            }

            _additionalConstraints = value;
            MarkNeedsLayout();
        }
    }

    protected override void PerformLayout()
    {
        var enforced = _additionalConstraints.Enforce(Constraints);

        if (Child != null)
        {
            Child.Layout(enforced, parentUsesSize: true);
            Size = Constraints.Constrain(Child.Size);
            ((BoxParentData)Child.parentData!).offset = new Point(0, 0);
        }
        else
        {
            Size = enforced.Constrain(new Size());
        }
    }
}

public sealed class RenderPadding : RenderProxyBox
{
    private Thickness _padding;

    public RenderPadding(Thickness padding, RenderBox? child = null)
    {
        _padding = padding;
        Child = child;
    }

    public Thickness Padding
    {
        get => _padding;
        set
        {
            if (_padding.Equals(value))
            {
                return;
            }

            _padding = value;
            MarkNeedsLayout();
        }
    }

    protected override void PerformLayout()
    {
        if (Child == null)
        {
            Size = Constraints.Constrain(new Size(Padding.Left + Padding.Right, Padding.Top + Padding.Bottom));
            return;
        }

        var innerConstraints = Constraints.Deflate(Padding);
        Child.Layout(innerConstraints, parentUsesSize: true);

        var childSize = Child.Size;
        Size = Constraints.Constrain(
            new Size(childSize.Width + Padding.Left + Padding.Right, childSize.Height + Padding.Top + Padding.Bottom));

        ((BoxParentData)Child.parentData!).offset = new Point(Padding.Left, Padding.Top);
    }
}

public sealed class RenderAlign : RenderProxyBox
{
    private Alignment _alignment;
    private double? _widthFactor;
    private double? _heightFactor;

    public RenderAlign(
        Alignment alignment = default,
        double? widthFactor = null,
        double? heightFactor = null,
        RenderBox? child = null)
    {
        _alignment = alignment;
        _widthFactor = ValidateFactor(widthFactor, nameof(widthFactor));
        _heightFactor = ValidateFactor(heightFactor, nameof(heightFactor));
        Child = child;
    }

    public Alignment Alignment
    {
        get => _alignment;
        set
        {
            if (_alignment == value)
            {
                return;
            }

            _alignment = value;
            MarkNeedsLayout();
        }
    }

    public double? WidthFactor
    {
        get => _widthFactor;
        set
        {
            var normalized = ValidateFactor(value, nameof(value));
            if (_widthFactor == normalized)
            {
                return;
            }

            _widthFactor = normalized;
            MarkNeedsLayout();
        }
    }

    public double? HeightFactor
    {
        get => _heightFactor;
        set
        {
            var normalized = ValidateFactor(value, nameof(value));
            if (_heightFactor == normalized)
            {
                return;
            }

            _heightFactor = normalized;
            MarkNeedsLayout();
        }
    }

    protected override void PerformLayout()
    {
        var shrinkWrapWidth = _widthFactor.HasValue;
        var shrinkWrapHeight = _heightFactor.HasValue;

        if (Child == null)
        {
            var fallbackWidth = shrinkWrapWidth ? 0.0 : double.PositiveInfinity;
            var fallbackHeight = shrinkWrapHeight ? 0.0 : double.PositiveInfinity;
            Size = Constraints.Constrain(new Size(fallbackWidth, fallbackHeight));
            return;
        }

        Child.Layout(BoxConstraints.Loose(Constraints.Biggest), parentUsesSize: true);
        var childSize = Child.Size;
        var targetWidth = shrinkWrapWidth ? childSize.Width * _widthFactor!.Value : double.PositiveInfinity;
        var targetHeight = shrinkWrapHeight ? childSize.Height * _heightFactor!.Value : double.PositiveInfinity;
        Size = Constraints.Constrain(new Size(targetWidth, targetHeight));
        ((BoxParentData)Child.parentData!).offset = _alignment.AlongOffset(Size, childSize);
    }

    private static double? ValidateFactor(double? value, string parameterName)
    {
        if (!value.HasValue)
        {
            return null;
        }

        if (value.Value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Factor must be non-negative.");
        }

        return value.Value;
    }
}

public sealed class RenderColoredBox : RenderProxyBox
{
    private Color _color;

    public RenderColoredBox(Color color, RenderBox? child = null)
    {
        _color = color;
        Child = child;
    }

    public Color Color
    {
        get => _color;
        set
        {
            if (_color == value)
            {
                return;
            }

            _color = value;
            MarkNeedsPaint();
        }
    }

    public override void Paint(PaintingContext ctx, Point offset)
    {
        ctx.DrawRectangle(new SolidColorBrush(Color), null, new Rect(offset, Size));
        base.Paint(ctx, offset);
    }
}

public sealed class RenderOpacity : RenderProxyBox
{
    private double _opacity;

    public RenderOpacity(double opacity = 1.0, RenderBox? child = null)
    {
        _opacity = Math.Clamp(opacity, 0.0, 1.0);
        Child = child;
    }

    public double Opacity
    {
        get => _opacity;
        set
        {
            var clamped = Math.Clamp(value, 0.0, 1.0);
            if (Math.Abs(_opacity - clamped) < 0.0001)
            {
                return;
            }

            _opacity = clamped;
            if (Child != null)
            {
                MarkNeedsCompositedLayerUpdate();
            }
        }
    }

    public override bool IsRepaintBoundary => Child != null;
    protected override bool AlwaysNeedsCompositing => Child != null && Opacity < 1.0;

    protected override OffsetLayer CreateCompositedLayer(OffsetLayer? oldLayer)
    {
        return oldLayer as OpacityOffsetLayer ?? new OpacityOffsetLayer();
    }

    protected override void UpdateCompositedLayer(OffsetLayer layer)
    {
        if (layer is OpacityOffsetLayer opacityLayer)
        {
            opacityLayer.Opacity = Opacity;
        }
    }
}

public sealed class RenderTransform : RenderProxyBox
{
    private Matrix _transform;

    public RenderTransform(Matrix transform, RenderBox? child = null)
    {
        _transform = transform;
        Child = child;
    }

    public Matrix Transform
    {
        get => _transform;
        set
        {
            if (_transform == value)
            {
                return;
            }

            _transform = value;
            if (Child != null)
            {
                MarkNeedsCompositedLayerUpdate();
                MarkNeedsSemanticsUpdate();
            }
        }
    }

    public override bool IsRepaintBoundary => Child != null;
    protected override bool AlwaysNeedsCompositing => Child != null;

    internal override void VisitChildrenForSemantics(Action<RenderObject, Point, Matrix> visitor)
    {
        if (Child != null)
        {
            var childParentData = (BoxParentData)Child.parentData!;
            visitor(Child, childParentData.offset, Transform);
        }
    }

    protected override OffsetLayer CreateCompositedLayer(OffsetLayer? oldLayer)
    {
        return oldLayer as TransformOffsetLayer ?? new TransformOffsetLayer();
    }

    protected override void UpdateCompositedLayer(OffsetLayer layer)
    {
        if (layer is TransformOffsetLayer transformLayer)
        {
            transformLayer.Transform = Transform;
        }
    }

    protected override bool HitTestChildren(BoxHitTestResult result, Point position)
    {
        if (Child == null)
        {
            return false;
        }

        if (!Transform.TryInvert(out var inverse))
        {
            return false;
        }

        var childParentData = (BoxParentData)Child.parentData!;
        var transformedPosition = inverse.Transform(position - childParentData.offset);
        return Child.HitTest(result, transformedPosition);
    }
}

public sealed class RenderClipRect : RenderProxyBox
{
    private Rect _clipRect;
    private bool _hasExplicitClipRect;

    public RenderClipRect(RenderBox? child = null)
    {
        Child = child;
    }

    public Rect ClipRect
    {
        get => _clipRect;
        set
        {
            if (_hasExplicitClipRect && _clipRect == value)
            {
                return;
            }

            _clipRect = value;
            _hasExplicitClipRect = true;
            if (Child != null)
            {
                MarkNeedsCompositedLayerUpdate();
                MarkNeedsSemanticsUpdate();
            }
        }
    }

    public override bool IsRepaintBoundary => Child != null;
    protected override bool AlwaysNeedsCompositing => Child != null;

    protected override OffsetLayer CreateCompositedLayer(OffsetLayer? oldLayer)
    {
        return oldLayer as ClipRectOffsetLayer ?? new ClipRectOffsetLayer();
    }

    protected override void UpdateCompositedLayer(OffsetLayer layer)
    {
        if (layer is ClipRectOffsetLayer clipLayer)
        {
            clipLayer.ClipRect = _hasExplicitClipRect ? _clipRect : new Rect(new Point(0, 0), Size);
        }
    }

    protected override Rect? DescribeSemanticsClip(RenderObject? child)
    {
        return null;
    }

    protected override Rect? DescribeApproximatePaintClip(RenderObject? child)
    {
        return _hasExplicitClipRect ? _clipRect : new Rect(new Point(0, 0), Size);
    }

    public override bool HitTest(BoxHitTestResult result, Point position)
    {
        var clip = _hasExplicitClipRect ? _clipRect : new Rect(new Point(0, 0), Size);
        if (!clip.Contains(position))
        {
            return false;
        }

        return base.HitTest(result, position);
    }
}

public sealed class RenderPointerListener : RenderProxyBox
{
    private HitTestBehavior _behavior;

    public RenderPointerListener(
        Action<PointerDownEvent>? onPointerDown = null,
        Action<PointerMoveEvent>? onPointerMove = null,
        Action<PointerHoverEvent>? onPointerHover = null,
        Action<PointerUpEvent>? onPointerUp = null,
        Action<PointerCancelEvent>? onPointerCancel = null,
        Action<PointerSignalEvent>? onPointerSignal = null,
        HitTestBehavior behavior = HitTestBehavior.DeferToChild,
        RenderBox? child = null)
    {
        OnPointerDown = onPointerDown;
        OnPointerMove = onPointerMove;
        OnPointerHover = onPointerHover;
        OnPointerUp = onPointerUp;
        OnPointerCancel = onPointerCancel;
        OnPointerSignal = onPointerSignal;
        _behavior = behavior;
        Child = child;
    }

    public Action<PointerDownEvent>? OnPointerDown { get; set; }

    public Action<PointerMoveEvent>? OnPointerMove { get; set; }

    public Action<PointerHoverEvent>? OnPointerHover { get; set; }

    public Action<PointerUpEvent>? OnPointerUp { get; set; }

    public Action<PointerCancelEvent>? OnPointerCancel { get; set; }

    public Action<PointerSignalEvent>? OnPointerSignal { get; set; }

    public HitTestBehavior Behavior
    {
        get => _behavior;
        set => _behavior = value;
    }

    public override bool HitTest(BoxHitTestResult result, Point position)
    {
        if (position.X < 0 || position.Y < 0 || position.X > Size.Width || position.Y > Size.Height)
        {
            return false;
        }

        var hitTarget = HitTestChildren(result, position) || HitTestSelf(position);
        if (hitTarget || Behavior == HitTestBehavior.Translucent || Behavior == HitTestBehavior.Opaque)
        {
            result.Add(new BoxHitTestEntry(this, position));
        }

        return hitTarget || Behavior == HitTestBehavior.Opaque || Behavior == HitTestBehavior.Translucent;
    }

    protected override bool HitTestSelf(Point position)
    {
        return Behavior == HitTestBehavior.Opaque;
    }

    public override void HandleEvent(PointerEvent @event, HitTestEntry entry)
    {
        switch (@event)
        {
            case PointerDownEvent downEvent:
                OnPointerDown?.Invoke(downEvent);
                break;
            case PointerMoveEvent moveEvent:
                OnPointerMove?.Invoke(moveEvent);
                break;
            case PointerHoverEvent hoverEvent:
                OnPointerHover?.Invoke(hoverEvent);
                break;
            case PointerUpEvent upEvent:
                OnPointerUp?.Invoke(upEvent);
                break;
            case PointerCancelEvent cancelEvent:
                OnPointerCancel?.Invoke(cancelEvent);
                break;
            case PointerSignalEvent signalEvent:
                OnPointerSignal?.Invoke(signalEvent);
                break;
        }
    }
}
