using Avalonia;
using Flutter.Rendering;
using Flutter.Widgets;
using Xunit;

namespace Flutter.Tests;

public sealed class AspectRatioTests
{
    [Fact]
    public void RenderAspectRatio_UsesBoundedWidthToComputeSize()
    {
        var child = new FixedSizeRenderBox(new Size(20, 20));
        var aspectRatio = new RenderAspectRatio(2.0, child);

        aspectRatio.Layout(new BoxConstraints(MinWidth: 0, MaxWidth: 180, MinHeight: 0, MaxHeight: 120));

        Assert.Equal(new Size(180, 90), aspectRatio.Size);
        Assert.Equal(new Size(180, 90), child.Size);
    }

    [Fact]
    public void RenderAspectRatio_UsesBoundedHeight_WhenWidthIsUnbounded()
    {
        var child = new FixedSizeRenderBox(new Size(20, 20));
        var aspectRatio = new RenderAspectRatio(2.0, child);

        aspectRatio.Layout(new BoxConstraints(MinWidth: 0, MaxWidth: double.PositiveInfinity, MinHeight: 0, MaxHeight: 70));

        Assert.Equal(new Size(140, 70), aspectRatio.Size);
        Assert.Equal(new Size(140, 70), child.Size);
    }

    [Fact]
    public void RenderAspectRatio_Throws_WhenBothAxesAreUnbounded()
    {
        var aspectRatio = new RenderAspectRatio(2.0);

        Assert.Throws<InvalidOperationException>(() =>
            aspectRatio.Layout(new BoxConstraints(MinWidth: 0, MaxWidth: double.PositiveInfinity, MinHeight: 0, MaxHeight: double.PositiveInfinity)));
    }

    [Fact]
    public void AspectRatioWidget_CreatesRenderAspectRatio_AndUpdatesRatio()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new AspectRatio(
                aspectRatio: 1.5,
                child: new SizedBox(width: 10, height: 10)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var renderAspectRatio = RequireRenderObject<RenderAspectRatio>(root.ChildElement);
        Assert.Equal(1.5, renderAspectRatio.AspectRatio);

        root.Update(new AspectRatio(
            aspectRatio: 0.75,
            child: new SizedBox(width: 10, height: 10)));
        owner.FlushBuild();

        var updated = RequireRenderObject<RenderAspectRatio>(root.ChildElement);
        Assert.Same(renderAspectRatio, updated);
        Assert.Equal(0.75, updated.AspectRatio);
    }

    [Fact]
    public void SpacerWidget_UsesExpandedTightFlexParentData()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Row(children:
            [
                new SizedBox(width: 10, height: 10),
                new Spacer(flex: 3),
                new SizedBox(width: 12, height: 10),
            ]));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var row = RequireRenderObject<RenderFlex>(root.ChildElement);
        var firstChild = Assert.IsAssignableFrom<RenderBox>(row.FirstChild);
        var middleChild = Assert.IsAssignableFrom<RenderBox>(row.ChildAfter(firstChild));
        var thirdChild = Assert.IsAssignableFrom<RenderBox>(row.ChildAfter(middleChild));
        Assert.NotNull(thirdChild);

        var parentData = Assert.IsType<FlexParentData>(middleChild.parentData);
        Assert.Equal(3, parentData.flex);
        Assert.Equal(FlexFit.Tight, parentData.fit);
    }

    [Fact]
    public void Spacer_Throws_ForNonPositiveFlex()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Spacer(flex: 0));
    }

    private static T RequireRenderObject<T>(Element? element) where T : RenderObject
    {
        Assert.NotNull(element);
        Assert.NotNull(element!.RenderObject);
        return Assert.IsType<T>(element.RenderObject);
    }

    private sealed class FixedSizeRenderBox : RenderBox
    {
        private readonly Size _size;

        public FixedSizeRenderBox(Size size)
        {
            _size = size;
        }

        protected override void PerformLayout()
        {
            Size = Constraints.Constrain(_size);
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
        }
    }

    private sealed class TestRootElement : Element, IRenderObjectHost
    {
        private Element? _child;

        public TestRootElement(Widget widget) : base(widget)
        {
        }

        public Element? ChildElement => _child;

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
}
