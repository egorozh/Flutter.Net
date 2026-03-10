using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;

// Dart parity source (reference): flutter/packages/flutter/lib/src/widgets/basic.dart (approximate)

namespace Flutter.Widgets;

public sealed class SizedBox : SingleChildRenderObjectWidget
{
    public SizedBox(double? width = null, double? height = null, Widget? child = null, Key? key = null) : base(child, key)
    {
        Width = width;
        Height = height;
    }

    public double? Width { get; }

    public double? Height { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderConstrainedBox(BoxConstraints.TightFor(width: Width, height: Height));
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        ((RenderConstrainedBox)renderObject).AdditionalConstraints = BoxConstraints.TightFor(width: Width, height: Height);
    }
}

public sealed class ConstrainedBox : SingleChildRenderObjectWidget
{
    public ConstrainedBox(BoxConstraints constraints, Widget? child = null, Key? key = null) : base(child, key)
    {
        Constraints = constraints;
    }

    public BoxConstraints Constraints { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderConstrainedBox(Constraints);
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        ((RenderConstrainedBox)renderObject).AdditionalConstraints = Constraints;
    }
}

public sealed class Padding : SingleChildRenderObjectWidget
{
    public Padding(Thickness insets, Widget child, Key? key = null) : base(child, key)
    {
        Insets = insets;
    }

    public Thickness Insets { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderPadding(Insets);
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        ((RenderPadding)renderObject).Padding = Insets;
    }
}

public sealed class ColoredBox : SingleChildRenderObjectWidget
{
    public ColoredBox(Color color, Widget? child = null, Key? key = null) : base(child, key)
    {
        Color = color;
    }

    public Color Color { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderColoredBox(Color);
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        ((RenderColoredBox)renderObject).Color = Color;
    }
}

public sealed class DecoratedBox : SingleChildRenderObjectWidget
{
    public DecoratedBox(BoxDecoration decoration, Widget? child = null, Key? key = null) : base(child, key)
    {
        Decoration = decoration ?? new BoxDecoration();
    }

    public BoxDecoration Decoration { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderDecoratedBox(Decoration);
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        ((RenderDecoratedBox)renderObject).Decoration = Decoration;
    }
}

public sealed class Opacity : SingleChildRenderObjectWidget
{
    public Opacity(double opacity, Widget? child = null, Key? key = null) : base(child, key)
    {
        Value = opacity;
    }

    public double Value { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderOpacity(Value);
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        ((RenderOpacity)renderObject).Opacity = Value;
    }
}

public sealed class Transform : SingleChildRenderObjectWidget
{
    public Transform(Matrix transform, Widget? child = null, Key? key = null) : base(child, key)
    {
        Matrix = transform;
    }

    public Matrix Matrix { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderTransform(Matrix);
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        ((RenderTransform)renderObject).Transform = Matrix;
    }
}

public sealed class ClipRect : SingleChildRenderObjectWidget
{
    public ClipRect(Rect clipRect, Widget? child = null, Key? key = null) : base(child, key)
    {
        Clip = clipRect;
    }

    public Rect Clip { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderClipRect
        {
            ClipRect = Clip
        };
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        ((RenderClipRect)renderObject).ClipRect = Clip;
    }
}

public sealed class Container : StatelessWidget
{
    public Container(
        Widget? child = null,
        Color? color = null,
        BoxDecoration? decoration = null,
        Alignment? alignment = null,
        Thickness? margin = null,
        Thickness? padding = null,
        double? width = null,
        double? height = null,
        Key? key = null) : base(key)
    {
        Child = child;
        Color = color;
        Decoration = decoration;
        Alignment = alignment;
        Margin = margin;
        Padding = padding;
        Width = width;
        Height = height;
    }

    public Widget? Child { get; }

    public Color? Color { get; }

    public BoxDecoration? Decoration { get; }

    public Alignment? Alignment { get; }

    public Thickness? Margin { get; }

    public Thickness? Padding { get; }

    public double? Width { get; }

    public double? Height { get; }

    public override Widget Build(BuildContext context)
    {
        Widget current = Child ?? new SizedBox();

        if (Alignment.HasValue)
        {
            current = new Align(
                alignment: Alignment.Value,
                child: current);
        }

        if (Padding.HasValue)
        {
            current = new Padding(Padding.Value, current);
        }

        if (Decoration != null)
        {
            current = new DecoratedBox(Decoration, current);
        }
        else if (Color.HasValue)
        {
            current = new ColoredBox(Color.Value, current);
        }

        if (Width.HasValue || Height.HasValue)
        {
            current = new SizedBox(width: Width, height: Height, child: current);
        }

        if (Margin.HasValue)
        {
            current = new Padding(Margin.Value, current);
        }

        return current;
    }
}

public class Align : SingleChildRenderObjectWidget
{
    public Align(
        Widget? child = null,
        Alignment alignment = default,
        double? widthFactor = null,
        double? heightFactor = null,
        Key? key = null) : base(child, key)
    {
        Alignment = alignment;
        WidthFactor = widthFactor;
        HeightFactor = heightFactor;
    }

    public Alignment Alignment { get; }

    public double? WidthFactor { get; }

    public double? HeightFactor { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderAlign(
            alignment: Alignment,
            widthFactor: WidthFactor,
            heightFactor: HeightFactor);
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        var align = (RenderAlign)renderObject;
        align.Alignment = Alignment;
        align.WidthFactor = WidthFactor;
        align.HeightFactor = HeightFactor;
    }
}

public sealed class Center : Align
{
    public Center(
        Widget? child = null,
        double? widthFactor = null,
        double? heightFactor = null,
        Key? key = null) : base(
        child: child,
        alignment: Alignment.Center,
        widthFactor: widthFactor,
        heightFactor: heightFactor,
        key: key)
    {
    }
}

public class Flex : MultiChildRenderObjectWidget
{
    public Flex(
        Axis direction,
        IReadOnlyList<Widget>? children = null,
        MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Center,
        double spacing = 0,
        Key? key = null) : base(children, key)
    {
        Direction = direction;
        MainAxisAlignment = mainAxisAlignment;
        CrossAxisAlignment = crossAxisAlignment;
        Spacing = spacing;
    }

    public Axis Direction { get; }

    public MainAxisAlignment MainAxisAlignment { get; }

    public CrossAxisAlignment CrossAxisAlignment { get; }

    public double Spacing { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderFlex(
            children: null,
            direction: Direction,
            mainAxisAlignment: MainAxisAlignment,
            crossAxisAlignment: CrossAxisAlignment,
            spacing: Spacing);
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        var flex = (RenderFlex)renderObject;
        flex.Direction = Direction;
        flex.MainAxisAlignment = MainAxisAlignment;
        flex.CrossAxisAlignment = CrossAxisAlignment;
        flex.Spacing = Spacing;
    }
}

public class Flexible : ParentDataWidget<FlexParentData>
{
    public Flexible(
        Widget child,
        int flex = 1,
        FlexFit fit = FlexFit.Loose,
        Key? key = null) : base(child, key)
    {
        Flex = flex;
        Fit = fit;
    }

    public int Flex { get; }

    public FlexFit Fit { get; }

    public override Type DebugTypicalAncestorWidgetType => typeof(Flex);

    protected override void ApplyParentData(RenderObject renderObject)
    {
        var parentData = (FlexParentData)renderObject.parentData!;
        var needsLayout = false;

        if (parentData.flex != Flex)
        {
            parentData.flex = Flex;
            needsLayout = true;
        }

        if (parentData.fit != Fit)
        {
            parentData.fit = Fit;
            needsLayout = true;
        }

        if (needsLayout)
        {
            renderObject.Parent?.MarkNeedsLayout();
        }
    }
}

public sealed class Expanded : Flexible
{
    public Expanded(Widget child, int flex = 1, Key? key = null) : base(
        child: child,
        flex: flex,
        fit: FlexFit.Tight,
        key: key)
    {
    }
}

public sealed class Row : Flex
{
    public Row(
        IReadOnlyList<Widget>? children = null,
        MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Center,
        double spacing = 0,
        Key? key = null) : base(
        direction: Axis.Horizontal,
        children: children,
        mainAxisAlignment: mainAxisAlignment,
        crossAxisAlignment: crossAxisAlignment,
        spacing: spacing,
        key: key)
    {
    }
}

public sealed class Column : Flex
{
    public Column(
        IReadOnlyList<Widget>? children = null,
        MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Center,
        double spacing = 0,
        Key? key = null) : base(
        direction: Axis.Vertical,
        children: children,
        mainAxisAlignment: mainAxisAlignment,
        crossAxisAlignment: crossAxisAlignment,
        spacing: spacing,
        key: key)
    {
    }
}

public sealed class Stack : MultiChildRenderObjectWidget
{
    public Stack(
        IReadOnlyList<Widget>? children = null,
        Alignment alignment = default,
        StackFit fit = StackFit.Loose,
        Key? key = null) : base(children, key)
    {
        Alignment = alignment;
        Fit = fit;
    }

    public Alignment Alignment { get; }

    public StackFit Fit { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderStack(
            alignment: Alignment,
            fit: Fit);
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        var stack = (RenderStack)renderObject;
        stack.Alignment = Alignment;
        stack.Fit = Fit;
    }
}

public sealed class Positioned : ParentDataWidget<StackParentData>
{
    public Positioned(
        Widget child,
        double? left = null,
        double? top = null,
        double? right = null,
        double? bottom = null,
        double? width = null,
        double? height = null,
        Key? key = null) : base(child, key)
    {
        if (left.HasValue && right.HasValue && width.HasValue)
        {
            throw new ArgumentException("Cannot provide left, right, and width simultaneously.");
        }

        if (top.HasValue && bottom.HasValue && height.HasValue)
        {
            throw new ArgumentException("Cannot provide top, bottom, and height simultaneously.");
        }

        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
        Width = width;
        Height = height;
    }

    public double? Left { get; }

    public double? Top { get; }

    public double? Right { get; }

    public double? Bottom { get; }

    public double? Width { get; }

    public double? Height { get; }

    public override Type DebugTypicalAncestorWidgetType => typeof(Stack);

    protected override void ApplyParentData(RenderObject renderObject)
    {
        var parentData = (StackParentData)renderObject.parentData!;
        var needsLayout = false;

        if (parentData.Left != Left)
        {
            parentData.Left = Left;
            needsLayout = true;
        }

        if (parentData.Top != Top)
        {
            parentData.Top = Top;
            needsLayout = true;
        }

        if (parentData.Right != Right)
        {
            parentData.Right = Right;
            needsLayout = true;
        }

        if (parentData.Bottom != Bottom)
        {
            parentData.Bottom = Bottom;
            needsLayout = true;
        }

        if (parentData.Width != Width)
        {
            parentData.Width = Width;
            needsLayout = true;
        }

        if (parentData.Height != Height)
        {
            parentData.Height = Height;
            needsLayout = true;
        }

        if (needsLayout)
        {
            renderObject.Parent?.MarkNeedsLayout();
        }
    }
}
