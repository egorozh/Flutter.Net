using Flutter.Rendering;
using Flutter.UI;
using Flutter.Widgets;
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
        var keyHandled = FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "Back", isDown: true));

        Assert.False(textHandled);
        Assert.False(keyHandled);
        Assert.Equal(string.Empty, controller.Text);
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
        Assert.True(FocusManager.Instance.HandleKeyEvent(new KeyEvent(key: "Backspace", isDown: true)));

        Assert.Equal(new[] { "a", "ab", "a" }, changes);
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
