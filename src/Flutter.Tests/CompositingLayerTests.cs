using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;
using Xunit;

namespace Flutter.Tests;

public sealed class CompositingLayerTests
{
    [Fact]
    public void RepaintBoundary_CreatesDedicatedOffsetLayer()
    {
        var leaf = new TestLeafRenderBox();
        var boundary = new TestRepaintBoundaryRenderBox(leaf);
        var root = new RenderView
        {
            Child = boundary
        };

        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);

        pipeline.FlushLayout(new Size(300, 200));
        pipeline.FlushCompositingBits();
        pipeline.FlushPaint();

        Assert.Single(pipeline.RootLayer.Children);
        var boundaryLayer = Assert.IsType<OffsetLayer>(pipeline.RootLayer.Children[0]);
        Assert.Single(boundaryLayer.Children);
        Assert.IsType<PictureLayer>(boundaryLayer.Children[0]);

        Assert.Equal(1, boundary.PaintCount);
        Assert.Equal(1, leaf.PaintCount);
    }

    [Fact]
    public void RepaintBoundary_DoesNotRepaintWhenOnlyParentIsDirty()
    {
        var leaf = new TestLeafRenderBox();
        var boundary = new TestRepaintBoundaryRenderBox(leaf);
        var parent = new TestParentPainterRenderBox(boundary);
        var root = new RenderView
        {
            Child = parent
        };

        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);

        pipeline.FlushLayout(new Size(300, 200));
        pipeline.FlushCompositingBits();
        pipeline.FlushPaint();

        Assert.Equal(1, parent.PaintCount);
        Assert.Equal(1, boundary.PaintCount);
        Assert.Equal(1, leaf.PaintCount);

        parent.TriggerRepaint();
        pipeline.FlushCompositingBits();
        pipeline.FlushPaint();

        Assert.Equal(2, parent.PaintCount);
        Assert.Equal(1, boundary.PaintCount);
        Assert.Equal(1, leaf.PaintCount);
    }

    private sealed class TestLeafRenderBox : RenderBox
    {
        public int PaintCount { get; private set; }

        protected override void PerformLayout()
        {
            Size = Constraints.Constrain(new Size(32, 32));
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
            PaintCount += 1;
            ctx.DrawRectangle(Brushes.CadetBlue, null, new Rect(offset, Size));
        }
    }

    private sealed class TestRepaintBoundaryRenderBox : RenderProxyBox
    {
        public int PaintCount { get; private set; }

        public TestRepaintBoundaryRenderBox(RenderBox child)
        {
            Child = child;
        }

        public override bool IsRepaintBoundary => true;

        public override void Paint(PaintingContext ctx, Point offset)
        {
            PaintCount += 1;
            base.Paint(ctx, offset);
        }
    }

    private sealed class TestParentPainterRenderBox : RenderProxyBox
    {
        public int PaintCount { get; private set; }

        public TestParentPainterRenderBox(RenderBox child)
        {
            Child = child;
        }

        public void TriggerRepaint()
        {
            MarkNeedsPaint();
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
            PaintCount += 1;
            ctx.DrawRectangle(Brushes.Transparent, null, new Rect(offset, Size));
            base.Paint(ctx, offset);
        }
    }
}
