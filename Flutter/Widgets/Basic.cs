using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;

namespace Flutter.Widgets;

// Base element that owns a Control (render host)
public abstract class ControlElement<TControl> : Element where TControl : Control, new()
{
    public TControl HostControl { get; } = new();

    public override Control Control => HostControl;

    protected ControlElement(Widget widget) : base(widget)
    {
    }

    protected override void OnMount()
    {
        base.OnMount();
        // Attach visual to parent container when first child comes in via InsertChildRenderObject
    }
}

// ─────────────────────────────────────────────────────────────────────
// SizedBox (as Decorator)
// ─────────────────────────────────────────────────────────────────────
public sealed class SizedBox : StatelessWidget
{
    public double? Width { get; }
    public double? Height { get; }
    public Widget? Child { get; }

    public SizedBox(double? width = null, double? height = null, Widget? child = null, Key? key = null) : base(key)
    {
        Width = width;
        Height = height;
        Child = child;
    }

    public override Widget Build(BuildContext context) => new _SizedBoxHost(this);

    private sealed class _SizedBoxHost : Widget
    {
        private readonly SizedBox _box;

        public _SizedBoxHost(SizedBox b)
        {
            _box = b;
        }

        internal override Element CreateElement() => new SizedBoxElement(this, _box);
    }
}

internal sealed class SizedBoxElement : ControlElement<Border>
{
    private readonly SizedBox _def;
    private Element? _child;

    public SizedBoxElement(Widget w, SizedBox def) : base(w)
    {
        _def = def;
    }

    protected override void OnMount()
    {
        base.OnMount();
        ApplySize();
        Rebuild();
    }

    private void ApplySize()
    {
        if (_def.Width.HasValue)
            HostControl.Width = _def.Width.Value;

        if (_def.Height.HasValue)
            HostControl.Height = _def.Height.Value;
    }

    internal override void Rebuild()
    {
        Dirty = false;
        _child = TreeHelpers.ReconcileSingleChild(this, _child, _def.Child ?? new SizedBox());
    }

    internal override void Update(Widget newWidget)
    {
        ApplySize();
        MarkNeedsBuild();
    }

    internal override void InsertChildRenderObject(int index, Element child)
    {
        HostControl.Child = child.Control as Control;
    }

    internal override void RemoveChildRenderObject(Element child)
    {
        if (ReferenceEquals(HostControl.Child, child.Control)) HostControl.Child = null;
    }
}

// ─────────────────────────────────────────────────────────────────────
// Padding (Border with Padding)
// ─────────────────────────────────────────────────────────────────────
public sealed class Padding : StatelessWidget
{
    public Thickness Thickness { get; }
    public Widget Child { get; }

    public Padding(Thickness thickness, Widget child, Key? key = null) : base(key)
    {
        Thickness = thickness;
        Child = child;
    }

    public override Widget Build(BuildContext context) => new _PaddingHost(this);

    private sealed class _PaddingHost : Widget
    {
        private readonly Padding _pad;

        public _PaddingHost(Padding p)
        {
            _pad = p;
        }

        internal override Element CreateElement() => new PaddingElement(this, _pad);
    }
}

internal sealed class PaddingElement : ControlElement<Border>
{
    private readonly Padding _def;
    private Element? _child;

    public PaddingElement(Widget w, Padding def) : base(w)
    {
        _def = def;
    }

    protected override void OnMount()
    {
        base.OnMount();
        Apply();
        Rebuild();
    }

    private void Apply()
    {
        HostControl.Padding = _def.Thickness;
    }

    internal override void Rebuild()
    {
        Dirty = false;
        _child = TreeHelpers.ReconcileSingleChild(this, _child, _def.Child);
    }

    internal override void Update(Widget newWidget)
    {
        Apply();
        MarkNeedsBuild();
    }

    internal override void InsertChildRenderObject(int index, Element child)
    {
        HostControl.Child = child.Control as Control;
    }

    internal override void RemoveChildRenderObject(Element child)
    {
        if (ReferenceEquals(HostControl.Child, child.Control)) HostControl.Child = null;
    }
}

// ─────────────────────────────────────────────────────────────────────
// Container (color, padding, corner radius)
// ─────────────────────────────────────────────────────────────────────
public sealed class Container : StatelessWidget
{
    public IBrush? Color { get; }
    public Thickness? Padding { get; }
    public CornerRadius? BorderRadius { get; }
    public Widget? Child { get; }
    public double? Width { get; }
    public double? Height { get; }

    public Container(Widget? child = null, IBrush? color = null, Thickness? padding = null,
        CornerRadius? borderRadius = null, double? width = null, double? height = null, Key? key = null) : base(key)
    {
        Child = child;
        Color = color;
        Padding = padding;
        BorderRadius = borderRadius;
        Width = width;
        Height = height;
    }

    public override Widget Build(BuildContext context) => new _ContainerHost(this);

    private sealed class _ContainerHost : Widget
    {
        private readonly Container _c;

        public _ContainerHost(Container c)
        {
            _c = c;
        }

        internal override Element CreateElement() => new ContainerElement(this, _c);
    }
}

internal sealed class ContainerElement : ControlElement<Border>
{
    private readonly Container _def;
    private Element? _child;

    public ContainerElement(Widget w, Container def) : base(w)
    {
        _def = def;
    }

    protected override void OnMount()
    {
        base.OnMount();
        Apply();
        Rebuild();
    }

    private void Apply()
    {
        HostControl.Background = _def.Color;

        if (_def.Padding.HasValue) HostControl.Padding = _def.Padding.Value;

        if (_def.BorderRadius.HasValue) HostControl.CornerRadius = _def.BorderRadius.Value;

        if (_def.Width.HasValue)
            HostControl.Width = _def.Width.Value;

        if (_def.Height.HasValue)
            HostControl.Height = _def.Height.Value;
    }

    internal override void Rebuild()
    {
        Dirty = false;
        if (_def.Child != null) _child = TreeHelpers.ReconcileSingleChild(this, _child, _def.Child);
    }

    internal override void Update(Widget newWidget)
    {
        Apply();
        MarkNeedsBuild();
    }

    internal override void InsertChildRenderObject(int index, Element child)
    {
        HostControl.Child = child.Control as Control;
    }

    internal override void RemoveChildRenderObject(Element child)
    {
        if (ReferenceEquals(HostControl.Child, child.Control)) HostControl.Child = null;
    }
}

// ─────────────────────────────────────────────────────────────────────
// Row / Column + Expanded
// ─────────────────────────────────────────────────────────────────────
public sealed class Row : StatelessWidget
{
    public IReadOnlyList<Widget> Children { get; }
    public MainAxisAlignment MainAxisAlignment { get; }
    public CrossAxisAlignment CrossAxisAlignment { get; }
    public double Spacing { get; }

    public Row(IReadOnlyList<Widget> children, MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Center, double spacing = 0,
        Key? key = null) : base(key)
    {
        Children = children;
        MainAxisAlignment = mainAxisAlignment;
        CrossAxisAlignment = crossAxisAlignment;
        Spacing = spacing;
    }

    public override Widget Build(BuildContext context) => new _FlexHost(Axis.Horizontal,
        MainAxisAlignment, CrossAxisAlignment, Spacing, Children);
}

public sealed class Column : StatelessWidget
{
    public IReadOnlyList<Widget> Children { get; }
    public MainAxisAlignment MainAxisAlignment { get; }
    public CrossAxisAlignment CrossAxisAlignment { get; }
    public double Spacing { get; }

    public Column(IReadOnlyList<Widget> children, MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Center, double spacing = 0,
        Key? key = null) : base(key)
    {
        Children = children;
        MainAxisAlignment = mainAxisAlignment;
        CrossAxisAlignment = crossAxisAlignment;
        Spacing = spacing;
    }

    public override Widget Build(BuildContext context) => new _FlexHost(Axis.Vertical,
        MainAxisAlignment, CrossAxisAlignment, Spacing, Children);
}

public sealed class Expanded : StatelessWidget
{
    public Widget Child { get; }
    public int Flex { get; }

    public Expanded(Widget child, int flex = 1, Key? key = null) : base(key)
    {
        Child = child;
        Flex = flex;
    }

    public override Widget Build(BuildContext context) => new _ExpandedHost(Child, Flex);
}

// Internal flex hosts map to FlexPanel element
internal sealed class _FlexHost : Widget
{
    private readonly Axis _axis;
    private readonly MainAxisAlignment _main;
    private readonly CrossAxisAlignment _cross;
    private readonly double _spacing;
    private readonly IReadOnlyList<Widget> _children;

    public _FlexHost(Axis axis, MainAxisAlignment main, CrossAxisAlignment cross, double spacing,
        IReadOnlyList<Widget> children)
    {
        _axis = axis;
        _main = main;
        _cross = cross;
        _spacing = spacing;
        _children = children;
    }

    internal override Element CreateElement() => new FlexElement(this, _axis, _main, _cross, _spacing, _children);
}

internal sealed class _ExpandedHost : Widget
{
    private readonly Widget _child;
    private readonly int _flex;

    public _ExpandedHost(Widget child, int flex)
    {
        _child = child;
        _flex = flex;
    }

    internal override Element CreateElement() => new ExpandedElement(this, _child, _flex);
}

internal sealed class FlexElement : ControlElement<FlexPanel>
{
    private readonly Axis _axis;
    private readonly MainAxisAlignment _main;
    private readonly CrossAxisAlignment _cross;
    private readonly double _spacing;
    private readonly IReadOnlyList<Widget> _widgets;
    private List<Element> _children = new();

    public FlexElement(Widget w, Axis axis, MainAxisAlignment main, CrossAxisAlignment cross,
        double spacing, IReadOnlyList<Widget> widgets) : base(w)
    {
        _axis = axis;
        _main = main;
        _cross = cross;
        _spacing = spacing;
        _widgets = widgets;
    }

    protected override void OnMount()
    {
        base.OnMount();
        ApplyProps();
        Rebuild();
    }

    private void ApplyProps()
    {
        HostControl.Direction = _axis;
        HostControl.MainAxisAlignment = _main;
        HostControl.CrossAxisAlignment = _cross;
        HostControl.Spacing = _spacing;
    }

    internal override void Rebuild()
    {
        Dirty = false;
        var old = _children;
        _children = TreeHelpers.ReconcileChildren(this, old, _widgets);
    }

    internal override void Update(Widget newWidget)
    {
        ApplyProps();
        MarkNeedsBuild();
    }

    internal override void InsertChildRenderObject(int index, Element child)
    {
        var ctrl = child.Control as Control;
        if (ctrl != null)
        {
            if (index <= HostControl.Children.Count) HostControl.Children.Insert(index, ctrl);
            else HostControl.Children.Add(ctrl);
        }
    }

    internal override void RemoveChildRenderObject(Element child)
    {
        var ctrl = child.Control as Control;
        if (ctrl != null) HostControl.Children.Remove(ctrl);
    }
}

internal sealed class ExpandedElement : Element
{
    private readonly Widget _childWidget;
    private readonly int _flex;
    private Element? _child;

    public ExpandedElement(Widget w, Widget child, int flex) : base(w)
    {
        _childWidget = child;
        _flex = flex;
    }

    protected override void OnMount()
    {
        base.OnMount();
        Rebuild();
    }

    internal override void Rebuild()
    {
        Dirty = false;

        _child = TreeHelpers.ReconcileSingleChild(this, _child, _childWidget);
    }

    internal override void Update(Widget newWidget)
    {
        MarkNeedsBuild();
    }

    public override Control? Control => _child?.Control;

    internal override void InsertChildRenderObject(int index, Element child)
    {
        // We don't own a container; pass through but annotate flex on actual control when added to a FlexPanel
        if (Parent is FlexElement flexParent)
        {
            if (child.Control is Control c)
            {
                FlexPanel.SetFlex(c, _flex);
            }
        }

        base.InsertChildRenderObject(index, child);
    }
}