using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;
using Xunit;

namespace Flutter.Tests;

public sealed class SemanticsTreeTests
{
    [Fact]
    public void Button_ProducesSemanticsNode_WithButtonFlagsAndTapAction()
    {
        var button = new RenderButton(
            label: "Increment",
            onPressed: static () => { },
            background: Colors.SteelBlue,
            foreground: Colors.White,
            fontSize: 14);

        var renderView = new RenderView
        {
            Child = button
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(200, 120));
        pipeline.FlushSemantics();

        var root = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(root);
        var semanticsButton = Assert.Single(root.Children);

        Assert.Equal("Increment", semanticsButton.Label);
        Assert.True(semanticsButton.Flags.HasFlag(SemanticsFlags.IsButton));
        Assert.True(semanticsButton.Flags.HasFlag(SemanticsFlags.IsEnabled));
        Assert.True(semanticsButton.Actions.HasFlag(SemanticsActions.Tap));
    }

    [Fact]
    public void Button_SemanticsNode_UpdatesEnabledState_WithoutChangingIdentity()
    {
        var button = new RenderButton(
            label: "Save",
            onPressed: static () => { },
            background: Colors.SeaGreen,
            foreground: Colors.White,
            fontSize: 14);

        var renderView = new RenderView
        {
            Child = button
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        var firstRoot = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(firstRoot);
        var firstNode = Assert.Single(firstRoot.Children);
        var firstId = firstNode.Id;

        button.OnPressed = null;
        pipeline.FlushSemantics();

        var updatedRoot = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(updatedRoot);
        var updatedNode = Assert.Single(updatedRoot.Children);
        Assert.Equal(firstId, updatedNode.Id);
        Assert.True(updatedNode.Flags.HasFlag(SemanticsFlags.IsButton));
        Assert.False(updatedNode.Flags.HasFlag(SemanticsFlags.IsEnabled));
        Assert.Equal(SemanticsActions.None, updatedNode.Actions);
    }

    [Fact]
    public void SemanticsOwner_PerformAction_InvokesTapHandler()
    {
        var tapCount = 0;
        var button = new RenderButton(
            label: "Tap me",
            onPressed: () => tapCount += 1,
            background: Colors.SteelBlue,
            foreground: Colors.White,
            fontSize: 14);

        var renderView = new RenderView
        {
            Child = button
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        var root = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(root);
        var buttonNode = Assert.Single(root.Children);

        Assert.True(pipeline.SemanticsOwner.PerformAction(buttonNode.Id, SemanticsActions.Tap));
        Assert.Equal(1, tapCount);

        button.OnPressed = null;
        pipeline.FlushSemantics();

        var updatedRoot = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(updatedRoot);
        var disabledNode = Assert.Single(updatedRoot.Children);

        Assert.False(pipeline.SemanticsOwner.PerformAction(disabledNode.Id, SemanticsActions.Tap));
        Assert.Equal(1, tapCount);
    }

    [Fact]
    public void MergeSemantics_MergesDescendantsIntoSingleNode()
    {
        var merge = new MergeSemanticsRenderBox(
            new RenderFlex(
                children:
                [
                    new RenderParagraph("One"),
                    new RenderParagraph("Two"),
                ],
                direction: Axis.Horizontal));

        var renderView = new RenderView
        {
            Child = merge
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(320, 120));
        pipeline.FlushSemantics();

        var root = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(root);
        var mergedNode = Assert.Single(root.Children);
        Assert.Equal("Group One Two", mergedNode.Label);
        Assert.Empty(mergedNode.Children);
    }

    [Fact]
    public void ExcludeSemantics_RemovesSubtreeFromSemanticsTree()
    {
        var excluded = new ExcludeSemanticsRenderBox(new RenderParagraph("Hidden"));
        var renderView = new RenderView
        {
            Child = excluded
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(320, 120));
        pipeline.FlushSemantics();

        var root = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(root);
        Assert.Empty(root.Children);
    }

    [Fact]
    public void DebugDumpTree_ContainsNodeActionsAndLabels()
    {
        var button = new RenderButton(
            label: "Save",
            onPressed: static () => { },
            background: Colors.SteelBlue,
            foreground: Colors.White,
            fontSize: 14);

        var renderView = new RenderView
        {
            Child = button
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        var dump = pipeline.SemanticsOwner.DebugDumpTree();
        Assert.Contains("label=\"Save\"", dump);
        Assert.Contains("actions=Tap", dump);
    }

    [Fact]
    public void SemanticsFlush_UpdatesDirtySubtree_WithoutForcingAncestorPerformSemantics()
    {
        var leaf = new CountingSemanticLeafRenderBox("A");
        var boundary = new CountingSemanticBoundaryRenderBox(leaf);
        var renderView = new RenderView
        {
            Child = boundary
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        boundary.ResetSemanticsCounter();
        leaf.ResetSemanticsCounter();

        leaf.Label = "B";
        pipeline.FlushSemantics();

        Assert.Equal(0, boundary.SemanticsUpdateCount);
        Assert.Equal(1, leaf.SemanticsUpdateCount);

        var root = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(root);
        var updatedLeaf = Assert.Single(root.Children[0].Children);
        Assert.Equal("B", updatedLeaf.Label);
    }

    [Fact]
    public void BlockingSemantics_DropsPreviouslyPaintedSiblings()
    {
        var back = new FixedSemanticBox("Back", new Size(20, 10));
        var front = new FixedSemanticBox("Front", new Size(20, 10), blocksPreviousNodes: true);
        var row = new RenderFlex(
            children: [back, front],
            direction: Axis.Horizontal);

        var renderView = new RenderView
        {
            Child = row
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        var root = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(root);
        var survivingNode = Assert.Single(root.Children);
        Assert.Equal("Front", survivingNode.Label);
    }

    [Fact]
    public void BlockingSemantics_LeavesBlockedSiblingParentDataDirty_UntilUnblocked()
    {
        var back = new FixedSemanticBox("Back", new Size(20, 10));
        var front = new MutableBlockingSemanticBox("Front", new Size(20, 10), blocksPreviousNodes: true);
        var row = new RenderFlex(
            children: [back, front],
            direction: Axis.Horizontal);

        var renderView = new RenderView
        {
            Child = row
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        Assert.True(back.SemanticsParentDataDirty);
        Assert.False(front.SemanticsParentDataDirty);

        front.BlocksPreviousNodes = false;
        pipeline.FlushSemantics();

        Assert.False(back.SemanticsParentDataDirty);
        Assert.False(front.SemanticsParentDataDirty);

        var root = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(root);
        Assert.Equal(2, root.Children.Count);
    }

    [Fact]
    public void VisitChildrenForSemantics_OmittedChildStaysParentDataDirty_UntilVisible()
    {
        var first = new FixedSemanticBox("First", new Size(20, 10));
        var second = new FixedSemanticBox("Second", new Size(20, 10));
        var row = new SelectiveSemanticsFlex(first, second)
        {
            IncludeSecondInSemantics = false
        };

        var renderView = new RenderView
        {
            Child = row
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        Assert.False(first.SemanticsParentDataDirty);
        Assert.True(second.SemanticsParentDataDirty);

        var root = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(root);
        var onlyNode = Assert.Single(root.Children);
        Assert.Equal("First", onlyNode.Label);

        row.IncludeSecondInSemantics = true;
        pipeline.FlushSemantics();

        Assert.False(second.SemanticsParentDataDirty);

        var updatedRoot = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(updatedRoot);
        Assert.Equal(2, updatedRoot.Children.Count);
    }

    [Fact]
    public void RenderTransform_AppliesTranslationToSemanticsRect()
    {
        var leaf = new FixedSemanticBox("Moved", new Size(12, 8));
        var transform = new RenderTransform(Matrix.CreateTranslation(30, 12), leaf);
        var renderView = new RenderView
        {
            Child = transform
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        var root = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(root);
        var moved = Assert.Single(root.Children);
        Assert.Equal(new Rect(30, 12, 12, 8), moved.Rect);
    }

    [Fact]
    public void RenderTransform_ChangingTransform_UpdatesSemanticsWithoutLayout()
    {
        var leaf = new FixedSemanticBox("Moved", new Size(12, 8));
        var transform = new RenderTransform(Matrix.CreateTranslation(10, 6), leaf);
        var renderView = new RenderView
        {
            Child = transform
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        var firstRoot = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(firstRoot);
        var firstNode = Assert.Single(firstRoot.Children);
        var firstId = firstNode.Id;
        Assert.Equal(new Rect(10, 6, 12, 8), firstNode.Rect);

        transform.Transform = Matrix.CreateTranslation(44, 18);
        pipeline.FlushSemantics();

        var updatedRoot = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(updatedRoot);
        var updatedNode = Assert.Single(updatedRoot.Children);
        Assert.Equal(firstId, updatedNode.Id);
        Assert.Equal(new Rect(44, 18, 12, 8), updatedNode.Rect);
    }

    [Fact]
    public void RenderClipRect_ExcludesSemanticsOutsideClip()
    {
        var leaf = new FixedSemanticBox("Hidden", new Size(12, 8));
        var transform = new RenderTransform(Matrix.CreateTranslation(40, 0), leaf);
        var clip = new RenderClipRect(transform)
        {
            ClipRect = new Rect(0, 0, 20, 20)
        };

        var renderView = new RenderView
        {
            Child = clip
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        var root = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(root);
        Assert.Empty(root.Children);
    }

    [Fact]
    public void RenderClipRect_ChangingClip_UpdatesSemanticsWithoutLayout()
    {
        var leaf = new FixedSemanticBox("Clipped", new Size(12, 8));
        var transform = new RenderTransform(Matrix.CreateTranslation(20, 0), leaf);
        var clip = new RenderClipRect(transform)
        {
            ClipRect = new Rect(0, 0, 64, 32)
        };

        var renderView = new RenderView
        {
            Child = clip
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();

        var firstRoot = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(firstRoot);
        Assert.Single(firstRoot.Children);

        clip.ClipRect = new Rect(0, 0, 8, 8);
        pipeline.FlushSemantics();

        var updatedRoot = pipeline.SemanticsOwner.RootNode;
        Assert.NotNull(updatedRoot);
        Assert.Empty(updatedRoot.Children);
    }

    [Fact]
    public void SemanticsParentDataDirty_ClearsAfterFlush_AndReappearsOnDirty()
    {
        var leaf = new FixedSemanticBox("State", new Size(12, 8));
        var renderView = new RenderView
        {
            Child = leaf
        };

        var pipeline = new PipelineOwner(renderView);
        pipeline.Attach(renderView);

        Assert.True(leaf.SemanticsParentDataDirty);

        pipeline.FlushLayout(new Size(220, 120));
        pipeline.FlushSemantics();
        Assert.False(leaf.SemanticsParentDataDirty);

        leaf.MarkNeedsSemanticsUpdate();
        Assert.True(leaf.SemanticsParentDataDirty);

        pipeline.FlushSemantics();
        Assert.False(leaf.SemanticsParentDataDirty);
    }

    private sealed class MergeSemanticsRenderBox : RenderProxyBox
    {
        public MergeSemanticsRenderBox(RenderBox child)
        {
            Child = child;
        }

        protected override void DescribeSemanticsConfiguration(SemanticsConfiguration configuration)
        {
            configuration.IsSemanticBoundary = true;
            configuration.IsMergingSemanticsOfDescendants = true;
            configuration.Label = "Group";
        }
    }

    private sealed class ExcludeSemanticsRenderBox : RenderProxyBox
    {
        public ExcludeSemanticsRenderBox(RenderBox child)
        {
            Child = child;
        }

        protected override void DescribeSemanticsConfiguration(SemanticsConfiguration configuration)
        {
            configuration.IsExcluded = true;
        }
    }

    private sealed class CountingSemanticBoundaryRenderBox : RenderProxyBox
    {
        public int SemanticsUpdateCount { get; private set; }

        public CountingSemanticBoundaryRenderBox(RenderBox child)
        {
            Child = child;
        }

        public void ResetSemanticsCounter()
        {
            SemanticsUpdateCount = 0;
        }

        protected override void PerformLayout()
        {
            Child!.Layout(Constraints, parentUsesSize: true);
            Size = Constraints.Constrain(Child.Size);
            ((BoxParentData)Child.parentData!).offset = new Point(0, 0);
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
            ctx.PaintChild(Child!, offset);
        }

        protected override void PerformSemantics()
        {
            SemanticsUpdateCount += 1;
        }

        protected override void DescribeSemanticsConfiguration(SemanticsConfiguration configuration)
        {
            configuration.IsSemanticBoundary = true;
            configuration.Label = "Boundary";
        }
    }

    private sealed class CountingSemanticLeafRenderBox : RenderBox
    {
        private string _label;

        public CountingSemanticLeafRenderBox(string label)
        {
            _label = label;
        }

        public int SemanticsUpdateCount { get; private set; }

        public string Label
        {
            get => _label;
            set
            {
                if (_label == value)
                {
                    return;
                }

                _label = value;
                MarkNeedsSemanticsUpdate();
            }
        }

        public void ResetSemanticsCounter()
        {
            SemanticsUpdateCount = 0;
        }

        protected override void PerformLayout()
        {
            Size = Constraints.Constrain(new Size(20, 10));
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
        }

        protected override void PerformSemantics()
        {
            SemanticsUpdateCount += 1;
        }

        protected override void DescribeSemanticsConfiguration(SemanticsConfiguration configuration)
        {
            configuration.IsSemanticBoundary = true;
            configuration.Label = Label;
        }
    }

    private sealed class FixedSemanticBox : RenderBox
    {
        private readonly string _label;
        private readonly Size _size;
        private readonly bool _blocksPreviousNodes;

        public FixedSemanticBox(string label, Size size, bool blocksPreviousNodes = false)
        {
            _label = label;
            _size = size;
            _blocksPreviousNodes = blocksPreviousNodes;
        }

        protected override void PerformLayout()
        {
            Size = Constraints.Constrain(_size);
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
        }

        protected override void DescribeSemanticsConfiguration(SemanticsConfiguration configuration)
        {
            configuration.IsSemanticBoundary = true;
            configuration.IsBlockingSemanticsOfPreviouslyPaintedNodes = _blocksPreviousNodes;
            configuration.Label = _label;
        }
    }

    private sealed class MutableBlockingSemanticBox : RenderBox
    {
        private readonly string _label;
        private readonly Size _size;
        private bool _blocksPreviousNodes;

        public MutableBlockingSemanticBox(string label, Size size, bool blocksPreviousNodes)
        {
            _label = label;
            _size = size;
            _blocksPreviousNodes = blocksPreviousNodes;
        }

        public bool BlocksPreviousNodes
        {
            get => _blocksPreviousNodes;
            set
            {
                if (_blocksPreviousNodes == value)
                {
                    return;
                }

                _blocksPreviousNodes = value;
                MarkNeedsSemanticsUpdate();
            }
        }

        protected override void PerformLayout()
        {
            Size = Constraints.Constrain(_size);
        }

        public override void Paint(PaintingContext ctx, Point offset)
        {
        }

        protected override void DescribeSemanticsConfiguration(SemanticsConfiguration configuration)
        {
            configuration.IsSemanticBoundary = true;
            configuration.IsBlockingSemanticsOfPreviouslyPaintedNodes = _blocksPreviousNodes;
            configuration.Label = _label;
        }
    }

    private sealed class SelectiveSemanticsFlex : RenderFlex
    {
        private bool _includeSecondInSemantics = true;

        public SelectiveSemanticsFlex(RenderBox first, RenderBox second)
            : base(children: [first, second], direction: Axis.Horizontal)
        {
        }

        public bool IncludeSecondInSemantics
        {
            get => _includeSecondInSemantics;
            set
            {
                if (_includeSecondInSemantics == value)
                {
                    return;
                }

                _includeSecondInSemantics = value;
                MarkNeedsSemanticsUpdate();
            }
        }

        internal override void VisitChildrenForSemantics(Action<RenderObject, Point, Matrix> visitor)
        {
            var firstChild = FirstChild;
            if (firstChild == null)
            {
                return;
            }

            var firstParentData = (FlexParentData)firstChild.parentData!;
            visitor(firstChild, firstParentData.offset, Matrix.Identity);

            if (!_includeSecondInSemantics)
            {
                return;
            }

            var secondChild = ChildAfter(firstChild);
            if (secondChild == null)
            {
                return;
            }

            var secondParentData = (FlexParentData)secondChild.parentData!;
            visitor(secondChild, secondParentData.offset, Matrix.Identity);
        }
    }
}
