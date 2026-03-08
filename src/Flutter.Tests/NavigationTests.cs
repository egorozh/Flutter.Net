using Flutter.Rendering;
using Flutter.Widgets;
using Xunit;

namespace Flutter.Tests;

public sealed class NavigationTests
{
    [Fact]
    public void Navigator_PushAndPop_UpdatesCurrentRoute()
    {
        var owner = new BuildOwner();
        NavigatorState? navigatorState = null;
        var currentPageName = string.Empty;

        var root = new TestRootElement(
            new Navigator(
                initialRoute: BuildRoute(
                    name: "root",
                    onBuild: name => currentPageName = name,
                    captureState: state => navigatorState ??= state)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.NotNull(navigatorState);
        Assert.Equal("root", currentPageName);
        Assert.False(navigatorState!.CanPop);

        navigatorState.Push(BuildRoute(
            name: "details",
            onBuild: name => currentPageName = name,
            captureState: _ => { }));
        owner.FlushBuild();

        Assert.Equal("details", currentPageName);
        Assert.True(navigatorState.CanPop);

        Assert.True(navigatorState.MaybePop());
        owner.FlushBuild();

        Assert.Equal("root", currentPageName);
        Assert.False(navigatorState.CanPop);
    }

    [Fact]
    public void Navigator_PushReplacement_ReplacesTopRoute()
    {
        var owner = new BuildOwner();
        NavigatorState? navigatorState = null;
        var currentPageName = string.Empty;

        var root = new TestRootElement(
            new Navigator(
                initialRoute: BuildRoute(
                    name: "root",
                    onBuild: name => currentPageName = name,
                    captureState: state => navigatorState ??= state)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.NotNull(navigatorState);
        Assert.Equal("root", currentPageName);

        navigatorState!.PushReplacement(BuildRoute(
            name: "replacement",
            onBuild: name => currentPageName = name,
            captureState: _ => { }));
        owner.FlushBuild();

        Assert.Equal("replacement", currentPageName);
        Assert.False(navigatorState.CanPop);
    }

    [Fact]
    public void NavigatorObserver_ReceivesPushPopAndReplace()
    {
        var owner = new BuildOwner();
        NavigatorState? navigatorState = null;
        var observer = new RecordingObserver();

        var root = new TestRootElement(
            new Navigator(
                initialRoute: BuildRoute(
                    name: "root",
                    onBuild: _ => { },
                    captureState: state => navigatorState ??= state),
                observers: [observer]));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        navigatorState!.Push(BuildRoute(
            name: "details",
            onBuild: _ => { },
            captureState: _ => { }));
        owner.FlushBuild();

        navigatorState.PushReplacement(BuildRoute(
            name: "replacement",
            onBuild: _ => { },
            captureState: _ => { }));
        owner.FlushBuild();

        Assert.True(navigatorState.MaybePop());
        owner.FlushBuild();

        Assert.Equal(
            [
                "push:root:null",
                "push:details:root",
                "replace:replacement:details",
                "pop:replacement:root"
            ],
            observer.Events);
    }

    [Fact]
    public void Navigator_TryHandleBackButton_PopsWhenPossible()
    {
        var owner = new BuildOwner();
        NavigatorState? navigatorState = null;
        var currentPageName = string.Empty;

        var root = new TestRootElement(
            new Navigator(
                initialRoute: BuildRoute(
                    name: "root",
                    onBuild: name => currentPageName = name,
                    captureState: state => navigatorState ??= state)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        navigatorState!.Push(BuildRoute(
            name: "details",
            onBuild: name => currentPageName = name,
            captureState: _ => { }));
        owner.FlushBuild();
        Assert.Equal("details", currentPageName);

        Assert.True(Navigator.TryHandleBackButton());
        owner.FlushBuild();

        Assert.Equal("root", currentPageName);
        Assert.False(Navigator.TryHandleBackButton());
    }

    private static Route BuildRoute(
        string name,
        Action<string> onBuild,
        Action<NavigatorState> captureState)
    {
        return new BuilderPageRoute(
            builder: context =>
            {
                captureState(Navigator.Of(context));
                onBuild(name);
                return new SizedBox(width: 1, height: 1);
            },
            settings: new RouteSettings(Name: name));
    }

    private sealed class RecordingObserver : NavigatorObserver
    {
        public List<string> Events { get; } = [];

        public override void DidPush(Route route, Route? previousRoute)
        {
            Events.Add($"push:{route.Settings.Name}:{previousRoute?.Settings.Name ?? "null"}");
        }

        public override void DidPop(Route route, Route? previousRoute)
        {
            Events.Add($"pop:{route.Settings.Name}:{previousRoute?.Settings.Name ?? "null"}");
        }

        public override void DidReplace(Route newRoute, Route? oldRoute)
        {
            Events.Add($"replace:{newRoute.Settings.Name}:{oldRoute?.Settings.Name ?? "null"}");
        }
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
