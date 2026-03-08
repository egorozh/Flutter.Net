using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Flutter.Rendering;

public abstract class Layer
{
    public ContainerLayer? Parent { get; private set; }

    internal void Attach(ContainerLayer parent)
    {
        Parent = parent;
    }

    internal void Detach()
    {
        Parent = null;
    }

    internal abstract void AddToScene(DrawingContext context, Point offset);
}

public class ContainerLayer : Layer
{
    private readonly List<Layer> _children = [];

    public IReadOnlyList<Layer> Children => _children;

    public void Append(Layer child)
    {
        if (ReferenceEquals(child.Parent, this))
        {
            return;
        }

        child.Parent?.Remove(child);
        _children.Add(child);
        child.Attach(this);
    }

    public void Remove(Layer child)
    {
        if (_children.Remove(child))
        {
            child.Detach();
        }
    }

    public void RemoveAllChildren()
    {
        foreach (var child in _children)
        {
            child.Detach();
        }

        _children.Clear();
    }

    internal override void AddToScene(DrawingContext context, Point offset)
    {
        for (var index = 0; index < _children.Count; index++)
        {
            _children[index].AddToScene(context, offset);
        }
    }
}

public sealed class OffsetLayer : ContainerLayer
{
    public Point Offset { get; set; }

    internal override void AddToScene(DrawingContext context, Point offset)
    {
        base.AddToScene(context, offset + Offset);
    }
}

public sealed class PictureLayer : Layer
{
    private readonly List<Action<DrawingContext, Point>> _commands = [];

    public bool IsEmpty => _commands.Count == 0;

    public void AddDrawCommand(Action<DrawingContext, Point> command)
    {
        _commands.Add(command);
    }

    internal override void AddToScene(DrawingContext context, Point offset)
    {
        for (var index = 0; index < _commands.Count; index++)
        {
            _commands[index](context, offset);
        }
    }
}
