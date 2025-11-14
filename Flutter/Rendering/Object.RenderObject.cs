using System.Diagnostics;
using Avalonia;
using Avalonia.Media;

namespace Flutter.Rendering;

public interface IRenderObject
{
}

/// <summary>
/// An object in the render tree.
/// </summary>
public abstract class RenderObject : IRenderObject
{
    internal bool _wasRepaintBoundary;

    bool _needsCompositedLayerUpdate = false;


    /// Cause the entire subtree rooted at the given [RenderObject] to be marked
    /// dirty for layout, paint, etc, so that the effects of a hot reload can be
    /// seen, or so that the effect of changing a global debug flag (such as
    /// [debugPaintSizeEnabled]) can be applied.
    ///
    /// This is called by the [RendererBinding] in response to the
    /// `ext.flutter.reassemble` hook, which is used by development tools when the
    /// application code has changed, to cause the widget tree to pick up any
    /// changed implementations.
    ///
    /// This is expensive and should not be called except during development.
    ///
    /// See also:
    ///
    ///  * [BindingBase.reassembleApplication]
    public void Reassemble()
    {
        MarkNeedsLayout();
        //MarkNeedsCompositingBitsUpdate();
        MarkNeedsPaint();
        //MarkNeedsSemanticsUpdate();

        VisitChildren(child => child.Reassemble());
    }

    // LAYOUT

    /// Data for use by the parent render object.
    ///
    /// The parent data is used by the render object that lays out this object
    /// (typically this object's parent in the render tree) to store information
    /// relevant to itself and to any other nodes who happen to know exactly what
    /// the data means. The parent data is opaque to the child.
    ///
    ///  * The parent data field must not be directly set, except by calling
    ///    [setupParentData] on the parent node.
    ///  * The parent data can be set before the child is added to the parent, by
    ///    calling [setupParentData] on the future parent node.
    ///  * The conventions for using the parent data depend on the layout protocol
    ///    used between the parent and child. For example, in box layout, the
    ///    parent data is completely opaque but in sector layout the child is
    ///    permitted to read some fields of the parent data.
    internal IParentData? parentData;


    /// Override to setup parent data correctly for your children.
    ///
    /// You can call this function to set up the parent data for child before the
    /// child is added to the parent's child list.
    public virtual void SetupParentData(RenderObject child)
    {
        //Debug.Assert(_debugCanPerformMutations);

        if (child.parentData is null)
        {
            child.parentData = new ParentData();
        }
    }

    /// The depth of this render object in the render tree.
    ///
    /// The depth of nodes in a tree monotonically increases as you traverse down
    /// the tree: a node always has a [depth] greater than its ancestors.
    /// There's no guarantee regarding depth between siblings.
    ///
    /// The [depth] of a child can be more than one greater than the [depth] of
    /// the parent, because the [depth] values are never decreased: all that
    /// matters is that it's greater than the parent. Consider a tree with a root
    /// node A, a child B, and a grandchild C. Initially, A will have [depth] 0,
    /// B [depth] 1, and C [depth] 2. If C is moved to be a child of A,
    /// sibling of B, then the numbers won't change. C's [depth] will still be 2.
    ///
    /// The depth of a node is used to ensure that nodes are processed in
    /// depth order.  The [depth] is automatically maintained by the [adoptChild]
    /// and [dropChild] methods.
    public int Depth { get; private set; }

    /// Adjust the [depth] of the given [child] to be greater than this node's own
    /// [depth].
    ///
    /// Only call this method from overrides of [redepthChildren].
    protected void RedepthChild(RenderObject child)
    {
        if (child.Depth <= Depth)
        {
            child.Depth = Depth + 1;
            child.RedepthChildren();
        }
    }

    /// Adjust the [depth] of this node's children, if any.
    ///
    /// Override this method in subclasses with child nodes to call [redepthChild]
    /// for each child. Do not call this method directly.
    protected virtual void RedepthChildren()
    {
    }


    /// <summary>
    /// The parent of this render object in the render tree.
    /// </summary>
    public RenderObject? Parent { get; private set; }

    /// <summary>
    /// Called by subclasses when they decide a render object is a child.
    /// </summary>
    public void AdoptChild(RenderObject child)
    {
        SetupParentData(child);
        MarkNeedsLayout();
        // MarkNeedsCompositingBitsUpdate();
        // MarkNeedsSemanticsUpdate();
        child.Parent = this;

        if (Attached)
        {
            child.Attach(Owner!);
        }

        RedepthChild(child);
    }

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

    protected virtual IConstraints Constraints
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
    protected virtual void PerformLayout()
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
    public abstract void Paint(PaintingContext ctx, Point offset);


    internal void _paintWithContext(PaintingContext context, Point offset)
    {
        // assert(!_debugDisposed);
        // assert(() {
        //   if (_debugDoingThisPaint) {
        //     throw FlutterError.fromParts(<DiagnosticsNode>[
        //       ErrorSummary('Tried to paint a RenderObject reentrantly.'),
        //       describeForError(
        //         'The following RenderObject was already being painted when it was '
        //         'painted again',
        //       ),
        //       ErrorDescription(
        //         'Since this typically indicates an infinite recursion, it is '
        //         'disallowed.',
        //       ),
        //     ]);
        //   }
        //   return true;
        // }());
        // If we still need layout, then that means that we were skipped in the
        // layout phase and therefore don't need painting. We might not know that
        // yet (that is, our layer might not have been detached yet), because the
        // same node that skipped us in layout is above us in the tree (obviously)
        // and therefore may not have had a chance to paint yet (since the tree
        // paints in reverse order). In particular this will happen if they have
        // a different layer, because there's a repaint boundary between us.
        if (_needsLayout)
        {
            return;
        }

        // if (!kReleaseMode && debugProfilePaintsEnabled)
        // {
        //     Map<String, String>? debugTimelineArguments;
        //     assert(() {
        //         if (debugEnhancePaintTimelineArguments)
        //         {
        //             debugTimelineArguments = toDiagnosticsNode().toTimelineArguments();
        //         }
        //
        //         return true;
        //     }
        //     ());
        //     FlutterTimeline.startSync('$runtimeType', arguments: debugTimelineArguments);
        // }

        // assert(() {
        //     if (_needsCompositingBitsUpdate)
        //     {
        //         final RenderObject? parent = this.parent;
        //         if (parent != null)
        //         {
        //             bool visitedByParent = false;
        //             parent.visitChildren((RenderObject child) {
        //                 if (child == this)
        //                 {
        //                     visitedByParent = true;
        //                 }
        //             });
        //             if (!visitedByParent)
        //             {
        //                 throw FlutterError.fromParts( < DiagnosticsNode >
        //                 [
        //                     ErrorSummary(
        //                         "A RenderObject was not visited by the parent's visitChildren "
        //                     'during paint.',
        //                     ),
        //                     parent.describeForError('The parent was'),
        //                     describeForError('The child that was not visited was'),
        //                     ErrorDescription(
        //                         'A RenderObject with children must implement visitChildren and '
        //                     'call the visitor exactly once for each child; it also should not '
        //                     'paint children that were removed with dropChild.',
        //                     ),
        //                     ErrorHint('This usually indicates an error in the Flutter framework itself.'),
        //                 ]);
        //             }
        //         }
        //
        //         throw FlutterError.fromParts( < DiagnosticsNode >
        //         [
        //             ErrorSummary(
        //                 'Tried to paint a RenderObject before its compositing bits were '
        //             'updated.',
        //             ),
        //             describeForError(
        //                 'The following RenderObject was marked as having dirty compositing '
        //             'bits at the time that it was painted',
        //             ),
        //             ErrorDescription(
        //                 'A RenderObject that still has dirty compositing bits cannot be '
        //             'painted because this indicates that the tree has not yet been '
        //             'properly configured for creating the layer tree.',
        //             ),
        //             ErrorHint('This usually indicates an error in the Flutter framework itself.'),
        //         ]);
        //     }
        //
        //     return true;
        // }
        // ());
        RenderObject? debugLastActivePaint;
        // assert(() {
        //     _debugDoingThisPaint = true;
        //     debugLastActivePaint = _debugActivePaint;
        //     _debugActivePaint = this;
        //     assert(!isRepaintBoundary || _layerHandle.layer != null);
        //     return true;
        // }
        // ());
        _needsPaint = false;

        _needsCompositedLayerUpdate = false;

        _wasRepaintBoundary = IsRepaintBoundary;

        try
        {
            Paint(context, offset);
            Debug.Assert(!_needsLayout); // check that the paint() method didn't mark us dirty again
            Debug.Assert(!_needsPaint); // check that the paint() method didn't mark us dirty again
        }
        catch (Exception e)
        {
            //_reportException('paint', e, stack);
        }

        // assert(() {
        //     debugPaint(context, offset);
        //     _debugActivePaint = debugLastActivePaint;
        //     _debugDoingThisPaint = false;
        //     return true;
        // }
        // ());
        // if (!kReleaseMode && debugProfilePaintsEnabled)
        // {
        //     FlutterTimeline.finishSync();
        // }
    }
}