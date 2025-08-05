using Avalonia.Controls;
using Flutter.Widgets;

namespace Flutter.Material;

public class Button(string text, Action onPressed) : IWidget
{
    public string Text { get; } = text;
    public Action OnPressed { get; } = onPressed;

    public Element CreateElement() => new ButtonElement(this);
}

public class ButtonElement(Button widget) : Element(widget)
{
    public new Button Widget
    {
        get => (Button)base.Widget;
        set => base.Widget = value;
    }


    public override void Mount(Control parent)
    {
        var button = new Avalonia.Controls.Button { Content = Widget.Text };
        button.Click += (_, _) => Widget.OnPressed();
        RenderObject = button;

        if (parent is Panel panel)
            panel.Children.Add(button);
    }

    public override void Update(IWidget newIWidget)
    {
        if (newIWidget is Button newButton)
        {
            Widget = newButton;

            if (RenderObject is Avalonia.Controls.Button btn)
            {
                btn.Content = newButton.Text;
                btn.Click += (_, _) => newButton.OnPressed();
            }
        }
    }
}