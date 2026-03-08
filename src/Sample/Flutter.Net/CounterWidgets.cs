using System;
using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Widgets;

namespace Flutter.Net;

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
        return new Button(
            label: $"id={_id} token={_token} taps={_taps}",
            onPressed: () => SetState(() => _taps++),
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
        return new Button(
            label: $"global taps={_taps}",
            onPressed: () => SetState(() => _taps++),
            background: Colors.DarkOrange,
            foreground: Colors.White,
            fontSize: 14,
            padding: new Thickness(10, 8));
    }
}
