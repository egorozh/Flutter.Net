using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.UI;
using Flutter.Widgets;
using Xunit;

namespace Flutter.Tests;

public sealed class TextWidgetTests
{
    [Fact]
    public void TextWidget_CreatesAndUpdatesRenderParagraph_WithTextLayoutOptions()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Text(
                "alpha",
                fontSize: 16,
                color: Colors.Red,
                textAlign: TextAlign.Center,
                softWrap: false,
                maxLines: 1,
                overflow: TextOverflow.Ellipsis,
                textDirection: TextDirection.Rtl));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var paragraph = RequireRenderObject<RenderParagraph>(root.ChildElement);
        Assert.Equal("alpha", paragraph.Text);
        Assert.Equal(16, paragraph.FontSize);
        Assert.Equal(TextAlign.Center, paragraph.TextAlign);
        Assert.False(paragraph.SoftWrap);
        Assert.Equal(1, paragraph.MaxLines);
        Assert.Equal(TextOverflow.Ellipsis, paragraph.Overflow);
        Assert.Equal(TextDirection.Rtl, paragraph.TextDirection);

        root.Update(new Text(
            "beta",
            fontSize: 12,
            color: Colors.Blue,
            textAlign: TextAlign.End,
            softWrap: true,
            maxLines: 3,
            overflow: TextOverflow.Clip,
            textDirection: TextDirection.Ltr));
        owner.FlushBuild();

        var updated = RequireRenderObject<RenderParagraph>(root.ChildElement);
        Assert.Same(paragraph, updated);
        Assert.Equal("beta", updated.Text);
        Assert.Equal(12, updated.FontSize);
        Assert.Equal(TextAlign.End, updated.TextAlign);
        Assert.True(updated.SoftWrap);
        Assert.Equal(3, updated.MaxLines);
        Assert.Equal(TextOverflow.Clip, updated.Overflow);
        Assert.Equal(TextDirection.Ltr, updated.TextDirection);
    }

    [Fact]
    public void RenderParagraph_UnboundedLayout_DoesNotClampWidthToArbitraryConstant()
    {
        var paragraph = new RenderParagraph(new string('W', 400))
        {
            FontSize = 14,
            SoftWrap = true
        };

        paragraph.Layout(new BoxConstraints(
            MinWidth: 0,
            MaxWidth: double.PositiveInfinity,
            MinHeight: 0,
            MaxHeight: double.PositiveInfinity));

        Assert.True(
            paragraph.Size.Width > 1000,
            $"Expected unbounded text width above 1000, got {paragraph.Size.Width:0.##}.");
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
