using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.Widgets;
using Xunit;

namespace Flutter.Tests;

public sealed class InheritedNotifierTests
{
    [Fact]
    public void InheritedNotifier_NotifierUpdates_RebuildDependents()
    {
        InheritedNotifierTracker.Reset();

        var owner = new BuildOwner();
        var notifier = new ValueNotifier<int>(0);
        var stableChildTree = new Row(children: [new NotifierReaderWidget()]);

        var root = new TestRootElement(new IntInheritedNotifier(notifier, stableChildTree));
        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.Equal([0], InheritedNotifierTracker.SeenValues);

        notifier.Value = 1;
        notifier.Value = 2;
        owner.FlushBuild();

        Assert.Equal([0, 2], InheritedNotifierTracker.SeenValues);
    }

    [Fact]
    public void InheritedNotifier_NotifierIdentityChange_UnsubscribesOldAndSubscribesNew()
    {
        InheritedNotifierTracker.Reset();

        var owner = new BuildOwner();
        var notifierA = new ValueNotifier<int>(0);
        var notifierB = new ValueNotifier<int>(10);
        var stableChildTree = new Row(children: [new NotifierReaderWidget()]);

        var root = new TestRootElement(new IntInheritedNotifier(notifierA, stableChildTree));
        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        root.Update(new IntInheritedNotifier(notifierB, stableChildTree));
        owner.FlushBuild();

        Assert.Equal([0, 10], InheritedNotifierTracker.SeenValues);

        notifierA.Value = 1;
        owner.FlushBuild();
        Assert.Equal([0, 10], InheritedNotifierTracker.SeenValues);

        notifierB.Value = 11;
        owner.FlushBuild();
        Assert.Equal([0, 10, 11], InheritedNotifierTracker.SeenValues);
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

        internal override void Unmount()
        {
            if (_child != null)
            {
                UnmountChild(_child);
                _child = null;
            }

            base.Unmount();
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
    }

    private sealed class IntInheritedNotifier : InheritedNotifier<ValueNotifier<int>>
    {
        public IntInheritedNotifier(ValueNotifier<int>? notifier, Widget child) : base(notifier, child)
        {
        }
    }

    private sealed class NotifierReaderWidget : StatefulWidget
    {
        public override State CreateState() => new NotifierReaderState();
    }

    private sealed class NotifierReaderState : State
    {
        public override Widget Build(BuildContext context)
        {
            var inherited = context.DependOnInherited<IntInheritedNotifier>()
                            ?? throw new InvalidOperationException("IntInheritedNotifier not found.");
            var value = inherited.Notifier?.Value ?? -1;
            InheritedNotifierTracker.SeenValues.Add(value);
            return new SizedBox(width: 1, height: 1);
        }
    }

    private static class InheritedNotifierTracker
    {
        public static readonly List<int> SeenValues = [];

        public static void Reset()
        {
            SeenValues.Clear();
        }
    }
}
