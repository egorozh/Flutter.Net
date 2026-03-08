using Avalonia;
using Flutter.Rendering;
using Flutter.Widgets;
using Xunit;

namespace Flutter.Tests;

public sealed class ScrollPipelineTests
{
    [Fact]
    public void ScrollPosition_ClampingPhysics_ClampsToContentBounds()
    {
        var position = new ScrollPosition(initialPixels: 10);
        var notifications = 0;
        position.AddListener(() => notifications += 1);

        position.ApplyViewportDimension(120);
        position.ApplyContentDimensions(0, 60);
        position.JumpTo(1000);

        Assert.Equal(60, position.Pixels);
        Assert.True(notifications > 0);
    }

    [Fact]
    public void ScrollPosition_EndDrag_EntersBallisticActivity()
    {
        Scheduler.ResetForTests();
        try
        {
            var position = new ScrollPosition(initialPixels: 40);
            position.ApplyViewportDimension(120);
            position.ApplyContentDimensions(0, 800);

            position.BeginDrag();
            position.EndDrag(primaryPointerVelocity: -1200);
            Assert.IsType<BallisticScrollActivity>(position.Activity);
            position.Dispose();
        }
        finally
        {
            Scheduler.ResetForTests();
        }
    }

    [Fact]
    public void ScrollController_JumpTo_UpdatesAttachedPositions()
    {
        var controller = new ScrollController();
        var first = controller.CreateScrollPosition();
        var second = controller.CreateScrollPosition();

        first.ApplyViewportDimension(100);
        first.ApplyContentDimensions(0, 200);
        second.ApplyViewportDimension(100);
        second.ApplyContentDimensions(0, 50);

        controller.Attach(first);
        controller.Attach(second);
        controller.JumpTo(120);

        Assert.Equal(120, first.Pixels);
        Assert.Equal(50, second.Pixels);
    }

    [Fact]
    public void RenderViewport_OffsetsChild_AndReportsMetrics()
    {
        double viewportExtent = -1;
        double minExtent = -1;
        double maxExtent = -1;

        var child = new FixedSizeBox(new Size(80, 600));
        var viewport = new RenderViewport(
            axis: Axis.Vertical,
            offsetPixels: 50,
            onViewportMetricsChanged: (viewport, min, max) =>
            {
                viewportExtent = viewport;
                minExtent = min;
                maxExtent = max;
            },
            child: child);

        var root = new RenderView
        {
            Child = viewport
        };

        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);
        pipeline.FlushLayout(new Size(100, 200));

        var childParentData = (BoxParentData)child.parentData!;
        Assert.Equal(new Point(0, -50), childParentData.offset);
        Assert.Equal(200, viewportExtent);
        Assert.Equal(0, minExtent);
        Assert.Equal(400, maxExtent);
    }

    [Fact]
    public void RenderViewport_LaysOutMultipleSlivers_AndAggregatesScrollExtent()
    {
        double maxExtent = -1;

        var first = new RenderSliverToBoxAdapter(new FixedSizeBox(new Size(100, 120)));
        var second = new RenderSliverToBoxAdapter(new FixedSizeBox(new Size(100, 160)));
        var viewport = new RenderViewport(
            axis: Axis.Vertical,
            offsetPixels: 80,
            onViewportMetricsChanged: (_, _, max) => maxExtent = max);
        viewport.Insert(first);
        viewport.Insert(second, after: first);

        var root = new RenderView { Child = viewport };
        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);
        pipeline.FlushLayout(new Size(100, 150));

        Assert.Equal(130, maxExtent);

        var firstBoxOffset = ((BoxParentData)((RenderBox)first.Child!).parentData!).offset;
        var secondBoxOffset = ((BoxParentData)((RenderBox)second.Child!).parentData!).offset;
        Assert.Equal(new Point(0, -80), firstBoxOffset);
        Assert.Equal(new Point(0, 0), secondBoxOffset);
    }

    private sealed class FixedSizeBox : RenderBox
    {
        private readonly Size _size;

        public FixedSizeBox(Size size)
        {
            _size = size;
        }

        protected override void PerformLayout()
        {
            Size = Constraints.Constrain(_size);
        }

        protected override bool HitTestSelf(Point position)
        {
            return true;
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
        }
    }
}
