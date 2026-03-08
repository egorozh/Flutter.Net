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

    [Fact]
    public void RenderViewport_AppliesScrollOffsetCorrection_FromSliver()
    {
        double maxExtent = -1;
        var correcting = new CorrectingSliver(
            correction: -100,
            scrollExtent: 500);
        var viewport = new RenderViewport(
            axis: Axis.Vertical,
            offsetPixels: 100,
            onViewportMetricsChanged: (_, _, max) => maxExtent = max);
        viewport.Insert(correcting);

        var root = new RenderView { Child = viewport };
        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);
        pipeline.FlushLayout(new Size(100, 200));

        Assert.Equal(0, viewport.OffsetPixels);
        Assert.Equal(300, maxExtent);
    }

    [Fact]
    public void RenderSliverList_CreatesOnlyNeededChildren_AndTrimsTrailingOnReverseScroll()
    {
        var manager = new TestSliverChildManager(childCount: 200, childExtent: 50);
        var sliverList = new RenderSliverList(manager);
        manager.AttachOwner(sliverList);

        var viewport = new RenderViewport(
            axis: Axis.Vertical,
            offsetPixels: 0);
        viewport.Insert(sliverList);

        var root = new RenderView { Child = viewport };
        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);
        pipeline.FlushLayout(new Size(100, 200));

        Assert.InRange(manager.MaxCreatedIndex, 0, 6);
        Assert.Equal(0, manager.RemoveCount);

        viewport.OffsetPixels = 450;
        pipeline.FlushLayout(new Size(100, 200));
        Assert.InRange(manager.MaxCreatedIndex, 9, 15);

        viewport.OffsetPixels = 0;
        pipeline.FlushLayout(new Size(100, 200));
        Assert.True(manager.RemoveCount > 0);
    }

    [Fact]
    public void RenderSliverList_KeepAliveChild_IsReused_WhenReturningToViewport()
    {
        var manager = new TestSliverChildManager(
            childCount: 200,
            childExtent: 50,
            keepAliveIndices: [0]);
        var sliverList = new RenderSliverList(manager);
        manager.AttachOwner(sliverList);

        var viewport = new RenderViewport(
            axis: Axis.Vertical,
            offsetPixels: 0);
        viewport.Insert(sliverList);

        var root = new RenderView { Child = viewport };
        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);

        pipeline.FlushLayout(new Size(100, 200));
        Assert.Equal(1, manager.CreateCountFor(0));
        Assert.Contains(0, ActiveIndices(sliverList));

        viewport.OffsetPixels = 600;
        pipeline.FlushLayout(new Size(100, 200));
        Assert.DoesNotContain(0, ActiveIndices(sliverList));
        Assert.DoesNotContain(0, manager.RemovedIndices);

        viewport.OffsetPixels = 0;
        pipeline.FlushLayout(new Size(100, 200));
        Assert.Equal(1, manager.CreateCountFor(0));
        Assert.Contains(0, ActiveIndices(sliverList));
    }

    private static IReadOnlyList<int> ActiveIndices(RenderSliverList sliverList)
    {
        var indices = new List<int>();
        for (var child = sliverList.FirstChild; child != null; child = sliverList.ChildAfter(child))
        {
            indices.Add(((SliverMultiBoxAdaptorParentData)child.parentData!).Index);
        }

        return indices;
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

    private sealed class CorrectingSliver : RenderSliver
    {
        private readonly double _correction;
        private readonly double _scrollExtent;
        private bool _didCorrect;

        public CorrectingSliver(double correction, double scrollExtent)
        {
            _correction = correction;
            _scrollExtent = scrollExtent;
        }

        protected override void PerformSliverLayout(SliverConstraints constraints)
        {
            if (!_didCorrect && Math.Abs(constraints.ScrollOffset) > 0.0001)
            {
                _didCorrect = true;
                Geometry = new SliverGeometry(ScrollOffsetCorrection: _correction);
                return;
            }

            var remaining = Math.Max(0, _scrollExtent - constraints.ScrollOffset);
            var paintExtent = Math.Min(remaining, constraints.RemainingPaintExtent);
            var layoutExtent = Math.Min(paintExtent, constraints.ViewportMainAxisExtent);
            Geometry = new SliverGeometry(
                ScrollExtent: _scrollExtent,
                PaintExtent: paintExtent,
                LayoutExtent: layoutExtent,
                MaxPaintExtent: _scrollExtent,
                CacheExtent: paintExtent,
                HasVisualOverflow: constraints.ScrollOffset > 0 || remaining > constraints.RemainingPaintExtent);
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
        }
    }

    private sealed class TestSliverChildManager : IRenderSliverBoxChildManager
    {
        private readonly int _childCount;
        private readonly double _childExtent;
        private readonly HashSet<int> _keepAliveIndices;
        private readonly Dictionary<int, RenderBox> _childrenByIndex = [];
        private readonly Dictionary<RenderBox, int> _indexByChild = [];
        private readonly Dictionary<int, int> _createCountByIndex = [];
        private RenderSliverList _owner = null!;

        public TestSliverChildManager(int childCount, double childExtent, IReadOnlyCollection<int>? keepAliveIndices = null)
        {
            _childCount = childCount;
            _childExtent = childExtent;
            _keepAliveIndices = keepAliveIndices != null
                ? [.. keepAliveIndices]
                : [];
        }

        public int? ChildCount => _childCount;

        public int MaxCreatedIndex { get; private set; } = -1;

        public int RemoveCount { get; private set; }
        public HashSet<int> RemovedIndices { get; } = [];

        public int CreateCountFor(int index)
        {
            return _createCountByIndex.TryGetValue(index, out var count) ? count : 0;
        }

        public void AttachOwner(RenderSliverList owner)
        {
            _owner = owner;
        }

        public bool CreateChild(int index, RenderBox? after)
        {
            if (index >= _childCount)
            {
                return false;
            }

            if (_childrenByIndex.ContainsKey(index))
            {
                return true;
            }

            var child = new FixedSizeBox(new Size(100, _childExtent));
            _childrenByIndex[index] = child;
            _indexByChild[child] = index;
            _createCountByIndex[index] = _createCountByIndex.TryGetValue(index, out var createdCount)
                ? createdCount + 1
                : 1;
            MaxCreatedIndex = Math.Max(MaxCreatedIndex, index);
            _owner.Insert(child, after);
            if (child.parentData is SliverMultiBoxAdaptorParentData parentData)
            {
                parentData.KeepAlive = _keepAliveIndices.Contains(index);
            }

            return true;
        }

        public void RemoveChild(RenderBox child)
        {
            if (!_indexByChild.TryGetValue(child, out var index))
            {
                return;
            }

            _indexByChild.Remove(child);
            _childrenByIndex.Remove(index);
            RemoveCount += 1;
            RemovedIndices.Add(index);
            _owner.Remove(child);
        }

        public void DidAdoptChild(RenderBox child)
        {
            if (!_indexByChild.TryGetValue(child, out var index))
            {
                return;
            }

            if (child.parentData is SliverMultiBoxAdaptorParentData parentData)
            {
                parentData.Index = index;
            }
        }

        public void SetDidUnderflow(bool value)
        {
        }
    }
}
