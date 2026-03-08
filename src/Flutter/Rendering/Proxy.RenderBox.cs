using Avalonia;
using Avalonia.Media;

namespace Flutter.Rendering;

public abstract class RenderProxyBox : RenderBox
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
