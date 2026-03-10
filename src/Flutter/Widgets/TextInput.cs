using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.UI;

// Dart parity source (reference): flutter/packages/flutter/lib/src/widgets/editable_text.dart; flutter/packages/flutter/lib/src/widgets/text_field.dart (adapted)

namespace Flutter.Widgets;

public readonly record struct TextSelection(int BaseOffset, int ExtentOffset)
{
    public int Start => Math.Min(BaseOffset, ExtentOffset);

    public int End => Math.Max(BaseOffset, ExtentOffset);

    public bool IsCollapsed => BaseOffset == ExtentOffset;

    public static TextSelection Collapsed(int offset)
    {
        return new TextSelection(offset, offset);
    }

    internal TextSelection Clamp(int textLength)
    {
        var clampedBaseOffset = Math.Clamp(BaseOffset, 0, textLength);
        var clampedExtentOffset = Math.Clamp(ExtentOffset, 0, textLength);
        return new TextSelection(clampedBaseOffset, clampedExtentOffset);
    }
}

public readonly record struct TextRange(int Start, int End)
{
    public bool IsCollapsed => Start == End;

    internal TextRange Clamp(int textLength)
    {
        return new TextRange(
            Start: Math.Clamp(Start, 0, textLength),
            End: Math.Clamp(End, 0, textLength));
    }
}

public sealed class TextEditingController : ChangeNotifier
{
    private string _text;
    private TextSelection _selection;
    private TextRange? _composing;

    public TextEditingController(
        string text = "",
        TextSelection? selection = null,
        TextRange? composing = null)
    {
        _text = text ?? string.Empty;
        _selection = (selection ?? TextSelection.Collapsed(_text.Length)).Clamp(_text.Length);
        _composing = composing?.Clamp(_text.Length);
    }

    public string Text
    {
        get => _text;
        set
        {
            var next = value ?? string.Empty;
            SetValue(
                text: next,
                selection: TextSelection.Collapsed(next.Length),
                composing: null);
        }
    }

    public TextSelection Selection
    {
        get => _selection;
        set => SetValue(text: _text, selection: value, composing: _composing);
    }

    public TextRange? Composing
    {
        get => _composing;
        set => SetValue(text: _text, selection: _selection, composing: value);
    }

    public void SetValue(
        string text,
        TextSelection? selection = null,
        TextRange? composing = null)
    {
        var normalizedText = text ?? string.Empty;
        var normalizedSelection = (selection ?? _selection).Clamp(normalizedText.Length);
        var normalizedComposing = composing?.Clamp(normalizedText.Length);
        if (normalizedComposing.HasValue && normalizedComposing.Value.IsCollapsed)
        {
            normalizedComposing = null;
        }

        if (string.Equals(_text, normalizedText, StringComparison.Ordinal)
            && _selection.Equals(normalizedSelection)
            && Nullable.Equals(_composing, normalizedComposing))
        {
            return;
        }

        _text = normalizedText;
        _selection = normalizedSelection;
        _composing = normalizedComposing;
        NotifyListeners();
    }

    public bool SelectAll()
    {
        return UpdateSelection(new TextSelection(0, _text.Length));
    }

    public bool MoveCaretLeft(bool extendSelection = false)
    {
        if (!extendSelection && !_selection.IsCollapsed)
        {
            return UpdateSelection(TextSelection.Collapsed(_selection.Start));
        }

        var nextExtentOffset = Math.Max(0, _selection.ExtentOffset - 1);
        var nextSelection = extendSelection
            ? new TextSelection(_selection.BaseOffset, nextExtentOffset)
            : TextSelection.Collapsed(nextExtentOffset);
        return UpdateSelection(nextSelection);
    }

    public bool MoveCaretRight(bool extendSelection = false)
    {
        if (!extendSelection && !_selection.IsCollapsed)
        {
            return UpdateSelection(TextSelection.Collapsed(_selection.End));
        }

        var nextExtentOffset = Math.Min(_text.Length, _selection.ExtentOffset + 1);
        var nextSelection = extendSelection
            ? new TextSelection(_selection.BaseOffset, nextExtentOffset)
            : TextSelection.Collapsed(nextExtentOffset);
        return UpdateSelection(nextSelection);
    }

    public bool MoveCaretToStart(bool extendSelection = false)
    {
        var nextSelection = extendSelection
            ? new TextSelection(_selection.BaseOffset, 0)
            : TextSelection.Collapsed(0);
        return UpdateSelection(nextSelection);
    }

    public bool MoveCaretToEnd(bool extendSelection = false)
    {
        var nextSelection = extendSelection
            ? new TextSelection(_selection.BaseOffset, _text.Length)
            : TextSelection.Collapsed(_text.Length);
        return UpdateSelection(nextSelection);
    }

    public bool Insert(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var start = _selection.Start;
        var end = _selection.End;
        var nextText = _text[..start] + text + _text[end..];
        var caretOffset = start + text.Length;
        SetValue(
            text: nextText,
            selection: TextSelection.Collapsed(caretOffset),
            composing: null);
        return true;
    }

    public bool DeleteBackward()
    {
        if (_selection.IsCollapsed && _selection.ExtentOffset <= 0)
        {
            return false;
        }

        var start = _selection.Start;
        var end = _selection.End;
        if (start == end)
        {
            start = end - 1;
        }

        var nextText = _text[..start] + _text[end..];
        SetValue(
            text: nextText,
            selection: TextSelection.Collapsed(start),
            composing: null);
        return true;
    }

    public bool DeleteForward()
    {
        if (_selection.IsCollapsed && _selection.ExtentOffset >= _text.Length)
        {
            return false;
        }

        var start = _selection.Start;
        var end = _selection.End;
        if (start == end)
        {
            end = start + 1;
        }

        var nextText = _text[..start] + _text[end..];
        SetValue(
            text: nextText,
            selection: TextSelection.Collapsed(start),
            composing: null);
        return true;
    }

    public void Clear()
    {
        SetValue(
            text: string.Empty,
            selection: TextSelection.Collapsed(0),
            composing: null);
    }

    private bool UpdateSelection(TextSelection nextSelection)
    {
        var normalizedSelection = nextSelection.Clamp(_text.Length);
        if (_selection.Equals(normalizedSelection))
        {
            return false;
        }

        _selection = normalizedSelection;
        _composing = null;
        NotifyListeners();
        return true;
    }
}

public sealed class EditableText : StatefulWidget
{
    public EditableText(
        TextEditingController controller,
        FocusNode? focusNode = null,
        string? placeholder = null,
        Action<string>? onChanged = null,
        bool autofocus = false,
        bool enabled = true,
        double fontSize = 14,
        Color? textColor = null,
        Color? placeholderColor = null,
        Color? backgroundColor = null,
        Color? focusedBackgroundColor = null,
        Thickness? padding = null,
        Key? key = null) : base(key)
    {
        Controller = controller;
        FocusNode = focusNode;
        Placeholder = placeholder;
        OnChanged = onChanged;
        Autofocus = autofocus;
        Enabled = enabled;
        FontSize = fontSize;
        TextColor = textColor ?? Colors.Black;
        PlaceholderColor = placeholderColor ?? Color.Parse("#FF757575");
        BackgroundColor = backgroundColor ?? Color.Parse("#FFF5F5F5");
        FocusedBackgroundColor = focusedBackgroundColor ?? Color.Parse("#FFE8F0FE");
        Padding = padding ?? new Thickness(8, 6);
    }

    public TextEditingController Controller { get; }

    public FocusNode? FocusNode { get; }

    public string? Placeholder { get; }

    public Action<string>? OnChanged { get; }

    public bool Autofocus { get; }

    public bool Enabled { get; }

    public double FontSize { get; }

    public Color TextColor { get; }

    public Color PlaceholderColor { get; }

    public Color BackgroundColor { get; }

    public Color FocusedBackgroundColor { get; }

    public Thickness Padding { get; }

    public override State CreateState()
    {
        return new EditableTextState();
    }

    private sealed class EditableTextState : State
    {
        private TextEditingController? _controller;
        private FocusNode? _focusNode;
        private bool _ownsFocusNode;

        private EditableText Widget => (EditableText)Element.Widget;

        public override void InitState()
        {
            AttachController(Widget.Controller);
            AttachFocusNode(Widget.FocusNode);
        }

        public override void DidUpdateWidget(StatefulWidget oldWidget)
        {
            var oldEditableText = (EditableText)oldWidget;
            if (!ReferenceEquals(oldEditableText.Controller, Widget.Controller))
            {
                DetachController();
                AttachController(Widget.Controller);
            }

            if (!ReferenceEquals(oldEditableText.FocusNode, Widget.FocusNode))
            {
                DetachFocusNode(disposeOwned: true);
                AttachFocusNode(Widget.FocusNode);
            }
        }

        public override void Dispose()
        {
            DetachController();
            DetachFocusNode(disposeOwned: true);
        }

        public override Widget Build(BuildContext context)
        {
            var text = _controller!.Text;
            var showPlaceholder = string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(Widget.Placeholder);
            var displayText = BuildDisplayText(
                text: text,
                showPlaceholder: showPlaceholder,
                placeholder: Widget.Placeholder,
                hasFocus: _focusNode!.HasFocus,
                selection: _controller.Selection);
            var textColor = showPlaceholder ? Widget.PlaceholderColor : Widget.TextColor;
            var backgroundColor = _focusNode!.HasFocus ? Widget.FocusedBackgroundColor : Widget.BackgroundColor;

            return new Focus(
                focusNode: _focusNode,
                autofocus: Widget.Autofocus,
                canRequestFocus: Widget.Enabled,
                onKeyEvent: HandleKeyEvent,
                onTextInput: HandleTextInput,
                child: new Container(
                    color: backgroundColor,
                    padding: Widget.Padding,
                    child: new Text(displayText, fontSize: Widget.FontSize, color: textColor)));
        }

        private void AttachController(TextEditingController controller)
        {
            _controller = controller;
            _controller.AddListener(HandleControllerChanged);
        }

        private void DetachController()
        {
            if (_controller == null)
            {
                return;
            }

            _controller.RemoveListener(HandleControllerChanged);
            _controller = null;
        }

        private void AttachFocusNode(FocusNode? externalNode)
        {
            _focusNode = externalNode ?? new FocusNode();
            _ownsFocusNode = externalNode is null;
            _focusNode.AddListener(HandleFocusNodeChanged);
        }

        private void DetachFocusNode(bool disposeOwned)
        {
            if (_focusNode == null)
            {
                return;
            }

            _focusNode.RemoveListener(HandleFocusNodeChanged);

            if (_ownsFocusNode)
            {
                _focusNode.OnKeyEvent = null;
                _focusNode.OnTextInput = null;
            }

            if (disposeOwned && _ownsFocusNode)
            {
                _focusNode.Dispose();
            }

            _focusNode = null;
            _ownsFocusNode = false;
        }

        private KeyEventResult HandleKeyEvent(FocusNode node, KeyEvent @event)
        {
            if (!Widget.Enabled || !@event.IsDown)
            {
                return KeyEventResult.Ignored;
            }

            var controller = _controller!;
            var key = @event.Key;
            var textChanged = false;

            if ((@event.IsControlPressed || @event.IsMetaPressed) && string.Equals(key, "A", StringComparison.Ordinal))
            {
                _ = controller.SelectAll();
                return KeyEventResult.Handled;
            }

            if (string.Equals(key, "Back", StringComparison.Ordinal)
                || string.Equals(key, "Backspace", StringComparison.Ordinal))
            {
                textChanged = controller.DeleteBackward();
            }
            else if (string.Equals(key, "Delete", StringComparison.Ordinal))
            {
                textChanged = controller.DeleteForward();
            }
            else if (string.Equals(key, "ArrowLeft", StringComparison.Ordinal)
                     || string.Equals(key, "Left", StringComparison.Ordinal))
            {
                _ = controller.MoveCaretLeft(extendSelection: @event.IsShiftPressed);
            }
            else if (string.Equals(key, "ArrowRight", StringComparison.Ordinal)
                     || string.Equals(key, "Right", StringComparison.Ordinal))
            {
                _ = controller.MoveCaretRight(extendSelection: @event.IsShiftPressed);
            }
            else if (string.Equals(key, "Home", StringComparison.Ordinal))
            {
                _ = controller.MoveCaretToStart(extendSelection: @event.IsShiftPressed);
            }
            else if (string.Equals(key, "End", StringComparison.Ordinal))
            {
                _ = controller.MoveCaretToEnd(extendSelection: @event.IsShiftPressed);
            }
            else
            {
                return KeyEventResult.Ignored;
            }

            if (textChanged)
            {
                Widget.OnChanged?.Invoke(controller.Text);
            }

            return KeyEventResult.Handled;
        }

        private bool HandleTextInput(FocusNode node, string text)
        {
            if (!Widget.Enabled || string.IsNullOrEmpty(text))
            {
                return false;
            }

            var inserted = _controller!.Insert(text);
            if (inserted)
            {
                Widget.OnChanged?.Invoke(_controller.Text);
            }

            return inserted;
        }

        private void HandleControllerChanged()
        {
            SetState(static () => { });
        }

        private void HandleFocusNodeChanged()
        {
            SetState(static () => { });
        }

        private static string BuildDisplayText(
            string text,
            bool showPlaceholder,
            string? placeholder,
            bool hasFocus,
            TextSelection selection)
        {
            if (showPlaceholder)
            {
                return placeholder ?? string.Empty;
            }

            if (!hasFocus)
            {
                return text;
            }

            var clampedSelection = selection.Clamp(text.Length);
            if (clampedSelection.IsCollapsed)
            {
                var caretOffset = clampedSelection.ExtentOffset;
                return text[..caretOffset] + "|" + text[caretOffset..];
            }

            var start = clampedSelection.Start;
            var end = clampedSelection.End;
            return text[..start] + "[" + text[start..end] + "]" + text[end..];
        }
    }
}
