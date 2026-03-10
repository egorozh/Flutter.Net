using Flutter;
using Flutter.Rendering;
using Flutter.UI;
using Flutter.Widgets;
using AvaloniaInputElement = Avalonia.Input.InputElement;
using AvaloniaTextSelection = Avalonia.Input.TextInput.TextSelection;
using TextInputMethodClientRequestedEventArgs = Avalonia.Input.TextInput.TextInputMethodClientRequestedEventArgs;
using Xunit;

// Dart parity source (reference): flutter/packages/flutter/lib/src/widgets/editable_text.dart; flutter/packages/flutter/test/widgets/editable_text_test.dart (parity regression tests)

namespace Flutter.Tests;

[Collection(SchedulerTestCollection.Name)]
public sealed class TextInputTests : IDisposable
{
    public TextInputTests()
    {
        FocusManager.Instance.ResetForTests();
    }

    public void Dispose()
    {
        FocusManager.Instance.ResetForTests();
    }

    [Fact]
    public void FocusManager_HandleTextInput_InvokesPrimaryFocusCallback()
    {
        var manager = new FocusManager();
        string? captured = null;
        var node = new FocusNode
        {
            OnTextInput = (_, text) =>
            {
                captured = text;
                return true;
            }
        };

        manager.RegisterNode(node);
        manager.RequestFocus(node);

        var handled = manager.HandleTextInput("A");

        Assert.True(handled);
        Assert.Equal("A", captured);
    }

    [Fact]
    public void FocusManager_HandleTextComposition_InvokesPrimaryFocusCallback()
    {
        var manager = new FocusManager();
        var captured = new List<(string Text, bool IsCommit)>();
        var node = new FocusNode
        {
            OnTextComposition = (_, text, isCommit) =>
            {
                captured.Add((text, isCommit));
                return true;
            }
        };

        manager.RegisterNode(node);
        manager.RequestFocus(node);

        var updateHandled = manager.HandleTextCompositionUpdate("pre");
        var commitHandled = manager.HandleTextCompositionCommit("final");

        Assert.True(updateHandled);
        Assert.True(commitHandled);
        Assert.Equal(2, captured.Count);
        Assert.Equal(("pre", false), captured[0]);
        Assert.Equal(("final", true), captured[1]);
    }

    [Fact]
    public void FlutterHost_TextInputMethodClientRequested_ProvidesPreeditBridge()
    {
        var owner = new BuildOwner();
        var controller = new TextEditingController();
        var root = new TestRootElement(
            new EditableText(
                controller: controller,
                autofocus: true));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var host = new FlutterHost();
        var requestArgs = new TextInputMethodClientRequestedEventArgs
        {
            RoutedEvent = AvaloniaInputElement.TextInputMethodClientRequestedEvent
        };

        host.RaiseEvent(requestArgs);

        Assert.NotNull(requestArgs.Client);
        Assert.True(requestArgs.Client!.SupportsPreedit);

        requestArgs.Client.SetPreeditText("ni");
        Assert.Equal("ni", controller.Text);
        Assert.Equal(new TextRange(0, 2), controller.Composing);
        Assert.Equal(TextSelection.Collapsed(2), controller.Selection);

        requestArgs.Client.SetPreeditText(null);
        Assert.Equal(string.Empty, controller.Text);
        Assert.Null(controller.Composing);
        Assert.Equal(TextSelection.Collapsed(0), controller.Selection);
    }

    [Fact]
    public void FlutterHost_TextInputMethodClient_ReflectsSurroundingTextSelectionAndCursorGeometry()
    {
        var owner = new BuildOwner();
        var controller = new TextEditingController();
        var root = new TestRootElement(
            new EditableText(
                controller: controller,
                autofocus: true));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.True(FocusManager.Instance.HandleTextInput("abcd"));
        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "ArrowLeft", isDown: true)));

        var host = new FlutterHost();
        var requestArgs = new TextInputMethodClientRequestedEventArgs
        {
            RoutedEvent = AvaloniaInputElement.TextInputMethodClientRequestedEvent
        };

        host.RaiseEvent(requestArgs);
        var client = requestArgs.Client;

        Assert.NotNull(client);
        Assert.True(client!.SupportsSurroundingText);
        Assert.Equal("abcd", client.SurroundingText);
        Assert.Equal(new AvaloniaTextSelection(3, 3), client.Selection);

        var cursorRect = client.CursorRectangle;
        Assert.True(cursorRect.Width > 0);
        Assert.True(cursorRect.Height > 0);

        client.Selection = new AvaloniaTextSelection(1, 3);
        Assert.Equal(1, controller.Selection.Start);
        Assert.Equal(3, controller.Selection.End);
    }

    [Fact]
    public void EditableText_Multiline_EnterAndVerticalCaretNavigation_Work()
    {
        var owner = new BuildOwner();
        var controller = new TextEditingController();
        var root = new TestRootElement(
            new EditableText(
                controller: controller,
                autofocus: true,
                multiline: true));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.True(FocusManager.Instance.HandleTextInput("ab"));
        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "Enter", isDown: true)));
        Assert.True(FocusManager.Instance.HandleTextInput("cd"));
        Assert.Equal("ab\ncd", controller.Text);
        Assert.Equal(TextSelection.Collapsed(5), controller.Selection);

        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "ArrowUp", isDown: true)));
        Assert.Equal(TextSelection.Collapsed(2), controller.Selection);

        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "ArrowDown", isDown: true)));
        Assert.Equal(TextSelection.Collapsed(5), controller.Selection);

        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "ArrowUp", isDown: true, isShiftPressed: true)));
        Assert.Equal(2, controller.Selection.Start);
        Assert.Equal(5, controller.Selection.End);
    }

    [Fact]
    public void TextEditingController_InsertAndSelectionReplacement_Work()
    {
        var controller = new TextEditingController("abcd");
        controller.Selection = TextSelection.Collapsed(2);

        Assert.True(controller.Insert("X"));
        Assert.Equal("abXcd", controller.Text);
        Assert.Equal(TextSelection.Collapsed(3), controller.Selection);

        controller.Selection = new TextSelection(1, 4);
        Assert.True(controller.Insert("!"));
        Assert.Equal("a!d", controller.Text);
        Assert.Equal(TextSelection.Collapsed(2), controller.Selection);
    }

    [Fact]
    public void TextEditingController_CompositionLifecycle_UpdateCommitAndClear_Work()
    {
        var controller = new TextEditingController("ab");
        controller.Selection = TextSelection.Collapsed(1);

        Assert.True(controller.SetComposing("ni"));
        Assert.Equal("anib", controller.Text);
        Assert.Equal(new TextRange(1, 3), controller.Composing);
        Assert.Equal(TextSelection.Collapsed(3), controller.Selection);

        Assert.True(controller.SetComposing("N"));
        Assert.Equal("aNb", controller.Text);
        Assert.Equal(new TextRange(1, 2), controller.Composing);
        Assert.Equal(TextSelection.Collapsed(2), controller.Selection);

        Assert.True(controller.ClearComposing());
        Assert.Null(controller.Composing);
        Assert.Equal("aNb", controller.Text);

        Assert.True(controller.SetComposing("x"));
        Assert.True(controller.CommitComposing("!"));
        Assert.Equal("aN!b", controller.Text);
        Assert.Null(controller.Composing);
        Assert.Equal(TextSelection.Collapsed(3), controller.Selection);
    }

    [Fact]
    public void EditableText_TextInputAndBackspace_UpdateController()
    {
        var owner = new BuildOwner();
        var controller = new TextEditingController();
        var root = new TestRootElement(
            new EditableText(
                controller: controller,
                autofocus: true,
                placeholder: "Type here"));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.Equal(string.Empty, controller.Text);

        var textHandled = FocusManager.Instance.HandleTextInput("Hi");
        Assert.True(textHandled);
        Assert.Equal("Hi", controller.Text);

        var backspaceHandled = FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "Back", isDown: true));
        Assert.True(backspaceHandled);
        Assert.Equal("H", controller.Text);
    }

    [Fact]
    public void EditableText_CompositionUpdateCommitAndEscape_Work()
    {
        var owner = new BuildOwner();
        var controller = new TextEditingController();
        var root = new TestRootElement(
            new EditableText(
                controller: controller,
                autofocus: true));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.True(FocusManager.Instance.HandleTextCompositionUpdate("ni"));
        Assert.Equal("ni", controller.Text);
        Assert.Equal(new TextRange(0, 2), controller.Composing);
        Assert.Equal(TextSelection.Collapsed(2), controller.Selection);

        Assert.True(FocusManager.Instance.HandleTextCompositionCommit("N"));
        Assert.Equal("N", controller.Text);
        Assert.Null(controller.Composing);
        Assert.Equal(TextSelection.Collapsed(1), controller.Selection);

        Assert.True(FocusManager.Instance.HandleTextCompositionUpdate("yz"));
        Assert.Equal("Nyz", controller.Text);
        Assert.Equal(new TextRange(1, 3), controller.Composing);

        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "Escape", isDown: true)));
        Assert.Equal("Nyz", controller.Text);
        Assert.Null(controller.Composing);

        controller.Clear();
        Assert.True(FocusManager.Instance.HandleTextCompositionUpdate("x"));
        Assert.True(FocusManager.Instance.HandleTextInput("X"));
        Assert.Equal("X", controller.Text);
        Assert.Null(controller.Composing);
    }

    [Fact]
    public void EditableText_ArrowAndSelectionKeys_UpdateControllerSelection()
    {
        var owner = new BuildOwner();
        var controller = new TextEditingController();
        var root = new TestRootElement(
            new EditableText(
                controller: controller,
                autofocus: true));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.True(FocusManager.Instance.HandleTextInput("abcd"));
        Assert.Equal(TextSelection.Collapsed(4), controller.Selection);

        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "ArrowLeft", isDown: true)));
        Assert.Equal(TextSelection.Collapsed(3), controller.Selection);

        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "ArrowLeft", isDown: true, isShiftPressed: true)));
        Assert.Equal(2, controller.Selection.Start);
        Assert.Equal(3, controller.Selection.End);
        Assert.False(controller.Selection.IsCollapsed);

        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "A", isDown: true, isControlPressed: true)));
        Assert.Equal(0, controller.Selection.Start);
        Assert.Equal(4, controller.Selection.End);

        Assert.True(FocusManager.Instance.HandleTextInput("Z"));
        Assert.Equal("Z", controller.Text);
        Assert.Equal(TextSelection.Collapsed(1), controller.Selection);
    }

    [Fact]
    public void EditableText_DeleteForward_AndBackspaceOnSelection_Work()
    {
        var owner = new BuildOwner();
        var controller = new TextEditingController();
        var root = new TestRootElement(
            new EditableText(
                controller: controller,
                autofocus: true));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.True(FocusManager.Instance.HandleTextInput("abcd"));
        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "ArrowLeft", isDown: true)));
        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "ArrowLeft", isDown: true)));
        Assert.Equal(TextSelection.Collapsed(2), controller.Selection);

        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "Delete", isDown: true)));
        Assert.Equal("abd", controller.Text);
        Assert.Equal(TextSelection.Collapsed(2), controller.Selection);

        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "ArrowLeft", isDown: true, isShiftPressed: true)));
        Assert.Equal(1, controller.Selection.Start);
        Assert.Equal(2, controller.Selection.End);

        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "Backspace", isDown: true)));
        Assert.Equal("ad", controller.Text);
        Assert.Equal(TextSelection.Collapsed(1), controller.Selection);
    }

    [Fact]
    public void EditableText_Disabled_IgnoresTextInput()
    {
        var owner = new BuildOwner();
        var controller = new TextEditingController();
        var root = new TestRootElement(
            new EditableText(
                controller: controller,
                enabled: false,
                autofocus: true));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var textHandled = FocusManager.Instance.HandleTextInput("x");
        var compositionUpdateHandled = FocusManager.Instance.HandleTextCompositionUpdate("y");
        var compositionCommitHandled = FocusManager.Instance.HandleTextCompositionCommit("z");
        var keyHandled = FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "Back", isDown: true));

        Assert.False(textHandled);
        Assert.False(compositionUpdateHandled);
        Assert.False(compositionCommitHandled);
        Assert.False(keyHandled);
        Assert.Equal(string.Empty, controller.Text);
        Assert.Null(controller.Composing);
    }

    [Fact]
    public void EditableText_OnChanged_IsRaisedOnTextMutation()
    {
        var owner = new BuildOwner();
        var controller = new TextEditingController();
        var changes = new List<string>();
        var root = new TestRootElement(
            new EditableText(
                controller: controller,
                autofocus: true,
                onChanged: value => changes.Add(value)));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        Assert.True(FocusManager.Instance.HandleTextInput("a"));
        Assert.True(FocusManager.Instance.HandleTextInput("b"));
        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "ArrowLeft", isDown: true)));
        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "Backspace", isDown: true)));

        Assert.Equal(new[] { "a", "ab", "b" }, changes);
    }

    private sealed class TestRootElement : Element, IRenderObjectHost
    {
        private Element? _child;

        public TestRootElement(Widget widget) : base(widget)
        {
        }

        protected override void OnMount()
        {
            base.OnMount();
            Rebuild();
        }

        internal override void Rebuild()
        {
            Dirty = false;
            _child = UpdateChild(_child, Widget, Slot);
        }

        internal override void Update(Widget newWidget)
        {
            base.Update(newWidget);
            Rebuild();
        }

        internal override void VisitChildren(Action<Element> visitor)
        {
            if (_child != null)
            {
                visitor(_child);
            }
        }

        internal override void ForgetChild(Element child)
        {
            if (ReferenceEquals(_child, child))
            {
                _child = null;
            }
        }

        internal override void Unmount()
        {
            if (_child != null)
            {
                UnmountChild(_child);
                _child = null;
            }

            base.Unmount();
        }

        public void InsertRenderObjectChild(RenderObject child, object? slot)
        {
            if (slot != null)
            {
                throw new InvalidOperationException("TestRootElement expects null slot.");
            }
        }

        public void MoveRenderObjectChild(RenderObject child, object? oldSlot, object? newSlot)
        {
            if (!Equals(oldSlot, newSlot))
            {
                throw new InvalidOperationException("TestRootElement does not support slot moves.");
            }
        }

        public void RemoveRenderObjectChild(RenderObject child, object? slot)
        {
            if (slot != null)
            {
                throw new InvalidOperationException("TestRootElement expects null slot.");
            }
        }
    }
}
