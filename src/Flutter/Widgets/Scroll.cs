using Flutter.Foundation;
using Flutter.Gestures;
using Flutter.Rendering;
using Flutter.UI;
using Avalonia.Media;

namespace Flutter.Widgets;

public delegate Widget IndexedWidgetBuilder(BuildContext context, int index);

public readonly record struct ScrollMetricsSnapshot(
    double Pixels,
    double MinScrollExtent,
    double MaxScrollExtent,
    double ViewportDimension);

public abstract class ScrollNotification(ScrollMetricsSnapshot metrics) : Notification
{
    public ScrollMetricsSnapshot Metrics { get; } = metrics;
}

public sealed class ScrollStartNotification(ScrollMetricsSnapshot metrics) : ScrollNotification(metrics)
{
}

public sealed class ScrollUpdateNotification(ScrollMetricsSnapshot metrics) : ScrollNotification(metrics)
{
}

public sealed class ScrollEndNotification(ScrollMetricsSnapshot metrics) : ScrollNotification(metrics)
{
}

public sealed class PrimaryScrollController : InheritedNotifier<ScrollController>
{
    public PrimaryScrollController(
        ScrollController controller,
        Widget child,
        Key? key = null) : base(controller, child, key)
    {
    }

    public static ScrollController? MaybeOf(BuildContext context)
    {
        return context.DependOnInherited<PrimaryScrollController>()?.Notifier;
    }

    public static ScrollController Of(BuildContext context)
    {
        return MaybeOf(context)
               ?? throw new InvalidOperationException("PrimaryScrollController not found in context.");
    }
}

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

    public ScrollPosition? PrimaryPosition => _positions.Count == 0 ? null : _positions[0];

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
        Widget? child = null,
        IReadOnlyList<Widget>? slivers = null,
        Axis axis = Axis.Vertical,
        ScrollController? controller = null,
        ScrollPhysics? physics = null,
        HitTestBehavior hitTestBehavior = HitTestBehavior.Opaque,
        Key? key = null) : base(key)
    {
        Child = child;
        Slivers = slivers;
        Axis = axis;
        Controller = controller;
        Physics = physics;
        HitTestBehavior = hitTestBehavior;
    }

    public Widget? Child { get; }

    public IReadOnlyList<Widget>? Slivers { get; }

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
            var slivers = ResolveSlivers(widget);

            return new Listener(
                behavior: widget.HitTestBehavior,
                onPointerSignal: HandlePointerSignal,
                child: new RawGestureDetector(
                    behavior: widget.HitTestBehavior,
                    onHorizontalDragStart: widget.Axis == Axis.Horizontal ? HandleDragStart : null,
                    onHorizontalDragUpdate: widget.Axis == Axis.Horizontal ? HandleHorizontalDragUpdate : null,
                    onHorizontalDragEnd: widget.Axis == Axis.Horizontal ? HandleDragEnd : null,
                    onVerticalDragStart: widget.Axis == Axis.Vertical ? HandleDragStart : null,
                    onVerticalDragUpdate: widget.Axis == Axis.Vertical ? HandleVerticalDragUpdate : null,
                    onVerticalDragEnd: widget.Axis == Axis.Vertical ? HandleDragEnd : null,
                    child: new Viewport(
                        axis: widget.Axis,
                        offsetPixels: _position.Pixels,
                        slivers: slivers,
                        onViewportMetricsChanged: HandleViewportMetricsChanged)));
        }

        private IReadOnlyList<Widget> ResolveSlivers(Scrollable widget)
        {
            if (widget.Slivers is { Count: > 0 } slivers)
            {
                return slivers;
            }

            return [new SliverToBoxAdapter(widget.Child ?? new SizedBox())];
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
            new ScrollUpdateNotification(CurrentMetrics()).Dispatch(Context);
            SetState(static () => { });
        }

        private void HandleDragStart(DragStartDetails _)
        {
            _position.BeginDrag();
            new ScrollStartNotification(CurrentMetrics()).Dispatch(Context);
        }

        private void HandleHorizontalDragUpdate(DragUpdateDetails details)
        {
            _position.ApplyUserOffset(details.PrimaryDelta);
        }

        private void HandleVerticalDragUpdate(DragUpdateDetails details)
        {
            _position.ApplyUserOffset(details.PrimaryDelta);
        }

        private void HandleDragEnd(DragEndDetails details)
        {
            _position.EndDrag(details.PrimaryVelocity);
            new ScrollEndNotification(CurrentMetrics()).Dispatch(Context);
        }

        private void HandlePointerSignal(PointerSignalEvent @event)
        {
            if (@event is not PointerScrollEvent scroll)
            {
                return;
            }

            var rawDelta = CurrentWidget.Axis == Axis.Vertical ? scroll.ScrollDelta.Y : scroll.ScrollDelta.X;
            new ScrollStartNotification(CurrentMetrics()).Dispatch(Context);
            _position.ApplyPointerScrollDelta(-rawDelta * 40.0);
            new ScrollEndNotification(CurrentMetrics()).Dispatch(Context);
        }

        private void HandleViewportMetricsChanged(double viewportExtent, double minScrollExtent, double maxScrollExtent)
        {
            _position.ApplyViewportDimension(viewportExtent);
            _position.ApplyContentDimensions(minScrollExtent, maxScrollExtent);
        }

        private ScrollMetricsSnapshot CurrentMetrics()
        {
            return new ScrollMetricsSnapshot(
                Pixels: _position.Pixels,
                MinScrollExtent: _position.MinScrollExtent,
                MaxScrollExtent: _position.MaxScrollExtent,
                ViewportDimension: _position.ViewportDimension);
        }
    }
}

public sealed class Viewport : MultiChildRenderObjectWidget
{
    public Viewport(
        Axis axis,
        double offsetPixels,
        IReadOnlyList<Widget> slivers,
        Action<double, double, double>? onViewportMetricsChanged = null,
        Key? key = null) : base(slivers, key)
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

public sealed class SliverToBoxAdapter : SingleChildRenderObjectWidget
{
    public SliverToBoxAdapter(Widget child, Key? key = null) : base(child, key)
    {
    }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderSliverToBoxAdapter();
    }
}

public sealed class SliverList : SingleChildRenderObjectWidget
{
    public SliverList(Widget child, Key? key = null) : base(child, key)
    {
    }

    public static SliverList FromChildren(IReadOnlyList<Widget> children, Axis axis = Axis.Vertical, Key? key = null)
    {
        Widget content = axis == Axis.Vertical
            ? new Column(crossAxisAlignment: CrossAxisAlignment.Stretch, children: children)
            : new Row(children: children);

        return new SliverList(content, key);
    }

    internal override RenderObject CreateRenderObject(BuildContext context)
    {
        return new RenderSliverList();
    }
}

public sealed class CustomScrollView : StatelessWidget
{
    public CustomScrollView(
        IReadOnlyList<Widget> slivers,
        Axis scrollDirection = Axis.Vertical,
        ScrollController? controller = null,
        bool? primary = null,
        ScrollPhysics? physics = null,
        Key? key = null) : base(key)
    {
        Slivers = slivers;
        ScrollDirection = scrollDirection;
        Controller = controller;
        Primary = primary;
        Physics = physics;
    }

    public IReadOnlyList<Widget> Slivers { get; }

    public Axis ScrollDirection { get; }

    public ScrollController? Controller { get; }

    public bool? Primary { get; }

    public ScrollPhysics? Physics { get; }

    public override Widget Build(BuildContext context)
    {
        var usePrimary = Primary ?? (ScrollDirection == Axis.Vertical && Controller == null);
        var effectiveController = Controller;
        if (effectiveController == null && usePrimary)
        {
            effectiveController = PrimaryScrollController.MaybeOf(context);
        }

        return new Scrollable(
            slivers: Slivers,
            axis: ScrollDirection,
            controller: effectiveController,
            physics: Physics);
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
        return new CustomScrollView(
            slivers: [new SliverToBoxAdapter(Child)],
            scrollDirection: ScrollDirection,
            controller: Controller,
            physics: Physics);
    }
}

public sealed class Scrollbar : StatefulWidget
{
    public Scrollbar(
        Widget child,
        ScrollController? controller = null,
        double thickness = 4.0,
        Color? thumbColor = null,
        Key? key = null) : base(key)
    {
        Child = child;
        Controller = controller;
        Thickness = thickness;
        ThumbColor = thumbColor ?? Color.Parse("#AA5A6B82");
    }

    public Widget Child { get; }

    public ScrollController? Controller { get; }

    public double Thickness { get; }

    public Color ThumbColor { get; }

    public override State CreateState()
    {
        return new ScrollbarState();
    }

    private sealed class ScrollbarState : State
    {
        private ScrollController? _controller;

        private Scrollbar CurrentWidget => (Scrollbar)Element.Widget;

        public override void InitState()
        {
            _controller = ResolveController();
            _controller?.AddListener(HandleControllerChanged);
        }

        public override void DidUpdateWidget(StatefulWidget oldWidget)
        {
            var oldScrollbar = (Scrollbar)oldWidget;
            if (ReferenceEquals(oldScrollbar.Controller, CurrentWidget.Controller))
            {
                return;
            }

            _controller?.RemoveListener(HandleControllerChanged);
            _controller = ResolveController();
            _controller?.AddListener(HandleControllerChanged);
            SetState(static () => { });
        }

        public override void Dispose()
        {
            _controller?.RemoveListener(HandleControllerChanged);
        }

        public override Widget Build(BuildContext context)
        {
            var widget = CurrentWidget;
            var controller = _controller;
            var position = controller?.PrimaryPosition;

            if (position == null || position.MaxScrollExtent <= 0 || position.ViewportDimension <= 0)
            {
                return widget.Child;
            }

            const int totalFlex = 1000;
            var fraction = Math.Clamp(position.ViewportDimension / (position.MaxScrollExtent + position.ViewportDimension), 0.05, 1.0);
            var thumbFlex = Math.Clamp((int)(fraction * totalFlex), 1, totalFlex);
            var offsetFraction = position.MaxScrollExtent <= 0 ? 0 : Math.Clamp(position.Pixels / position.MaxScrollExtent, 0, 1);
            var beforeFlex = Math.Clamp((int)(offsetFraction * (totalFlex - thumbFlex)), 0, totalFlex - thumbFlex);
            var afterFlex = Math.Max(0, totalFlex - thumbFlex - beforeFlex);

            return new Row(
                children:
                [
                    new Expanded(child: widget.Child),
                    new SizedBox(
                        width: widget.Thickness,
                        child: new Column(
                            children:
                            [
                                new Expanded(flex: Math.Max(1, beforeFlex), child: new SizedBox()),
                                new Expanded(
                                    flex: Math.Max(1, thumbFlex),
                                    child: new ColoredBox(widget.ThumbColor)),
                                new Expanded(flex: Math.Max(1, afterFlex), child: new SizedBox()),
                            ]))
                ]);
        }

        private ScrollController? ResolveController()
        {
            return CurrentWidget.Controller ?? PrimaryScrollController.MaybeOf(Context);
        }

        private void HandleControllerChanged()
        {
            SetState(static () => { });
        }
    }
}

public sealed class ListView : StatefulWidget
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
        ItemExtent = 44;
    }

    private ListView(
        int itemCount,
        IndexedWidgetBuilder itemBuilder,
        double itemExtent,
        Axis scrollDirection,
        ScrollController? controller,
        ScrollPhysics? physics,
        Key? key) : base(key)
    {
        _itemCount = itemCount;
        _itemBuilder = itemBuilder;
        ItemExtent = itemExtent;
        ScrollDirection = scrollDirection;
        Controller = controller;
        Physics = physics;
    }

    public Axis ScrollDirection { get; }

    public ScrollController? Controller { get; }

    public ScrollPhysics? Physics { get; }

    public double ItemExtent { get; }

    public static ListView Builder(
        int itemCount,
        IndexedWidgetBuilder itemBuilder,
        double itemExtent = 44,
        Axis scrollDirection = Axis.Vertical,
        ScrollController? controller = null,
        ScrollPhysics? physics = null,
        Key? key = null)
    {
        return new ListView(
            itemCount: itemCount,
            itemBuilder: itemBuilder,
            itemExtent: itemExtent,
            scrollDirection: scrollDirection,
            controller: controller,
            physics: physics,
            key: key);
    }

    public override State CreateState()
    {
        return new ListViewState();
    }

    private sealed class ListViewState : State
    {
        private ScrollController? _fallbackController;
        private ScrollController _activeController = null!;

        private ListView CurrentWidget => (ListView)Element.Widget;

        public override void InitState()
        {
            _activeController = ResolveController(CurrentWidget.Controller);
            _activeController.AddListener(HandleControllerChanged);
        }

        public override void DidUpdateWidget(StatefulWidget oldWidget)
        {
            var oldList = (ListView)oldWidget;
            if (ReferenceEquals(oldList.Controller, CurrentWidget.Controller))
            {
                return;
            }

            _activeController.RemoveListener(HandleControllerChanged);
            _activeController = ResolveController(CurrentWidget.Controller);
            _activeController.AddListener(HandleControllerChanged);
            SetState(static () => { });
        }

        public override void Dispose()
        {
            _activeController.RemoveListener(HandleControllerChanged);
            _fallbackController?.Dispose();
        }

        public override Widget Build(BuildContext context)
        {
            var widget = CurrentWidget;
            var sliver = widget._itemBuilder == null
                ? BuildStaticSliver(widget)
                : BuildVirtualizedBuilderSliver(widget, context, _activeController);

            return new CustomScrollView(
                slivers: [sliver],
                scrollDirection: widget.ScrollDirection,
                controller: _activeController,
                physics: widget.Physics);
        }

        private ScrollController ResolveController(ScrollController? provided)
        {
            if (provided != null)
            {
                return provided;
            }

            _fallbackController ??= new ScrollController();
            return _fallbackController;
        }

        private static Widget BuildStaticSliver(ListView widget)
        {
            var children = widget._children ?? [];
            return SliverList.FromChildren(children, widget.ScrollDirection);
        }

        private static Widget BuildVirtualizedBuilderSliver(
            ListView widget,
            BuildContext context,
            ScrollController controller)
        {
            var itemBuilder = widget._itemBuilder!;
            var itemCount = Math.Max(0, widget._itemCount);
            var itemExtent = Math.Max(1, widget.ItemExtent);
            var offset = Math.Max(0, controller.Offset);
            var viewportExtent = controller.PrimaryPosition?.ViewportDimension ?? 0;
            if (viewportExtent <= 0)
            {
                viewportExtent = 480;
            }

            const int cacheItems = 2;
            var firstVisible = (int)Math.Floor(offset / itemExtent);
            var visibleCount = (int)Math.Ceiling(viewportExtent / itemExtent) + cacheItems * 2 + 1;
            var start = Math.Clamp(firstVisible - cacheItems, 0, Math.Max(0, itemCount - 1));
            var end = Math.Clamp(start + visibleCount - 1, 0, Math.Max(0, itemCount - 1));

            var beforeExtent = start * itemExtent;
            var afterExtent = Math.Max(0, (itemCount - end - 1) * itemExtent);

            var children = new List<Widget>();
            if (beforeExtent > 0)
            {
                children.Add(widget.ScrollDirection == Axis.Vertical
                    ? new SizedBox(height: beforeExtent)
                    : new SizedBox(width: beforeExtent));
            }

            for (var index = start; index <= end; index++)
            {
                var item = itemBuilder(context, index);
                item = widget.ScrollDirection == Axis.Vertical
                    ? new SizedBox(height: itemExtent, child: item)
                    : new SizedBox(width: itemExtent, child: item);
                children.Add(item);
            }

            if (afterExtent > 0)
            {
                children.Add(widget.ScrollDirection == Axis.Vertical
                    ? new SizedBox(height: afterExtent)
                    : new SizedBox(width: afterExtent));
            }

            if (children.Count == 0)
            {
                children.Add(new SizedBox(height: 0, width: 0));
            }

            return SliverList.FromChildren(children, widget.ScrollDirection);
        }

        private void HandleControllerChanged()
        {
            SetState(static () => { });
        }
    }
}
