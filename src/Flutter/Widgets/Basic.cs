using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;

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

public sealed class Container : StatelessWidget
{
    public Container(
        Widget? child = null,
        Color? color = null,
        Thickness? padding = null,
        double? width = null,
        double? height = null,
        Key? key = null) : base(key)
    {
        Child = child;
        Color = color;
        Padding = padding;
        Width = width;
        Height = height;
    }

    public Widget? Child { get; }

    public Color? Color { get; }

    public Thickness? Padding { get; }

    public double? Width { get; }

    public double? Height { get; }

    public override Widget Build(BuildContext context)
    {
        Widget current = Child ?? new SizedBox();

        if (Padding.HasValue)
        {
            current = new Padding(Padding.Value, current);
        }

        if (Color.HasValue)
        {
            current = new ColoredBox(Color.Value, current);
        }

        if (Width.HasValue || Height.HasValue)
        {
            current = new SizedBox(width: Width, height: Height, child: current);
        }

        return current;
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
