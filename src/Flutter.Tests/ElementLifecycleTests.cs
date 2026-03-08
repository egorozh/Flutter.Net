using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.Widgets;
using Xunit;

namespace Flutter.Tests;

public sealed class ElementLifecycleTests
{
    [Fact]
    public void UpdateChildren_WithValueKeys_PreservesStateAcrossReorder_AndDisposesRemoved()
    {
        KeyedTracker.Reset();

        var owner = new BuildOwner();
        var root = new TestRootElement(new KeyedListHost([1, 2, 3]));
        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var initial = KeyedTracker.CurrentStateIdByItem.ToDictionary(static pair => pair.Key, static pair => pair.Value);

        root.Update(new KeyedListHost([3, 1, 2]));
        owner.FlushBuild();

        Assert.Equal(initial[1], KeyedTracker.CurrentStateIdByItem[1]);
        Assert.Equal(initial[2], KeyedTracker.CurrentStateIdByItem[2]);
        Assert.Equal(initial[3], KeyedTracker.CurrentStateIdByItem[3]);
        Assert.Empty(KeyedTracker.DisposedStateIds);

        root.Update(new KeyedListHost([3, 2, 4]));
        owner.FlushBuild();

        Assert.Equal(initial[3], KeyedTracker.CurrentStateIdByItem[3]);
        Assert.Equal(initial[2], KeyedTracker.CurrentStateIdByItem[2]);
        Assert.True(KeyedTracker.CurrentStateIdByItem.ContainsKey(4));
        Assert.False(KeyedTracker.CurrentStateIdByItem.ContainsKey(1));
        Assert.Contains(initial[1], KeyedTracker.DisposedStateIds);
    }

    [Fact]
    public void GlobalKey_Reparenting_TriggersDeactivateThenActivate_WithoutDisposeUntilUnmount()
    {
        LifecycleTracker.Reset();

        var owner = new BuildOwner();
        var identity = new object();

        var root = new TestRootElement(new GlobalReparentHost(moveToLeft: true, identity));
        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.Equal(["init"], LifecycleTracker.Events);

        root.Update(new GlobalReparentHost(moveToLeft: false, identity));
        owner.FlushBuild();

        var deactivateIndex = LifecycleTracker.Events.IndexOf("deactivate");
        var activateIndex = LifecycleTracker.Events.IndexOf("activate");

        Assert.True(deactivateIndex >= 0, "Expected deactivate event during GlobalKey move.");
        Assert.True(activateIndex > deactivateIndex, "Expected activate after deactivate during GlobalKey move.");
        Assert.DoesNotContain("dispose", LifecycleTracker.Events);

        root.Unmount();

        Assert.Equal(1, LifecycleTracker.Events.Count(static eventName => eventName == "dispose"));
    }

    [Fact]
    public void GlobalKey_SameSlotUpdate_DoesNotTriggerDeactivateOrActivate()
    {
        LifecycleTracker.Reset();

        var owner = new BuildOwner();
        var identity = new object();

        var root = new TestRootElement(new GlobalPlacementHost(includeGlobal: true, moveToLeft: true, identity));
        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.Equal(["init"], LifecycleTracker.Events);

        root.Update(new GlobalPlacementHost(includeGlobal: true, moveToLeft: true, identity));
        owner.FlushBuild();

        Assert.Equal(["init"], LifecycleTracker.Events);

        root.Unmount();
        Assert.Equal(1, LifecycleTracker.Events.Count(static eventName => eventName == "dispose"));
    }

    [Fact]
    public void GlobalKey_RemoveThenReinsertBeforeFlush_RetainsState()
    {
        LifecycleTracker.Reset();

        var owner = new BuildOwner();
        var identity = new object();

        var root = new TestRootElement(new GlobalPlacementHost(includeGlobal: true, moveToLeft: true, identity));
        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        root.Update(new GlobalPlacementHost(includeGlobal: false, moveToLeft: true, identity));
        root.Update(new GlobalPlacementHost(includeGlobal: true, moveToLeft: false, identity));
        owner.FlushBuild();

        Assert.Equal(1, LifecycleTracker.Events.Count(static eventName => eventName == "deactivate"));
        Assert.Equal(1, LifecycleTracker.Events.Count(static eventName => eventName == "activate"));
        Assert.DoesNotContain("dispose", LifecycleTracker.Events);

        root.Unmount();
        Assert.Equal(1, LifecycleTracker.Events.Count(static eventName => eventName == "dispose"));
    }

    [Fact]
    public void GlobalKey_TwoMovesBeforeFlush_ProcessesBothMovesWithoutDispose()
    {
        LifecycleTracker.Reset();

        var owner = new BuildOwner();
        var identity = new object();

        var root = new TestRootElement(new GlobalPlacementHost(includeGlobal: true, moveToLeft: true, identity));
        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        root.Update(new GlobalPlacementHost(includeGlobal: true, moveToLeft: false, identity));
        root.Update(new GlobalPlacementHost(includeGlobal: true, moveToLeft: true, identity));
        owner.FlushBuild();

        Assert.Equal(2, LifecycleTracker.Events.Count(static eventName => eventName == "deactivate"));
        Assert.Equal(2, LifecycleTracker.Events.Count(static eventName => eventName == "activate"));
        Assert.DoesNotContain("dispose", LifecycleTracker.Events);

        root.Unmount();
        Assert.Equal(1, LifecycleTracker.Events.Count(static eventName => eventName == "dispose"));
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

    private sealed class KeyedListHost(IReadOnlyList<int> ids) : StatelessWidget
    {
        public override Widget Build(BuildContext context)
        {
            return new Column(children: [..ids.Select(id => new TrackedItemWidget(id, new ValueKey<int>(id)))]);
        }
    }

    private sealed class TrackedItemWidget : StatefulWidget
    {
        public int ItemId { get; }

        public TrackedItemWidget(int itemId, Key key) : base(key)
        {
            ItemId = itemId;
        }

        public override State CreateState() => new TrackedItemState(ItemId);
    }

    private sealed class TrackedItemState(int itemId) : State
    {
        private readonly string _stateId = Guid.NewGuid().ToString("N");

        public override void InitState()
        {
            KeyedTracker.CurrentStateIdByItem[itemId] = _stateId;
        }

        public override void Dispose()
        {
            if (KeyedTracker.CurrentStateIdByItem.TryGetValue(itemId, out var stateId) && stateId == _stateId)
            {
                KeyedTracker.CurrentStateIdByItem.Remove(itemId);
            }

            KeyedTracker.DisposedStateIds.Add(_stateId);
        }

        public override Widget Build(BuildContext context)
        {
            return new SizedBox(width: 1, height: 1);
        }
    }

    private static class KeyedTracker
    {
        public static readonly Dictionary<int, string> CurrentStateIdByItem = [];
        public static readonly List<string> DisposedStateIds = [];

        public static void Reset()
        {
            CurrentStateIdByItem.Clear();
            DisposedStateIds.Clear();
        }
    }

    private sealed class GlobalReparentHost(bool moveToLeft, object identity) : StatelessWidget
    {
        public override Widget Build(BuildContext context)
        {
            var globalWidget = new LifecycleRecorderWidget(new GlobalObjectKey<LifecycleRecorderState>(identity));

            return new Row(
                children:
                [
                    new Expanded(moveToLeft ? globalWidget : new SizedBox(width: 1, height: 1)),
                    new Expanded(!moveToLeft ? globalWidget : new SizedBox(width: 1, height: 1)),
                ]);
        }
    }

    private sealed class GlobalPlacementHost(bool includeGlobal, bool moveToLeft, object identity) : StatelessWidget
    {
        public override Widget Build(BuildContext context)
        {
            var globalWidget = new LifecycleRecorderWidget(new GlobalObjectKey<LifecycleRecorderState>(identity));

            Widget Left()
            {
                return includeGlobal && moveToLeft ? globalWidget : new SizedBox(width: 1, height: 1);
            }

            Widget Right()
            {
                return includeGlobal && !moveToLeft ? globalWidget : new SizedBox(width: 1, height: 1);
            }

            return new Row(children: [new Expanded(Left()), new Expanded(Right())]);
        }
    }

    private sealed class LifecycleRecorderWidget : StatefulWidget
    {
        public LifecycleRecorderWidget(Key key) : base(key)
        {
        }

        public override State CreateState() => new LifecycleRecorderState();
    }

    private sealed class LifecycleRecorderState : State
    {
        public override void InitState()
        {
            LifecycleTracker.Events.Add("init");
        }

        public override void Deactivate()
        {
            LifecycleTracker.Events.Add("deactivate");
        }

        public override void Activate()
        {
            LifecycleTracker.Events.Add("activate");
        }

        public override void Dispose()
        {
            LifecycleTracker.Events.Add("dispose");
        }

        public override Widget Build(BuildContext context)
        {
            return new SizedBox(width: 1, height: 1);
        }
    }

    private static class LifecycleTracker
    {
        public static readonly List<string> Events = [];

        public static void Reset()
        {
            Events.Clear();
        }
    }
}
