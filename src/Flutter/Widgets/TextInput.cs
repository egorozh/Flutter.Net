using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.UI;

// Dart parity source (reference): flutter/packages/flutter/lib/src/widgets/editable_text.dart; flutter/packages/flutter/lib/src/widgets/text_field.dart (adapted)

namespace Flutter.Widgets;

public sealed class TextEditingController : ChangeNotifier
{
    private string _text;

    public TextEditingController(string text = "")
    {
        _text = text;
    }

    public string Text
    {
        get => _text;
        set
        {
            var next = value ?? string.Empty;
            if (string.Equals(_text, next, StringComparison.Ordinal))
            {
                return;
            }

            _text = next;
            NotifyListeners();
        }
    }

    public void Clear()
    {
        Text = string.Empty;
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
            var displayText = showPlaceholder ? Widget.Placeholder! : text;
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

            if (string.Equals(@event.Key, "Back", StringComparison.Ordinal)
                || string.Equals(@event.Key, "Backspace", StringComparison.Ordinal)
                || string.Equals(@event.Key, "Delete", StringComparison.Ordinal))
            {
                if (string.IsNullOrEmpty(_controller!.Text))
                {
                    return KeyEventResult.Ignored;
                }

                var text = _controller.Text;
                _controller.Text = text[..^1];
                Widget.OnChanged?.Invoke(_controller.Text);
                return KeyEventResult.Handled;
            }

            return KeyEventResult.Ignored;
        }

        private bool HandleTextInput(FocusNode node, string text)
        {
            if (!Widget.Enabled || string.IsNullOrEmpty(text))
            {
                return false;
            }

            _controller!.Text += text;
            Widget.OnChanged?.Invoke(_controller.Text);
            return true;
        }

        private void HandleControllerChanged()
        {
            SetState(static () => { });
        }

        private void HandleFocusNodeChanged()
        {
            SetState(static () => { });
        }
    }
}
