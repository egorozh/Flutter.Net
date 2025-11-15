using Avalonia.Controls;
using Avalonia.Media;
using Flutter.Foundation;

namespace Flutter.Widgets;

// ─────────────────────────────────────────────────────────────────────
// Text
// ─────────────────────────────────────────────────────────────────────
public sealed class Text : Widget
{
    public string Data { get; }
    public double? FontSize { get; }
    public IBrush? Foreground { get; }

    public Text(string data, double? fontSize = null, IBrush? color = null, Key? key = null) : base(key)
    {
        Data = data;
        FontSize = fontSize;
        Foreground = color;
    }

    internal override Element CreateElement() => new TextElement(this);
}

public sealed class TextElement : ControlElement<TextBlock>
{
    private Text _w;

    public TextElement(Text w) : base(w)
    {
        _w = w;
    }

    protected override void OnMount()
    {
        base.OnMount();
        Apply(_w);
    }

    internal override void Rebuild()
    {
        Dirty = false;
    }

    internal override void Update(Widget newWidget)
    {
        _w = (Text)newWidget;
        Apply(_w);
    }

    private void Apply(Text w)
    {
        HostControl.Text = w.Data;
        if (w.FontSize.HasValue) HostControl.FontSize = w.FontSize.Value;
        if (w.Foreground is not null) HostControl.Foreground = w.Foreground;
    }
}