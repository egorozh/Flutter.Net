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
}
