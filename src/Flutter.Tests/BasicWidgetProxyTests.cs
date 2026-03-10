using Avalonia;
using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.Widgets;
using Xunit;

namespace Flutter.Tests;

public sealed class BasicWidgetProxyTests
{
    [Fact]
    public void OpacityWidget_CreatesRenderOpacity_AndUpdatesOpacity()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Opacity(
                opacity: 1.5,
                child: new SizedBox(width: 16, height: 16)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var renderOpacity = RequireRenderObject<RenderOpacity>(root.ChildElement);
        Assert.Equal(1.0, renderOpacity.Opacity);

        root.Update(new Opacity(
            opacity: 0.25,
            child: new SizedBox(width: 16, height: 16)));
        owner.FlushBuild();

        var updatedRenderOpacity = RequireRenderObject<RenderOpacity>(root.ChildElement);
        Assert.Same(renderOpacity, updatedRenderOpacity);
        Assert.Equal(0.25, updatedRenderOpacity.Opacity);
    }

    [Fact]
    public void TransformWidget_CreatesRenderTransform_AndUpdatesTransform()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Transform(
                transform: Matrix.CreateTranslation(12, 6),
                child: new SizedBox(width: 20, height: 12)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var renderTransform = RequireRenderObject<RenderTransform>(root.ChildElement);
        Assert.Equal(Matrix.CreateTranslation(12, 6), renderTransform.Transform);

        root.Update(new Transform(
            transform: Matrix.CreateTranslation(30, 18),
            child: new SizedBox(width: 20, height: 12)));
        owner.FlushBuild();

        var updatedRenderTransform = RequireRenderObject<RenderTransform>(root.ChildElement);
        Assert.Same(renderTransform, updatedRenderTransform);
        Assert.Equal(Matrix.CreateTranslation(30, 18), updatedRenderTransform.Transform);
    }

    [Fact]
    public void ClipRectWidget_CreatesRenderClipRect_AndUpdatesClip()
    {
        var owner = new BuildOwner();
        var initialClip = new Rect(1, 2, 30, 40);
        var root = new TestRootElement(
            new ClipRect(
                clipRect: initialClip,
                child: new SizedBox(width: 40, height: 50)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var renderClipRect = RequireRenderObject<RenderClipRect>(root.ChildElement);
        Assert.Equal(initialClip, renderClipRect.ClipRect);

        var updatedClip = new Rect(4, 6, 12, 14);
        root.Update(new ClipRect(
            clipRect: updatedClip,
            child: new SizedBox(width: 40, height: 50)));
        owner.FlushBuild();

        var updatedRenderClipRect = RequireRenderObject<RenderClipRect>(root.ChildElement);
        Assert.Same(renderClipRect, updatedRenderClipRect);
        Assert.Equal(updatedClip, updatedRenderClipRect.ClipRect);
    }

    private static T RequireRenderObject<T>(Element? element) where T : RenderObject
    {
        Assert.NotNull(element);
        Assert.NotNull(element!.RenderObject);
        return Assert.IsType<T>(element.RenderObject);
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
