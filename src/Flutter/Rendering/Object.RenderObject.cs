using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
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
    internal Layer? _layer;
    internal SemanticsNode? _semanticsNode;
    private bool _needsCompositingBitsUpdate = true;
    private bool _needsSemanticsUpdate = true;
    private bool _semanticsParentDataDirty = true;
    private bool _needsCompositedLayerUpdate;
    private bool _isSemanticsBoundary;
    private bool _hasCachedSemanticsConfiguration;
    private SemanticsConfiguration? _cachedSemanticsConfiguration;
    private Matrix _semanticsTransform = Matrix.Identity;
    private Rect? _semanticsClipRect;
    private Rect? _semanticsPaintClipRect;
    private Rect _semanticsRect;
    private bool _semanticsRectClippedOut;
    private bool _semanticsHidden;
    private bool _hasBlockingSemanticsCache;
    private bool _blocksPreviousSemanticsCache;


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
        MarkNeedsCompositingBitsUpdate();
        MarkNeedsPaint();
        MarkNeedsSemanticsUpdate();

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
        MarkNeedsCompositingBitsUpdate();
        MarkNeedsSemanticsUpdate();
        child.Parent = this;

        if (Attached)
        {
            child.Attach(Owner!);
        }

        RedepthChild(child);
    }

    public void DropChild(RenderObject child)
    {
        if (!ReferenceEquals(child.Parent, this))
        {
            return;
        }

        child.Parent = null;

        if (Attached && child.Attached)
        {
            child.Detach();
        }

        MarkNeedsLayout();
        MarkNeedsCompositingBitsUpdate();
        MarkNeedsPaint();
        MarkNeedsSemanticsUpdate();
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
        if (_needsLayout)
        {
            if (Parent == null || _isRelayoutBoundary == true)
            {
                Owner.RequestLayoutFor(this);
            }
            else
            {
                Owner.RequestLayout();
            }
        }

        if (_needsCompositingBitsUpdate)
        {
            _needsCompositingBitsUpdate = false;
            MarkNeedsCompositingBitsUpdate();
        }

        if (_needsPaint)
        {
            Owner.RequestPaint();
        }

        if (_needsSemanticsUpdate)
        {
            Owner.RequestSemanticsUpdateFor(this);
        }
    }

    /// <summary>
    /// Mark this render object as detached from its [PipelineOwner].
    /// </summary>
    public void Detach()
    {
        _layer = null;
        _semanticsNode = null;
        _semanticsParentDataDirty = true;
        _hasCachedSemanticsConfiguration = false;
        _cachedSemanticsConfiguration = null;
        _semanticsRectClippedOut = false;
        _semanticsHidden = false;
        _hasBlockingSemanticsCache = false;
        _blocksPreviousSemanticsCache = false;
        Owner = null;
    }

    private bool _needsLayout = true;
    private bool _descendantNeedsLayout = true;

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
        if (!constraints.IsNormalized)
        {
            throw new InvalidOperationException("RenderObject.layout requires normalized constraints.");
        }

        if (!_needsLayout
            && !_descendantNeedsLayout
            && _constraints is BoxConstraints previousConstraints
            && previousConstraints.Equals(constraints))
        {
            return;
        }

        _isRelayoutBoundary = !parentUsesSize || SizedByParent || constraints.IsTight || Parent == null;

        _constraints = constraints;

        if (SizedByParent)
        {
            try
            {
                PerformResize();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        try
        {
            PerformLayout();
        }
        catch (Exception)
        {
            // ignored
        }

        _needsLayout = false;
        _descendantNeedsLayout = false;
        MarkNeedsCompositingBitsUpdate();
        MarkNeedsPaint();
        MarkNeedsSemanticsUpdate();
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

        if (Parent != null)
        {
            Parent.MarkDescendantNeedsLayout();

            if (_isRelayoutBoundary == true)
            {
                Owner?.RequestLayoutFor(this);
                return;
            }

            Parent.MarkNeedsLayout();
            return;
        }

        Owner?.RequestLayoutFor(this);
    }

    protected void MarkParentNeedsLayout()
    {
        _needsLayout = true;

        var parent = this.Parent!;

        parent.MarkDescendantNeedsLayout();

        if (!DebugDoingThisLayoutWithCallback)
        {
            parent.MarkNeedsLayout();
        }
    }

    private void MarkDescendantNeedsLayout()
    {
        if (_descendantNeedsLayout)
        {
            return;
        }

        _descendantNeedsLayout = true;

        if (Parent != null)
        {
            Parent.MarkDescendantNeedsLayout();
        }
    }

    public void MarkNeedsCompositingBitsUpdate()
    {
        if (_needsCompositingBitsUpdate)
        {
            return;
        }

        _needsCompositingBitsUpdate = true;
        if (Parent != null)
        {
            if (Parent._needsCompositingBitsUpdate)
            {
                return;
            }

            if ((!_wasRepaintBoundary || !IsRepaintBoundary) && !Parent.IsRepaintBoundary)
            {
                Parent.MarkNeedsCompositingBitsUpdate();
                return;
            }
        }

        Owner?.RequestCompositingBitsUpdateFor(this);
    }

    public void MarkNeedsSemanticsUpdate()
    {
        _semanticsParentDataDirty = true;
        _hasBlockingSemanticsCache = false;

        if (_needsSemanticsUpdate)
        {
            return;
        }

        _needsSemanticsUpdate = true;
        if (!Attached || Owner == null)
        {
            return;
        }

        var boundary = this;
        if (!boundary._isSemanticsBoundary)
        {
            var ancestor = Parent;
            while (ancestor != null)
            {
                ancestor.MarkSemanticsParentDataDirty();
                boundary = ancestor;
                if (boundary._isSemanticsBoundary)
                {
                    break;
                }

                ancestor = ancestor.Parent;
            }
        }

        Owner.RequestSemanticsUpdateFor(boundary);
    }

    internal void UpdateCompositingBits()
    {
        if (!_needsCompositingBitsUpdate)
        {
            return;
        }

        VisitChildren(static child => child.UpdateCompositingBits());

        var oldNeedsCompositing = NeedsCompositing;
        PerformUpdateCompositingBits();

        if (!IsRepaintBoundary && _wasRepaintBoundary)
        {
            _needsPaint = false;
            _needsCompositedLayerUpdate = false;
            Owner?.ForgetPaintFor(this);
            _needsCompositingBitsUpdate = false;
            MarkNeedsPaint();
            return;
        }

        _needsCompositingBitsUpdate = false;

        if (oldNeedsCompositing != NeedsCompositing)
        {
            MarkNeedsPaint();
        }
    }

    internal void UpdateSemanticsChildren(Matrix transform, Rect? semanticsClipRect, Rect? paintClipRect)
    {
        if (_needsSemanticsUpdate)
        {
            _needsSemanticsUpdate = false;
            PerformSemantics();
        }

        var config = new SemanticsConfiguration();
        DescribeSemanticsConfiguration(config);
        _cachedSemanticsConfiguration = config;
        _hasCachedSemanticsConfiguration = true;
        _isSemanticsBoundary = config.IsSemanticBoundary;
        _semanticsTransform = transform;
        _semanticsClipRect = semanticsClipRect;
        _semanticsPaintClipRect = paintClipRect;
        _semanticsRectClippedOut = false;
        _semanticsHidden = false;
        _semanticsParentDataDirty = false;
        _hasBlockingSemanticsCache = false;

        // Start from dirty parent data for all render children. Only children
        // visited for semantics in this pass will become clean.
        VisitChildren(static child => child.MarkSemanticsParentDataDirty());

        if (config.IsExcluded)
        {
            _semanticsNode = null;
            return;
        }

        var semanticsChildren = new List<(RenderObject child, Matrix transform, Rect? semanticsClipRect, Rect? paintClipRect)>();
        VisitChildrenForSemantics((child, childOffset, childTransform) =>
        {
            var childMatrix =
                transform
                * Matrix.CreateTranslation(childOffset.X, childOffset.Y)
                * childTransform;

            var localPaintClip = DescribeApproximatePaintClip(child);
            var localSemanticsClip = DescribeSemanticsClip(child) ?? localPaintClip;
            var semanticsClipForChild = IntersectClip(semanticsClipRect, localSemanticsClip, transform);
            var paintClipForChild = IntersectClip(paintClipRect, localPaintClip, transform);
            semanticsChildren.Add((child, childMatrix, semanticsClipForChild, paintClipForChild));
        });

        foreach (var entry in semanticsChildren)
        {
            entry.child.UpdateSemanticsChildren(entry.transform, entry.semanticsClipRect, entry.paintClipRect);
        }

        var nonBlockedChildren = new List<RenderObject>();
        foreach (var entry in semanticsChildren)
        {
            if (entry.child.BlocksPreviousSemanticsSibling())
            {
                foreach (var droppedSibling in nonBlockedChildren)
                {
                    droppedSibling.MarkSemanticsParentDataDirty();
                }

                nonBlockedChildren.Clear();
            }

            nonBlockedChildren.Add(entry.child);
        }
    }

    internal void EnsureSemanticsGeometry()
    {
        if (_semanticsParentDataDirty || !_hasCachedSemanticsConfiguration || _cachedSemanticsConfiguration == null)
        {
            return;
        }

        var config = _cachedSemanticsConfiguration;
        if (config.IsExcluded)
        {
            _semanticsRectClippedOut = true;
            _semanticsHidden = false;
            return;
        }

        if (HasOwnSemantics(config))
        {
            var localBounds = config.ExplicitRect ?? SemanticBounds;
            var transformedRect = TransformRect(_semanticsTransform, localBounds);
            _semanticsRect = transformedRect;
            _semanticsRectClippedOut = _semanticsClipRect.HasValue
                                       && !_semanticsClipRect.Value.Intersects(transformedRect);
            _semanticsHidden = !_semanticsRectClippedOut
                               && _semanticsPaintClipRect.HasValue
                               && !_semanticsPaintClipRect.Value.Intersects(transformedRect);
        }

        VisitChildrenForSemantics((child, _, _) => child.EnsureSemanticsGeometry());
    }

    internal void EnsureSemanticsNode(SemanticsOwner owner, List<SemanticsNode> output)
    {
        if (_semanticsParentDataDirty || !_hasCachedSemanticsConfiguration || _cachedSemanticsConfiguration == null)
        {
            return;
        }

        var config = _cachedSemanticsConfiguration;
        if (config.IsExcluded)
        {
            _semanticsNode = null;
            return;
        }

        var children = new List<SemanticsNode>();
        VisitChildrenForSemantics((child, _, _) =>
        {
            var childNodes = new List<SemanticsNode>();
            child.EnsureSemanticsNode(owner, childNodes);

            if (childNodes.Any(static node => node.BlocksPreviousNodes))
            {
                children.Clear();
            }

            children.AddRange(childNodes);
        });

        if (config.IsMergingSemanticsOfDescendants && children.Count > 0)
        {
            MergeChildSemanticsIntoConfiguration(config, children);
            children.Clear();
        }

        if (!HasOwnSemantics(config))
        {
            _semanticsNode = null;
            output.AddRange(children);
            return;
        }

        if (_semanticsRectClippedOut)
        {
            _semanticsNode = null;
            return;
        }

        var semanticsNode = owner.EnsureNode(this);
        semanticsNode.Rect = _semanticsRect;
        semanticsNode.Label = config.Label;
        semanticsNode.Flags = config.Flags;
        semanticsNode.Actions = config.Actions;
        semanticsNode.IsHidden = _semanticsHidden;
        semanticsNode.BlocksPreviousNodes = config.IsBlockingSemanticsOfPreviouslyPaintedNodes;
        semanticsNode.ReplaceChildren(children);
        semanticsNode.SetActionHandlers(config.ActionHandlers);

        output.Add(semanticsNode);
    }

    private static bool HasOwnSemantics(SemanticsConfiguration config)
    {
        return config.IsSemanticBoundary
               || config.IsMergingSemanticsOfDescendants
               || config.IsBlockingSemanticsOfPreviouslyPaintedNodes
               || !string.IsNullOrEmpty(config.Label)
               || config.Flags != SemanticsFlags.None
               || config.Actions != SemanticsActions.None
               || config.HasActionHandlers;
    }

    private void MarkSemanticsParentDataDirty()
    {
        _semanticsParentDataDirty = true;
        _hasBlockingSemanticsCache = false;
    }

    private bool BlocksPreviousSemanticsSibling()
    {
        if (_hasBlockingSemanticsCache)
        {
            return _blocksPreviousSemanticsCache;
        }

        if (!_hasCachedSemanticsConfiguration || _cachedSemanticsConfiguration == null)
        {
            _hasBlockingSemanticsCache = true;
            _blocksPreviousSemanticsCache = false;
            return false;
        }

        var config = _cachedSemanticsConfiguration;
        if (config.IsExcluded)
        {
            _hasBlockingSemanticsCache = true;
            _blocksPreviousSemanticsCache = false;
            return false;
        }

        if (config.IsBlockingSemanticsOfPreviouslyPaintedNodes)
        {
            _hasBlockingSemanticsCache = true;
            _blocksPreviousSemanticsCache = true;
            return true;
        }

        if (config.IsSemanticBoundary)
        {
            _hasBlockingSemanticsCache = true;
            _blocksPreviousSemanticsCache = false;
            return false;
        }

        var blocksPreviousSibling = false;
        VisitChildrenForSemantics((child, _, _) =>
        {
            if (!blocksPreviousSibling && child.BlocksPreviousSemanticsSibling())
            {
                blocksPreviousSibling = true;
            }
        });

        _hasBlockingSemanticsCache = true;
        _blocksPreviousSemanticsCache = blocksPreviousSibling;
        return blocksPreviousSibling;
    }

    private static void MergeChildSemanticsIntoConfiguration(
        SemanticsConfiguration configuration,
        List<SemanticsNode> children)
    {
        var labels = new List<string>();
        if (!string.IsNullOrWhiteSpace(configuration.Label))
        {
            labels.Add(configuration.Label);
        }

        var mergedHandlers = new Dictionary<SemanticsActions, Action>();
        foreach (var pair in configuration.ActionHandlers)
        {
            mergedHandlers[pair.Key] = pair.Value;
        }

        foreach (var child in children)
        {
            if (!string.IsNullOrWhiteSpace(child.Label))
            {
                labels.Add(child.Label);
            }

            configuration.Flags |= child.Flags;
            configuration.Actions |= child.Actions;
            child.CopyActionHandlersTo(mergedHandlers);
        }

        configuration.Label = labels.Count == 0 ? configuration.Label : string.Join(" ", labels);
        configuration.ReplaceActionHandlers(mergedHandlers);
    }

    protected virtual void PerformUpdateCompositingBits()
    {
        var needsCompositing = IsRepaintBoundary || AlwaysNeedsCompositing;

        if (!needsCompositing)
        {
            VisitChildren(child =>
            {
                if (child.NeedsCompositing || child.IsRepaintBoundary || child.AlwaysNeedsCompositing)
                {
                    needsCompositing = true;
                }
            });
        }

        NeedsCompositing = needsCompositing;
    }

    protected virtual void PerformSemantics()
    {
    }

    protected virtual void DescribeSemanticsConfiguration(SemanticsConfiguration configuration)
    {
    }

    protected virtual bool AlwaysNeedsCompositing => false;

    protected virtual Rect SemanticBounds => new Rect();

    internal virtual void VisitChildrenForSemantics(Action<RenderObject, Point, Matrix> visitor)
    {
        VisitChildren(child => visitor(child, new Point(0, 0), Matrix.Identity));
    }

    protected virtual Rect? DescribeSemanticsClip(RenderObject? child)
    {
        return null;
    }

    protected virtual Rect? DescribeApproximatePaintClip(RenderObject? child)
    {
        return null;
    }

    internal bool HasBoxConstraints => _constraints is BoxConstraints;
    internal BoxConstraints CurrentBoxConstraints => (BoxConstraints)_constraints!;
    internal bool NeedsLayoutOrDescendantNeedsLayout => _needsLayout || _descendantNeedsLayout;
    internal bool NeedsLayout => _needsLayout;
    internal bool NeedsCompositingBitsUpdate => _needsCompositingBitsUpdate;
    internal bool SemanticsParentDataDirty => _semanticsParentDataDirty;

    /// <summary>
    /// Whether this render object repaints separately from its parent.
    /// </summary>
    public virtual bool IsRepaintBoundary => false;


    private bool _needsPaint = true;
    internal bool NeedsPaint => _needsPaint;
    internal bool NeedsCompositedLayerUpdate => _needsCompositedLayerUpdate;
    internal bool NeedsCompositing { get; private set; }

    protected void MarkNeedsPaint()
    {
        if (_needsPaint)
        {
            return;
        }

        _needsPaint = true;

        if (IsRepaintBoundary && _wasRepaintBoundary)
        {
            Owner?.RequestPaintFor(this);
            return;
        }

        if (Parent != null)
        {
            Parent.MarkNeedsPaint();
            return;
        }

        Owner?.RequestPaintFor(this);
    }

    protected void MarkNeedsCompositedLayerUpdate()
    {
        if (_needsCompositedLayerUpdate || _needsPaint)
        {
            return;
        }

        _needsCompositedLayerUpdate = true;

        if (IsRepaintBoundary && _wasRepaintBoundary)
        {
            Owner?.RequestPaintFor(this);
            return;
        }

        MarkNeedsPaint();
    }

    protected virtual OffsetLayer CreateCompositedLayer(OffsetLayer? oldLayer)
    {
        return oldLayer ?? new OffsetLayer();
    }

    protected virtual void UpdateCompositedLayer(OffsetLayer layer)
    {
    }

    internal OffsetLayer EnsureCompositedLayer()
    {
        var oldLayer = _layer as OffsetLayer;
        var layer = CreateCompositedLayer(oldLayer);

        if (!ReferenceEquals(oldLayer, layer))
        {
            oldLayer?.Parent?.Remove(oldLayer);
            _layer = layer;
            _needsCompositedLayerUpdate = true;
        }

        return layer;
    }

    internal void UpdateCompositedLayerProperties()
    {
        if (!_needsCompositedLayerUpdate)
        {
            return;
        }

        if (IsRepaintBoundary && _layer is OffsetLayer layer)
        {
            UpdateCompositedLayer(layer);
        }

        _needsCompositedLayerUpdate = false;
    }

    internal void UpdateSemanticsSubtree()
    {
        if (_needsSemanticsUpdate)
        {
            _needsSemanticsUpdate = false;
            PerformSemantics();
        }

        VisitChildrenForSemantics((child, _, _) => child.UpdateSemanticsSubtree());
    }

    internal void HandleSkippedPaintingOnDetachedLayer()
    {
        if (!Attached || !IsRepaintBoundary || _layer is not OffsetLayer layer || layer.Parent != null)
        {
            return;
        }

        RenderObject? node = Parent;
        while (node != null)
        {
            if (node.IsRepaintBoundary)
            {
                node._needsPaint = true;
                node.Owner?.RequestPaintFor(node);

                if (node._layer is not OffsetLayer ancestorLayer || ancestorLayer.Parent != null)
                {
                    break;
                }
            }

            node = node.Parent;
        }
    }

    private static Rect TransformRect(Matrix transform, Rect rect)
    {
        var p1 = transform.Transform(rect.TopLeft);
        var p2 = transform.Transform(rect.TopRight);
        var p3 = transform.Transform(rect.BottomLeft);
        var p4 = transform.Transform(rect.BottomRight);

        var minX = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
        var minY = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));
        var maxX = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
        var maxY = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private static Rect? IntersectClip(Rect? inheritedClip, Rect? localClip, Matrix transform)
    {
        Rect? transformedLocalClip = null;
        if (localClip.HasValue)
        {
            transformedLocalClip = TransformRect(transform, localClip.Value);
        }

        if (!inheritedClip.HasValue)
        {
            return transformedLocalClip;
        }

        if (!transformedLocalClip.HasValue)
        {
            return inheritedClip;
        }

        var intersection = inheritedClip.Value.Intersect(transformedLocalClip.Value);
        return intersection.Width <= 0 || intersection.Height <= 0 ? null : intersection;
    }

    /// <summary>
    /// Paint this render object into the given context at the given offset.
    /// </summary>
    public abstract void Paint(PaintingContext ctx, Point offset);

    public virtual bool HitTest(BoxHitTestResult result, Point position)
    {
        return false;
    }

    public virtual void HandleEvent(PointerPressedEventArgs @event, HitTestEntry entry)
    {
    }


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

        if (_needsCompositingBitsUpdate)
        {
            throw new InvalidOperationException(
                "RenderObject.paint called before compositing bits were updated.");
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
        catch (Exception)
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
