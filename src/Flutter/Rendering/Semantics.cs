using Avalonia;

namespace Flutter.Rendering;

[Flags]
public enum SemanticsFlags
{
    None = 0,
    IsButton = 1 << 0,
    IsEnabled = 1 << 1,
}

[Flags]
public enum SemanticsActions
{
    None = 0,
    Tap = 1 << 0,
}

public sealed class SemanticsConfiguration
{
    public bool IsSemanticBoundary { get; set; }
    public string? Label { get; set; }
    public SemanticsFlags Flags { get; set; } = SemanticsFlags.None;
    public SemanticsActions Actions { get; set; } = SemanticsActions.None;
    public Rect? ExplicitRect { get; set; }
}

public sealed class SemanticsNode
{
    private readonly List<SemanticsNode> _children = [];

    internal SemanticsNode(int id)
    {
        Id = id;
    }

    public int Id { get; }
    public Rect Rect { get; internal set; }
    public string? Label { get; internal set; }
    public SemanticsFlags Flags { get; internal set; }
    public SemanticsActions Actions { get; internal set; }
    public IReadOnlyList<SemanticsNode> Children => _children;

    internal void ReplaceChildren(List<SemanticsNode> children)
    {
        _children.Clear();
        _children.AddRange(children);
    }
}

public sealed class SemanticsOwner
{
    private int _nextNodeId;
    private SemanticsNode? _syntheticRoot;

    public SemanticsNode? RootNode { get; private set; }

    internal SemanticsNode EnsureNode(RenderObject renderObject)
    {
        if (renderObject._semanticsNode != null)
        {
            return renderObject._semanticsNode;
        }

        var node = new SemanticsNode(++_nextNodeId);
        renderObject._semanticsNode = node;
        return node;
    }

    internal void UpdateRoot(List<SemanticsNode> roots)
    {
        if (roots.Count == 0)
        {
            RootNode = null;
            return;
        }

        if (roots.Count == 1)
        {
            RootNode = roots[0];
            return;
        }

        _syntheticRoot ??= new SemanticsNode(++_nextNodeId);
        _syntheticRoot.ReplaceChildren(roots);
        _syntheticRoot.Rect = UnionBounds(roots);
        _syntheticRoot.Label = null;
        _syntheticRoot.Flags = SemanticsFlags.None;
        _syntheticRoot.Actions = SemanticsActions.None;
        RootNode = _syntheticRoot;
    }

    private static Rect UnionBounds(List<SemanticsNode> nodes)
    {
        var first = nodes[0].Rect;
        var minX = first.X;
        var minY = first.Y;
        var maxX = first.Right;
        var maxY = first.Bottom;

        for (var index = 1; index < nodes.Count; index++)
        {
            var rect = nodes[index].Rect;
            minX = Math.Min(minX, rect.X);
            minY = Math.Min(minY, rect.Y);
            maxX = Math.Max(maxX, rect.Right);
            maxY = Math.Max(maxY, rect.Bottom);
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }
}
