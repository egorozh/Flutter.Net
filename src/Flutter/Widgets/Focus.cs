using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.UI;

// Dart parity source (reference): flutter/packages/flutter/lib/src/widgets/focus_manager.dart; flutter/packages/flutter/lib/src/widgets/focus_scope.dart (adapted)

namespace Flutter.Widgets;

public enum KeyEventResult
{
    Ignored,
    Handled
}

public delegate KeyEventResult FocusOnKeyEventCallback(FocusNode node, KeyEvent @event);

public class FocusNode : ChangeNotifier
{
    private bool _hasFocus;
    private bool _canRequestFocus = true;
    private bool _skipTraversal;

    public bool HasFocus => _hasFocus;

    public bool CanRequestFocus
    {
        get => _canRequestFocus;
        set
        {
            if (_canRequestFocus == value)
            {
                return;
            }

            _canRequestFocus = value;

            if (!_canRequestFocus && _hasFocus)
            {
                Unfocus();
            }
        }
    }

    public bool SkipTraversal
    {
        get => _skipTraversal;
        set => _skipTraversal = value;
    }

    public FocusOnKeyEventCallback? OnKeyEvent { get; set; }

    internal FocusManager? Manager { get; private set; }

    internal FocusScopeNode? Scope { get; private set; }

    public bool RequestFocus()
    {
        return (Manager ?? FocusManager.Instance).RequestFocus(this);
    }

    public void Unfocus()
    {
        (Manager ?? FocusManager.Instance).Unfocus(this);
    }

    internal void AttachManager(FocusManager manager)
    {
        Manager = manager;
    }

    internal void DetachManager(FocusManager manager)
    {
        if (ReferenceEquals(Manager, manager))
        {
            Manager = null;
        }
    }

    internal void AttachScope(FocusScopeNode scope)
    {
        Scope = scope;
    }

    internal void DetachScope()
    {
        Scope = null;
    }

    internal void SetHasFocus(bool value)
    {
        if (_hasFocus == value)
        {
            return;
        }

        _hasFocus = value;
        NotifyListeners();
    }

    internal KeyEventResult HandleKeyEvent(KeyEvent @event)
    {
        return OnKeyEvent?.Invoke(this, @event) ?? KeyEventResult.Ignored;
    }

    public override void Dispose()
    {
        (Manager ?? FocusManager.Instance).UnregisterNode(this);
        base.Dispose();
    }
}

public sealed class FocusScopeNode : FocusNode
{
    private readonly List<FocusNode> _members = [];

    public FocusNode? FocusedChild { get; private set; }

    internal IReadOnlyList<FocusNode> Members => _members;

    internal void AddMember(FocusNode node)
    {
        if (_members.Contains(node))
        {
            return;
        }

        _members.Add(node);
    }

    internal void RemoveMember(FocusNode node)
    {
        if (!_members.Remove(node))
        {
            return;
        }

        if (ReferenceEquals(FocusedChild, node))
        {
            FocusedChild = null;
        }
    }

    internal void SetFocusedChild(FocusNode? node)
    {
        if (node != null && !ReferenceEquals(node.Scope, this))
        {
            return;
        }

        FocusedChild = node;
    }

    internal void ResetForTests()
    {
        _members.Clear();
        FocusedChild = null;
    }
}

public sealed class FocusManager
{
    private readonly List<FocusNode> _nodes = [];
    private readonly FocusScopeNode _rootScope = new()
    {
        CanRequestFocus = false,
        SkipTraversal = true
    };

    public FocusManager()
    {
        _rootScope.AttachManager(this);
    }

    public static FocusManager Instance { get; } = new();

    public FocusNode? PrimaryFocus { get; private set; }

    public FocusScopeNode RootScope => _rootScope;

    public void RegisterNode(FocusNode node, FocusScopeNode? scope = null)
    {
        var effectiveScope = scope ?? _rootScope;

        if (node.Manager != null && !ReferenceEquals(node.Manager, this))
        {
            node.Manager.UnregisterNode(node);
        }

        if (!ReferenceEquals(effectiveScope.Manager, this))
        {
            if (effectiveScope.Manager != null)
            {
                effectiveScope.Manager.UnregisterNode(effectiveScope);
            }

            RegisterNode(effectiveScope, _rootScope);
        }

        if (_nodes.Contains(node))
        {
            MoveNodeToScope(node, effectiveScope);
            return;
        }

        _nodes.Add(node);
        node.AttachManager(this);
        MoveNodeToScope(node, effectiveScope);
    }

    public void UnregisterNode(FocusNode node)
    {
        if (!_nodes.Remove(node))
        {
            return;
        }

        node.DetachManager(this);
        node.Scope?.RemoveMember(node);
        node.DetachScope();

        if (ReferenceEquals(PrimaryFocus, node))
        {
            SetPrimaryFocus(null);
        }
    }

    public bool RequestFocus(FocusNode node)
    {
        if (!node.CanRequestFocus)
        {
            return false;
        }

        RegisterNode(node, node.Scope ?? _rootScope);

        if (ReferenceEquals(PrimaryFocus, node))
        {
            return true;
        }

        SetPrimaryFocus(node);
        return true;
    }

    public void Unfocus(FocusNode node)
    {
        if (ReferenceEquals(PrimaryFocus, node))
        {
            SetPrimaryFocus(null);
        }
    }

    public bool FocusNext()
    {
        var candidates = CollectTraversalCandidates();
        if (candidates.Count == 0)
        {
            return false;
        }

        var currentIndex = PrimaryFocus != null ? candidates.IndexOf(PrimaryFocus) : -1;
        var startIndex = currentIndex >= 0 ? currentIndex + 1 : 0;

        for (var index = startIndex; index < candidates.Count; index++)
        {
            if (RequestFocus(candidates[index]))
            {
                return true;
            }
        }

        return false;
    }

    public bool FocusPrevious()
    {
        var candidates = CollectTraversalCandidates();
        if (candidates.Count == 0)
        {
            return false;
        }

        var currentIndex = PrimaryFocus != null ? candidates.IndexOf(PrimaryFocus) : -1;
        var startIndex = currentIndex >= 0 ? currentIndex - 1 : candidates.Count - 1;

        for (var index = startIndex; index >= 0; index--)
        {
            if (RequestFocus(candidates[index]))
            {
                return true;
            }
        }

        return false;
    }

    public bool HandleKeyEvent(KeyEvent @event)
    {
        if (PrimaryFocus != null && PrimaryFocus.HandleKeyEvent(@event) == KeyEventResult.Handled)
        {
            return true;
        }

        if (!@event.IsDown)
        {
            return false;
        }

        if (!string.Equals(@event.Key, "Tab", StringComparison.Ordinal))
        {
            if (IsDirectionalNextKey(@event.Key))
            {
                return FocusNext();
            }

            if (IsDirectionalPreviousKey(@event.Key))
            {
                return FocusPrevious();
            }

            return false;
        }

        return @event.IsShiftPressed ? FocusPrevious() : FocusNext();
    }

    internal void ResetForTests()
    {
        SetPrimaryFocus(null);

        foreach (var node in _nodes.ToArray())
        {
            node.Scope?.RemoveMember(node);
            node.DetachManager(this);
            node.DetachScope();
        }

        foreach (var node in _nodes)
        {
            if (node is FocusScopeNode scopeNode)
            {
                scopeNode.ResetForTests();
            }
        }

        _nodes.Clear();
        _rootScope.ResetForTests();
    }

    private void SetPrimaryFocus(FocusNode? next)
    {
        if (ReferenceEquals(PrimaryFocus, next))
        {
            return;
        }

        var previous = PrimaryFocus;
        PrimaryFocus = next;

        if (previous != null && !ReferenceEquals(previous.Scope, next?.Scope))
        {
            previous.Scope?.SetFocusedChild(null);
        }

        next?.Scope?.SetFocusedChild(next);
        previous?.SetHasFocus(false);
        next?.SetHasFocus(true);
    }

    private void MoveNodeToScope(FocusNode node, FocusScopeNode scope)
    {
        if (ReferenceEquals(node.Scope, scope))
        {
            return;
        }

        node.Scope?.RemoveMember(node);
        node.AttachScope(scope);
        scope.AddMember(node);
    }

    private List<FocusNode> CollectTraversalCandidates()
    {
        var scope = PrimaryFocus?.Scope ?? _rootScope;
        var result = new List<FocusNode>();

        foreach (var candidate in scope.Members)
        {
            if (candidate is FocusScopeNode)
            {
                continue;
            }

            if (!candidate.CanRequestFocus || candidate.SkipTraversal)
            {
                continue;
            }

            result.Add(candidate);
        }

        return result;
    }

    private static bool IsDirectionalNextKey(string key)
    {
        return string.Equals(key, "ArrowRight", StringComparison.Ordinal)
               || string.Equals(key, "ArrowDown", StringComparison.Ordinal)
               || string.Equals(key, "Right", StringComparison.Ordinal)
               || string.Equals(key, "Down", StringComparison.Ordinal);
    }

    private static bool IsDirectionalPreviousKey(string key)
    {
        return string.Equals(key, "ArrowLeft", StringComparison.Ordinal)
               || string.Equals(key, "ArrowUp", StringComparison.Ordinal)
               || string.Equals(key, "Left", StringComparison.Ordinal)
               || string.Equals(key, "Up", StringComparison.Ordinal);
    }
}

internal sealed class FocusScopeMarker : InheritedWidget
{
    public FocusScopeMarker(
        FocusScopeNode scopeNode,
        Widget child,
        Key? key = null) : base(key)
    {
        ScopeNode = scopeNode;
        Child = child;
    }

    public FocusScopeNode ScopeNode { get; }

    public Widget Child { get; }

    public override Widget Build(BuildContext context)
    {
        return Child;
    }

    protected internal override bool UpdateShouldNotify(InheritedWidget oldWidget)
    {
        return !ReferenceEquals(((FocusScopeMarker)oldWidget).ScopeNode, ScopeNode);
    }
}

public sealed class FocusScope : StatefulWidget
{
    public FocusScope(
        Widget child,
        FocusScopeNode? focusScopeNode = null,
        bool autofocus = false,
        bool canRequestFocus = true,
        bool skipTraversal = true,
        Key? key = null) : base(key)
    {
        Child = child;
        FocusScopeNode = focusScopeNode;
        Autofocus = autofocus;
        CanRequestFocus = canRequestFocus;
        SkipTraversal = skipTraversal;
    }

    public Widget Child { get; }

    public FocusScopeNode? FocusScopeNode { get; }

    public bool Autofocus { get; }

    public bool CanRequestFocus { get; }

    public bool SkipTraversal { get; }

    public static FocusScopeNode? MaybeOf(BuildContext context)
    {
        return context.DependOnInherited<FocusScopeMarker>()?.ScopeNode;
    }

    public override State CreateState()
    {
        return new FocusScopeState();
    }

    private sealed class FocusScopeState : State
    {
        private FocusScopeNode? _scopeNode;
        private bool _ownsScopeNode;
        private bool _autofocusApplied;

        private FocusScope Widget => (FocusScope)Element.Widget;

        public override void InitState()
        {
            AttachScopeNode(Widget.FocusScopeNode);
            ApplyWidgetConfiguration();
            EnsureScopeRegistration(scope: FocusManager.Instance.RootScope);
        }

        public override void DidChangeDependencies()
        {
            EnsureScopeRegistration(ResolveParentScope());
            ApplyAutofocusIfNeeded();
        }

        public override void DidUpdateWidget(StatefulWidget oldWidget)
        {
            var oldScopeWidget = (FocusScope)oldWidget;
            var scopeWidget = Widget;

            if (!ReferenceEquals(oldScopeWidget.FocusScopeNode, scopeWidget.FocusScopeNode))
            {
                DetachScopeNode(disposeOwned: true);
                AttachScopeNode(scopeWidget.FocusScopeNode);
            }

            ApplyWidgetConfiguration();
            EnsureScopeRegistration(ResolveParentScope());

            if (!oldScopeWidget.Autofocus && scopeWidget.Autofocus)
            {
                _autofocusApplied = false;
            }

            ApplyAutofocusIfNeeded();
        }

        public override Widget Build(BuildContext context)
        {
            return new FocusScopeMarker(
                scopeNode: _scopeNode!,
                child: Widget.Child);
        }

        public override void Dispose()
        {
            DetachScopeNode(disposeOwned: true);
        }

        private FocusScopeNode ResolveParentScope()
        {
            return FocusScope.MaybeOf(Context) ?? FocusManager.Instance.RootScope;
        }

        private void AttachScopeNode(FocusScopeNode? externalNode)
        {
            _scopeNode = externalNode ?? new FocusScopeNode();
            _ownsScopeNode = externalNode is null;
        }

        private void DetachScopeNode(bool disposeOwned)
        {
            if (_scopeNode == null)
            {
                return;
            }

            FocusManager.Instance.UnregisterNode(_scopeNode);

            if (disposeOwned && _ownsScopeNode)
            {
                _scopeNode.Dispose();
            }

            _scopeNode = null;
            _ownsScopeNode = false;
            _autofocusApplied = false;
        }

        private void EnsureScopeRegistration(FocusScopeNode scope)
        {
            FocusManager.Instance.RegisterNode(_scopeNode!, scope);
        }

        private void ApplyWidgetConfiguration()
        {
            var node = _scopeNode!;
            node.CanRequestFocus = Widget.CanRequestFocus;
            node.SkipTraversal = Widget.SkipTraversal;
        }

        private void ApplyAutofocusIfNeeded()
        {
            if (!Widget.Autofocus || _autofocusApplied)
            {
                return;
            }

            _autofocusApplied = true;
            _scopeNode!.RequestFocus();
        }
    }
}

public sealed class Focus : StatefulWidget
{
    public Focus(
        Widget child,
        FocusNode? focusNode = null,
        bool autofocus = false,
        bool canRequestFocus = true,
        bool skipTraversal = false,
        FocusOnKeyEventCallback? onKeyEvent = null,
        Key? key = null) : base(key)
    {
        Child = child;
        FocusNode = focusNode;
        Autofocus = autofocus;
        CanRequestFocus = canRequestFocus;
        SkipTraversal = skipTraversal;
        OnKeyEvent = onKeyEvent;
    }

    public Widget Child { get; }

    public FocusNode? FocusNode { get; }

    public bool Autofocus { get; }

    public bool CanRequestFocus { get; }

    public bool SkipTraversal { get; }

    public FocusOnKeyEventCallback? OnKeyEvent { get; }

    public override State CreateState()
    {
        return new FocusState();
    }

    private sealed class FocusState : State
    {
        private FocusNode? _focusNode;
        private bool _ownsFocusNode;
        private bool _autofocusApplied;

        private Focus Widget => (Focus)Element.Widget;

        public override void InitState()
        {
            AttachNode(Widget.FocusNode);
            ApplyWidgetConfiguration();
            EnsureNodeRegistration(scope: FocusManager.Instance.RootScope);
        }

        public override void DidChangeDependencies()
        {
            EnsureNodeRegistration(ResolveScope());
            ApplyAutofocusIfNeeded();
        }

        public override void DidUpdateWidget(StatefulWidget oldWidget)
        {
            var oldFocusWidget = (Focus)oldWidget;
            var focusWidget = Widget;

            if (!ReferenceEquals(oldFocusWidget.FocusNode, focusWidget.FocusNode))
            {
                DetachNode(disposeOwned: true);
                AttachNode(focusWidget.FocusNode);
            }

            ApplyWidgetConfiguration();
            EnsureNodeRegistration(ResolveScope());

            if (!oldFocusWidget.Autofocus && focusWidget.Autofocus)
            {
                _autofocusApplied = false;
            }

            ApplyAutofocusIfNeeded();
        }

        public override Widget Build(BuildContext context)
        {
            return new Listener(
                child: Widget.Child,
                behavior: HitTestBehavior.Translucent,
                onPointerDown: HandlePointerDown);
        }

        public override void Dispose()
        {
            DetachNode(disposeOwned: true);
        }

        private void HandlePointerDown(PointerDownEvent @event)
        {
            if (_focusNode == null || !_focusNode.CanRequestFocus)
            {
                return;
            }

            _focusNode.RequestFocus();
        }

        private void AttachNode(FocusNode? externalNode)
        {
            _focusNode = externalNode ?? new FocusNode();
            _ownsFocusNode = externalNode is null;
        }

        private FocusScopeNode ResolveScope()
        {
            return FocusScope.MaybeOf(Context) ?? FocusManager.Instance.RootScope;
        }

        private void DetachNode(bool disposeOwned)
        {
            if (_focusNode == null)
            {
                return;
            }

            FocusManager.Instance.UnregisterNode(_focusNode);

            if (_ownsFocusNode)
            {
                _focusNode.OnKeyEvent = null;
            }

            if (disposeOwned && _ownsFocusNode)
            {
                _focusNode.Dispose();
            }

            _focusNode = null;
            _ownsFocusNode = false;
            _autofocusApplied = false;
        }

        private void EnsureNodeRegistration(FocusScopeNode scope)
        {
            FocusManager.Instance.RegisterNode(_focusNode!, scope);
        }

        private void ApplyWidgetConfiguration()
        {
            var node = _focusNode!;
            node.CanRequestFocus = Widget.CanRequestFocus;
            node.SkipTraversal = Widget.SkipTraversal;
            node.OnKeyEvent = Widget.OnKeyEvent;
        }

        private void ApplyAutofocusIfNeeded()
        {
            if (!Widget.Autofocus || _autofocusApplied)
            {
                return;
            }

            _autofocusApplied = true;
            _focusNode!.RequestFocus();
        }
    }
}
