using System;
using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.Widgets;

namespace Flutter.Net;

internal sealed class CounterTapButton : StatelessWidget
{
    public CounterTapButton(
        string label,
        Action? onTap,
        Color background,
        Color foreground,
        double fontSize,
        Thickness? padding = null,
        Key? key = null) : base(key)
    {
        Label = label;
        OnTap = onTap;
        Background = background;
        Foreground = foreground;
        FontSize = fontSize;
        Padding = padding ?? new Thickness(14, 10);
    }

    public string Label { get; }

    public Action? OnTap { get; }

    public Color Background { get; }

    public Color Foreground { get; }

    public double FontSize { get; }

    public Thickness Padding { get; }

    public override Widget Build(BuildContext context)
    {
        return new GestureDetector(
            behavior: HitTestBehavior.Opaque,
            onTap: OnTap,
            child: new Container(
                color: OnTap == null ? DisabledColor(Background) : Background,
                padding: Padding,
                child: new Text(Label, fontSize: FontSize, color: Foreground)));
    }

    private static Color DisabledColor(Color color)
    {
        var alpha = (byte)Math.Clamp((int)(color.A * 0.45), 0, 255);
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }
}

internal sealed class KeyedListItem : StatefulWidget
{
    private readonly int _id;

    public KeyedListItem(int id, Key? key = null) : base(key)
    {
        _id = id;
    }

    public override State CreateState() => new KeyedListItemState(_id);
}

internal sealed class KeyedListItemState : State
{
    private readonly int _id;
    private readonly int _token;
    private int _taps;

    public KeyedListItemState(int id)
    {
        _id = id;
        _token = Random.Shared.Next(1000, 9999);
    }

    public override Widget Build(BuildContext context)
    {
        return new CounterTapButton(
            label: $"id={_id} token={_token} taps={_taps}",
            onTap: () => SetState(() => _taps++),
            background: Color.Parse("#FFF5F5F5"),
            foreground: Colors.Black,
            fontSize: 14,
            padding: new Thickness(10, 8));
    }
}

internal sealed class MovableBadge : StatefulWidget
{
    public MovableBadge(Key? key = null) : base(key)
    {
    }

    public override State CreateState() => new MovableBadgeState();
}

internal sealed class MovableBadgeState : State
{
    private int _taps;

    public override Widget Build(BuildContext context)
    {
        return new CounterTapButton(
            label: $"global taps={_taps}",
            onTap: () => SetState(() => _taps++),
            background: Colors.DarkOrange,
            foreground: Colors.White,
            fontSize: 14,
            padding: new Thickness(10, 8));
    }
}
