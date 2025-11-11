using Avalonia;
using Avalonia.Media;

namespace Flutter.Rendering;

/// <summary>
/// An abstract set of layout constraints.
/// </summary>
public interface IConstraints
{
    /// <summary>
    /// Whether there is exactly one size possible given these constraints.
    /// </summary>
    bool IsTight { get; }

    /// <summary>
    /// Whether the constraint is expressed in a consistent manner.
    /// </summary>
    bool IsNormalized { get; }
}

/// <summary>
/// An object in the render tree.
/// </summary>
public abstract class RenderObject
{
    /// <summary>
    /// The parent of this render object in the render tree.
    /// </summary>
    public RenderObject? Parent { get; internal set; }

    /// <summary>
    /// The owner for this render object (null if unattached).
    /// </summary>
    public PipelineOwner? Owner { get; internal set; }

    /// <summary>
    /// Whether the render tree this render object belongs to is attached to a [PipelineOwner].
    /// </summary>
    public bool Attached => Owner != null;

    /// <summary>
    /// Mark this render object as attached to the given owner.
    /// </summary>
    public void Attach(PipelineOwner owner)
    {
        Owner = owner;

        // If the node was dirtied in some way while unattached, make sure to add
        // it to the appropriate dirty list now that an owner is available
        if (_needsLayout && _isRelayoutBoundary != null)
        {
            // Don't enter this block if we've never laid out at all;
            // scheduleInitialLayout() will handle it
            _needsLayout = false;
            MarkNeedsLayout();
        }

        if (_needsPaint
            //&& _layerHandle.layer != null
           )
        {
            // Don't enter this block if we've never painted at all;
            // scheduleInitialPaint() will handle it
            _needsPaint = false;
            MarkNeedsPaint();
        }
    }

    /// <summary>
    /// Mark this render object as detached from its [PipelineOwner].
    /// </summary>
    public void Detach()
    {
        Owner = null;
    }

    private bool _needsLayout = true;

    /// <summary>
    /// Whether this [RenderObject] is a known relayout boundary.
    /// </summary>
    private bool? _isRelayoutBoundary;

    /// Whether [invokeLayoutCallback] for this render object is currently running.
    public bool DebugDoingThisLayoutWithCallback { get; private set; } = false;

    private IConstraints? _constraints;

    protected IConstraints Constraints
    {
        get
        {
            if (_constraints == null)
            {
                throw new InvalidOperationException(
                    "A RenderObject does not have any constraints before it has been laid out.");
            }

            return _constraints!;
        }
    }


    /// <summary>
    /// Compute the layout for this render object.
    /// </summary>
    public virtual void Layout(BoxConstraints constraints, bool parentUsesSize = false)
    {
        _isRelayoutBoundary = !parentUsesSize || SizedByParent || constraints.IsTight || Parent == null;

        _constraints = constraints;

        if (SizedByParent)
        {
            try
            {
                PerformResize();
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        try
        {
            PerformLayout();
            //MarkNeedsSemanticsUpdate();
        }
        catch (Exception e)
        {
            // ignored
        }

        _needsLayout = false;
        MarkNeedsPaint();
    }

    protected bool SizedByParent { get; private set; } = false;

    /// <summary>
    /// Updates the render objects size using only the constraints.
    /// </summary>
    protected void PerformResize()
    {
    }

    /// <summary>
    /// Do the work of computing the layout for this render object.
    /// </summary>
    protected void PerformLayout()
    {
    }

    public virtual void VisitChildren(Action<RenderObject> visitor)
    {
    }

    /// <summary>
    /// Mark this render object's layout information as dirty, and either register
    /// this object with its [PipelineOwner], or defer to the parent, depending on
    /// whether this object is a relayout boundary or not respectively.
    /// </summary>
    public void MarkNeedsLayout()
    {
        if (_needsLayout)
        {
            return;
        }

        _needsLayout = true;

        Owner?.RequestLayout();
    }

    protected void MarkParentNeedsLayout()
    {
        _needsLayout = true;

        var parent = this.Parent!;

        if (!DebugDoingThisLayoutWithCallback)
        {
            parent.MarkNeedsLayout();
        }
    }

    /// <summary>
    /// Whether this render object repaints separately from its parent.
    /// </summary>
    public virtual bool IsRepaintBoundary => false;


    private bool _needsPaint = true;

    protected void MarkNeedsPaint()
    {
        if (_needsPaint)
        {
            return;
        }

        _needsPaint = true;

        Owner?.RequestPaint();
    }

    /// <summary>
    /// Paint this render object into the given context at the given offset.
    /// </summary>
    public abstract void Paint(DrawingContext ctx, Point offset);
}