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
}
