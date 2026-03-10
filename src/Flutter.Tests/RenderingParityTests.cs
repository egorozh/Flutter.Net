using Avalonia;
using Flutter.Rendering;
using Xunit;

// Dart parity source (reference): flutter/packages/flutter/lib/src/rendering/object.dart; flutter/packages/flutter/lib/src/rendering/box.dart (parity regression tests)

namespace Flutter.Tests;

public sealed class RenderingParityTests
{
    [Fact]
    public void Layout_Skips_WhenNotDirtyAndConstraintsUnchanged()
    {
        var box = new CountingRenderBox();
        var constraints = new BoxConstraints(0, 200, 0, 100);

        box.Layout(constraints);
        box.Layout(constraints);

        Assert.Equal(1, box.LayoutCount);

        box.MarkNeedsLayout();
        box.Layout(constraints);

        Assert.Equal(2, box.LayoutCount);
    }

    [Fact]
    public void Layout_Throws_ForNonNormalizedConstraints()
    {
        var box = new CountingRenderBox();
        var nonNormalized = new BoxConstraints(MinWidth: 200, MaxWidth: 100, MinHeight: 0, MaxHeight: 100);

        Assert.Throws<InvalidOperationException>(() => box.Layout(nonNormalized));
    }

    [Fact]
    public void BoxConstraints_Tighten_ClampsRequestedSizeToCurrentRange()
    {
        var constraints = new BoxConstraints(MinWidth: 10, MaxWidth: 100, MinHeight: 20, MaxHeight: 200);

        var tightened = constraints.Tighten(width: 300, height: 5);

        Assert.Equal(100, tightened.MinWidth);
        Assert.Equal(100, tightened.MaxWidth);
        Assert.Equal(20, tightened.MinHeight);
        Assert.Equal(20, tightened.MaxHeight);
        Assert.True(tightened.IsNormalized);
    }

    [Fact]
    public void RenderConstrainedBox_IgnoresNoOpAdditionalConstraintsUpdate()
    {
        var child = new CountingRenderBox();
        var constrained = new RenderConstrainedBox(
            additionalConstraints: new BoxConstraints(MinWidth: 10, MaxWidth: 120, MinHeight: 5, MaxHeight: 80),
            child: child);
        var parent = new ParentUsesSizeRenderBox(constrained);
        var root = new RenderView
        {
            Child = parent
        };

        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);

        var viewport = new Size(400, 300);
        pipeline.FlushLayout(viewport);
        Assert.Equal(1, parent.LayoutCount);

        constrained.AdditionalConstraints = new BoxConstraints(MinWidth: 10, MaxWidth: 120, MinHeight: 5, MaxHeight: 80);
        pipeline.FlushLayout(viewport);

        Assert.Equal(1, parent.LayoutCount);
    }

    [Fact]
    public void RenderPadding_IgnoresNoOpPaddingUpdate()
    {
        var child = new CountingRenderBox();
        var padding = new RenderPadding(new Thickness(8, 10, 12, 14), child);
        var parent = new ParentUsesSizeRenderBox(padding);
        var root = new RenderView
        {
            Child = parent
        };

        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);

        var viewport = new Size(300, 200);
        pipeline.FlushLayout(viewport);
        Assert.Equal(1, parent.LayoutCount);

        padding.Padding = new Thickness(8, 10, 12, 14);
        pipeline.FlushLayout(viewport);

        Assert.Equal(1, parent.LayoutCount);
    }

    [Fact]
    public void RelayoutBoundary_ChildDirty_RelayoutsOnlyBoundary()
    {
        var child = new CountingRenderBox();
        var parent = new BoundaryParentRenderBox(child);
        var root = new RenderView
        {
            Child = parent
        };

        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);

        var viewport = new Size(400, 240);

        pipeline.FlushLayout(viewport);
        Assert.Equal(1, parent.LayoutCount);
        Assert.Equal(1, child.LayoutCount);

        child.MarkNeedsLayout();
        pipeline.FlushLayout(viewport);

        Assert.Equal(1, parent.LayoutCount);
        Assert.Equal(2, child.LayoutCount);
    }

    [Fact]
    public void RequestLayoutOnRoot_WithChangedViewport_RelayoutsTree()
    {
        var child = new CountingRenderBox();
        var root = new RenderView
        {
            Child = child
        };

        var pipeline = new PipelineOwner(root);
        pipeline.Attach(root);

        pipeline.FlushLayout(new Size(300, 200));
        Assert.Equal(1, child.LayoutCount);

        pipeline.RequestLayout();
        pipeline.FlushLayout(new Size(500, 200));

        Assert.Equal(2, child.LayoutCount);
    }

    [Fact]
    public void Layout_PropagatesPerformLayoutException_AndStaysDirty()
    {
        var box = new ThrowingRenderBox();
        var constraints = new BoxConstraints(0, 200, 0, 100);

        var exception = Assert.Throws<InvalidOperationException>(() => box.Layout(constraints));
        Assert.Equal("layout boom", exception.Message);
        Assert.True(box.NeedsLayout);

        box.Layout(constraints);

        Assert.Equal(2, box.LayoutAttemptCount);
        Assert.False(box.NeedsLayout);
    }

    private sealed class CountingRenderBox : RenderBox
    {
        public int LayoutCount { get; private set; }

        protected override void PerformLayout()
        {
            LayoutCount += 1;
            Size = Constraints.Constrain(new Size(20, 10));
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
        }
    }

    private sealed class ThrowingRenderBox : RenderBox
    {
        public int LayoutAttemptCount { get; private set; }
        private bool _throwOnFirstLayout = true;

        protected override void PerformLayout()
        {
            LayoutAttemptCount += 1;

            if (_throwOnFirstLayout)
            {
                _throwOnFirstLayout = false;
                throw new InvalidOperationException("layout boom");
            }

            Size = Constraints.Constrain(new Size(20, 10));
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
        }
    }

    private sealed class BoundaryParentRenderBox : RenderBox
    {
        private RenderBox _child;
        public int LayoutCount { get; private set; }

        public BoundaryParentRenderBox(RenderBox child)
        {
            _child = child;
            AdoptChild(_child);
        }

        public override void SetupParentData(RenderObject child)
        {
            if (child.parentData is not BoxParentData)
            {
                child.parentData = new BoxParentData();
            }
        }

        public override void VisitChildren(Action<RenderObject> visitor)
        {
            visitor(_child);
        }

        protected override void PerformLayout()
        {
            LayoutCount += 1;
            _child.Layout(Constraints, parentUsesSize: false);
            Size = Constraints.Constrain(_child.Size);
            ((BoxParentData)_child.parentData!).offset = new Point(0, 0);
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
            ctx.PaintChild(_child, offset);
        }
    }

    private sealed class ParentUsesSizeRenderBox : RenderBox
    {
        private readonly RenderBox _child;
        public int LayoutCount { get; private set; }

        public ParentUsesSizeRenderBox(RenderBox child)
        {
            _child = child;
            AdoptChild(_child);
        }

        public override void SetupParentData(RenderObject child)
        {
            if (child.parentData is not BoxParentData)
            {
                child.parentData = new BoxParentData();
            }
        }

        public override void VisitChildren(Action<RenderObject> visitor)
        {
            visitor(_child);
        }

        protected override void PerformLayout()
        {
            LayoutCount += 1;
            _child.Layout(Constraints, parentUsesSize: true);
            Size = Constraints.Constrain(_child.Size);
            ((BoxParentData)_child.parentData!).offset = new Point(0, 0);
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
            ctx.PaintChild(_child, offset);
        }
    }
}
