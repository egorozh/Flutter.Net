using Flutter.Foundation;
using Flutter.Gestures;
using Flutter.Rendering;
using Flutter.UI;

namespace Flutter.Widgets;

public delegate Widget IndexedWidgetBuilder(BuildContext context, int index);

public sealed class ScrollController : ChangeNotifier
{
    private readonly List<ScrollPosition> _positions = [];

    public ScrollController(double initialScrollOffset = 0.0, ScrollPhysics? physics = null)
    {
        InitialScrollOffset = initialScrollOffset;
        Physics = physics ?? new ClampingScrollPhysics();
    }

    public double InitialScrollOffset { get; }

    public ScrollPhysics Physics { get; }

    public bool HasClients => _positions.Count > 0;

    public double Offset => _positions.Count == 0 ? InitialScrollOffset : _positions[0].Pixels;

    internal ScrollPosition CreateScrollPosition(ScrollPhysics? physics = null)
    {
        return new ScrollPosition(initialPixels: InitialScrollOffset, physics: physics ?? Physics);
    }

    internal void Attach(ScrollPosition position)
    {
        if (_positions.Contains(position))
        {
            return;
        }

        _positions.Add(position);
        position.AddListener(NotifyListeners);
    }

    internal void Detach(ScrollPosition position)
    {
        if (!_positions.Remove(position))
        {
            return;
        }

        position.RemoveListener(NotifyListeners);
    }

    public void JumpTo(double value)
    {
        foreach (var position in _positions.ToArray())
        {
            position.JumpTo(value);
        }
    }

    public override void Dispose()
    {
        foreach (var position in _positions.ToArray())
        {
            position.RemoveListener(NotifyListeners);
        }

        _positions.Clear();
        base.Dispose();
    }
}

public sealed class Scrollable : StatefulWidget
{
    public Scrollable(
        Widget child,
        Axis axis = Axis.Vertical,
        ScrollController? controller = null,
        ScrollPhysics? physics = null,
        HitTestBehavior hitTestBehavior = HitTestBehavior.Opaque,
        Key? key = null) : base(key)
    {
        Child = child;
        Axis = axis;
        Controller = controller;
        Physics = physics;
        HitTestBehavior = hitTestBehavior;
    }

    public Widget Child { get; }

    public Axis Axis { get; }

    public ScrollController? Controller { get; }

    public ScrollPhysics? Physics { get; }

    public HitTestBehavior HitTestBehavior { get; }

    public override State CreateState()
    {
        return new ScrollableState();
    }

    private sealed class ScrollableState : State
    {
        private ScrollController? _fallbackController;
        private ScrollController? _attachedController;
        private ScrollPosition _position = null!;

        private Scrollable CurrentWidget => (Scrollable)Element.Widget;

        public override void InitState()
        {
            _position = AttachToController(CurrentWidget.Controller, CurrentWidget.Physics);
            _position.AddListener(HandlePositionChanged);
        }

        public override void DidUpdateWidget(StatefulWidget oldWidget)
        {
            var oldScrollable = (Scrollable)oldWidget;
            var current = CurrentWidget;
            var controllerChanged = !ReferenceEquals(oldScrollable.Controller, current.Controller);
            var physicsChanged = !ReferenceEquals(oldScrollable.Physics, current.Physics);

            if (!controllerChanged && !physicsChanged)
            {
                return;
            }

            _position.RemoveListener(HandlePositionChanged);
            _attachedController?.Detach(_position);

            _position = AttachToController(current.Controller, current.Physics);
            _position.AddListener(HandlePositionChanged);
            SetState(static () => { });
        }

        public override void Dispose()
        {
            _position.RemoveListener(HandlePositionChanged);
            _attachedController?.Detach(_position);
            _fallbackController?.Dispose();
        }

        public override Widget Build(BuildContext context)
        {
            var widget = CurrentWidget;

            return new Listener(
                behavior: widget.HitTestBehavior,
                onPointerSignal: HandlePointerSignal,
                child: new RawGestureDetector(
                    behavior: widget.HitTestBehavior,
                    onHorizontalDragUpdate: widget.Axis == Axis.Horizontal ? HandleHorizontalDragUpdate : null,
                    onVerticalDragUpdate: widget.Axis == Axis.Vertical ? HandleVerticalDragUpdate : null,
                    child: new Viewport(
                        axis: widget.Axis,
                        offsetPixels: _position.Pixels,
                        onViewportMetricsChanged: HandleViewportMetricsChanged,
                        child: widget.Child)));
        }

        private ScrollPosition AttachToController(ScrollController? providedController, ScrollPhysics? physics)
        {
            _fallbackController ??= new ScrollController();
            _attachedController = providedController ?? _fallbackController;
            var position = _attachedController.CreateScrollPosition(physics);
            _attachedController.Attach(position);
            return position;
        }

        private void HandlePositionChanged()
        {
            SetState(static () => { });
        }

        private void HandleHorizontalDragUpdate(DragUpdateDetails details)
        {
            _position.ApplyUserOffset(details.PrimaryDelta);
        }

        private void HandleVerticalDragUpdate(DragUpdateDetails details)
        {
            _position.ApplyUserOffset(details.PrimaryDelta);
        }

        private void HandlePointerSignal(PointerSignalEvent @event)
        {
            if (@event is not PointerScrollEvent scroll)
            {
                return;
            }

            var rawDelta = CurrentWidget.Axis == Axis.Vertical ? scroll.ScrollDelta.Y : scroll.ScrollDelta.X;
            _position.ApplyPointerScrollDelta(-rawDelta * 40.0);
        }

        private void HandleViewportMetricsChanged(double viewportExtent, double minScrollExtent, double maxScrollExtent)
        {
            _position.ApplyViewportDimension(viewportExtent);
            _position.ApplyContentDimensions(minScrollExtent, maxScrollExtent);
        }
    }
}

public sealed class Viewport : SingleChildRenderObjectWidget
{
    public Viewport(
        Axis axis,
        double offsetPixels,
        Widget child,
        Action<double, double, double>? onViewportMetricsChanged = null,
        Key? key = null) : base(child, key)
    {
        Axis = axis;
        OffsetPixels = offsetPixels;
        OnViewportMetricsChanged = onViewportMetricsChanged;
    }

    public Axis Axis { get; }

    public double OffsetPixels { get; }

    public Action<double, double, double>? OnViewportMetricsChanged { get; }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderViewport(
            axis: Axis,
            offsetPixels: OffsetPixels,
            onViewportMetricsChanged: OnViewportMetricsChanged);
    }

    internal override void UpdateRenderObject(BuildContext context, RenderObject renderObject)
    {
        var viewport = (RenderViewport)renderObject;
        viewport.Axis = Axis;
        viewport.OffsetPixels = OffsetPixels;
        viewport.OnViewportMetricsChanged = OnViewportMetricsChanged;
    }
}

public sealed class SingleChildScrollView : StatelessWidget
{
    public SingleChildScrollView(
        Widget child,
        Axis scrollDirection = Axis.Vertical,
        ScrollController? controller = null,
        ScrollPhysics? physics = null,
        Key? key = null) : base(key)
    {
        Child = child;
        ScrollDirection = scrollDirection;
        Controller = controller;
        Physics = physics;
    }

    public Widget Child { get; }

    public Axis ScrollDirection { get; }

    public ScrollController? Controller { get; }

    public ScrollPhysics? Physics { get; }

    public override Widget Build(BuildContext context)
    {
        return new Scrollable(
            axis: ScrollDirection,
            controller: Controller,
            physics: Physics,
            child: Child);
    }
}

public sealed class ListView : StatelessWidget
{
    private readonly IReadOnlyList<Widget>? _children;
    private readonly IndexedWidgetBuilder? _itemBuilder;
    private readonly int _itemCount;

    public ListView(
        IReadOnlyList<Widget>? children = null,
        Axis scrollDirection = Axis.Vertical,
        ScrollController? controller = null,
        ScrollPhysics? physics = null,
        Key? key = null) : base(key)
    {
        _children = children ?? [];
        ScrollDirection = scrollDirection;
        Controller = controller;
        Physics = physics;
    }

    private ListView(
        int itemCount,
        IndexedWidgetBuilder itemBuilder,
        Axis scrollDirection,
        ScrollController? controller,
        ScrollPhysics? physics,
        Key? key) : base(key)
    {
        _itemCount = itemCount;
        _itemBuilder = itemBuilder;
        ScrollDirection = scrollDirection;
        Controller = controller;
        Physics = physics;
    }

    public Axis ScrollDirection { get; }

    public ScrollController? Controller { get; }

    public ScrollPhysics? Physics { get; }

    public static ListView Builder(
        int itemCount,
        IndexedWidgetBuilder itemBuilder,
        Axis scrollDirection = Axis.Vertical,
        ScrollController? controller = null,
        ScrollPhysics? physics = null,
        Key? key = null)
    {
        return new ListView(
            itemCount: itemCount,
            itemBuilder: itemBuilder,
            scrollDirection: scrollDirection,
            controller: controller,
            physics: physics,
            key: key);
    }

    public override Widget Build(BuildContext context)
    {
        var children = _itemBuilder != null
            ? Enumerable.Range(0, _itemCount).Select(index => _itemBuilder(context, index)).ToArray()
            : _children ?? [];

        Widget viewportContent = ScrollDirection == Axis.Vertical
            ? new Column(crossAxisAlignment: CrossAxisAlignment.Stretch, children: children)
            : new Row(children: children);

        return new SingleChildScrollView(
            scrollDirection: ScrollDirection,
            controller: Controller,
            physics: Physics,
            child: viewportContent);
    }
}
