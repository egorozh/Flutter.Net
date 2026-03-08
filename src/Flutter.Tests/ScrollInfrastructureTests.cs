using Flutter.Rendering;
using Flutter.Widgets;
using Xunit;

namespace Flutter.Tests;

public sealed class ScrollInfrastructureTests
{
    [Fact]
    public void NotificationListener_ReceivesBubbledNotification()
    {
        var owner = new BuildOwner();
        var handled = 0;

        var root = new TestRootElement(
            new NotificationListener<TestScrollNotification>(
                onNotification: notification =>
                {
                    handled += notification.Value;
                    return true;
                },
                child: new NotificationEmitterWidget()));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.Equal(1, handled);
    }

    [Fact]
    public void PrimaryScrollController_Of_ResolvesControllerFromContext()
    {
        var owner = new BuildOwner();
        var injected = new ScrollController();
        ScrollController? resolved = null;

        var root = new TestRootElement(
            new PrimaryScrollController(
                controller: injected,
                child: new PrimaryControllerProbe(controller => resolved = controller)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.Same(injected, resolved);
    }

    [Fact]
    public void AutomaticKeepAlive_HandlesKeepAliveNotification()
    {
        var owner = new BuildOwner();
        var handle = new KeepAliveHandle();
        var dispatched = false;

        var root = new TestRootElement(
            new AutomaticKeepAlive(
                child: new KeepAliveNotificationEmitter(
                    handle,
                    value => dispatched = value)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.True(dispatched);
    }

    [Fact]
    public void AutomaticKeepAliveClientMixin_DispatchesWhenEnabled()
    {
        var owner = new BuildOwner();
        var notifications = 0;
        Func<KeepAliveNotification, bool> onNotification = _ =>
        {
            notifications += 1;
            return true;
        };

        var root = new TestRootElement(
            new NotificationListener<KeepAliveNotification>(
                onNotification: onNotification,
                child: new KeepAliveClientProbeWidget(keepAlive: true)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();
        Assert.Equal(1, notifications);

        root.Update(new NotificationListener<KeepAliveNotification>(
            onNotification: onNotification,
            child: new KeepAliveClientProbeWidget(keepAlive: false)));
        owner.FlushBuild();

        root.Update(new NotificationListener<KeepAliveNotification>(
            onNotification: onNotification,
            child: new KeepAliveClientProbeWidget(keepAlive: true)));
        owner.FlushBuild();

        Assert.Equal(2, notifications);
    }

    private sealed class NotificationEmitterWidget : StatelessWidget
    {
        public override Widget Build(BuildContext context)
        {
            new TestScrollNotification(1).Dispatch(context);
            return new SizedBox(width: 1, height: 1);
        }
    }

    private sealed class PrimaryControllerProbe : StatelessWidget
    {
        private readonly Action<ScrollController> _onResolved;

        public PrimaryControllerProbe(Action<ScrollController> onResolved)
        {
            _onResolved = onResolved;
        }

        public override Widget Build(BuildContext context)
        {
            _onResolved(PrimaryScrollController.Of(context));
            return new SizedBox(width: 1, height: 1);
        }
    }

    private sealed class KeepAliveNotificationEmitter : StatelessWidget
    {
        private readonly KeepAliveHandle _handle;
        private readonly Action<bool> _onDispatched;

        public KeepAliveNotificationEmitter(KeepAliveHandle handle, Action<bool> onDispatched)
        {
            _handle = handle;
            _onDispatched = onDispatched;
        }

        public override Widget Build(BuildContext context)
        {
            _onDispatched(new KeepAliveNotification(_handle).Dispatch(context));
            return new SizedBox(width: 1, height: 1);
        }
    }

    private sealed class KeepAliveClientProbeWidget : StatefulWidget
    {
        public KeepAliveClientProbeWidget(bool keepAlive)
        {
            KeepAlive = keepAlive;
        }

        public bool KeepAlive { get; }

        public override State CreateState()
        {
            return new KeepAliveClientProbeState();
        }
    }

    private sealed class KeepAliveClientProbeState : AutomaticKeepAliveClientMixin
    {
        private KeepAliveClientProbeWidget CurrentWidget => (KeepAliveClientProbeWidget)Element.Widget;

        protected override bool WantKeepAlive => CurrentWidget.KeepAlive;

        public override void DidUpdateWidget(StatefulWidget oldWidget)
        {
            base.DidUpdateWidget(oldWidget);
            UpdateKeepAlive();
        }

        public override Widget Build(BuildContext context)
        {
            return new SizedBox(width: 1, height: 1);
        }
    }

    private sealed class TestScrollNotification(int value) : ScrollNotification(
        new ScrollMetricsSnapshot(0, 0, 0, 0))
    {
        public int Value { get; } = value;
    }

    private sealed class TestRootElement : Element, IRenderObjectHost
    {
        private Element? _child;

        public TestRootElement(Widget widget) : base(widget)
        {
        }

        protected override void OnMount()
        {
            base.OnMount();
            Rebuild();
        }

        internal override void Rebuild()
        {
            Dirty = false;
            _child = UpdateChild(_child, Widget, Slot);
        }

        internal override void Update(Widget newWidget)
        {
            base.Update(newWidget);
            Rebuild();
        }

        internal override void VisitChildren(Action<Element> visitor)
        {
            if (_child != null)
            {
                visitor(_child);
            }
        }

        internal override void ForgetChild(Element child)
        {
            if (ReferenceEquals(_child, child))
            {
                _child = null;
            }
        }

        public void InsertRenderObjectChild(RenderObject child, object? slot)
        {
            if (slot != null)
            {
                throw new InvalidOperationException("TestRootElement expects null slot.");
            }
        }

        public void MoveRenderObjectChild(RenderObject child, object? oldSlot, object? newSlot)
        {
            if (!Equals(oldSlot, newSlot))
            {
                throw new InvalidOperationException("TestRootElement does not support slot moves.");
            }
        }

        public void RemoveRenderObjectChild(RenderObject child, object? slot)
        {
            if (slot != null)
            {
                throw new InvalidOperationException("TestRootElement expects null slot.");
            }
        }

        internal override void Unmount()
        {
            if (_child != null)
            {
                UnmountChild(_child);
                _child = null;
            }

            base.Unmount();
        }
    }
}
