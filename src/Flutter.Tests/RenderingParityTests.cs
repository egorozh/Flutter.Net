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
}
