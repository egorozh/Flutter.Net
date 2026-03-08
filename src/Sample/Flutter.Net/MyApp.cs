using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.Widgets;

namespace Flutter.Net;

public sealed class CounterApp : StatefulWidget
{
    public override State CreateState() => new CounterState();

    private sealed class CounterState : State
    {
        private readonly object _globalBadgeIdentity = new();
        private int _count;
        private int _nextId = 5;
        private bool _placeGlobalOnLeft = true;
        private List<int> _items = [1, 2, 3, 4];

        public override Widget Build(BuildContext context)
        {
            return new Container(
                color: Colors.White,
                padding: new Thickness(20),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.Stretch,
                    spacing: 12,
                    children:
                    [
                        new Text("Flutter Counter", fontSize: 24, color: Colors.Black),
                        new Text($"Count: {_count}", fontSize: 18, color: Colors.DarkSlateBlue),
                        new Row(
                            spacing: 12,
                            children:
                            [
                                new Expanded(
                                    child: new Button(
                                        label: "-",
                                        onPressed: () => SetState(() => _count--),
                                        background: Colors.LightGray,
                                        foreground: Colors.Black,
                                        fontSize: 20)),
                                new Expanded(
                                    child: new Button(
                                        label: "+",
                                        onPressed: () => SetState(() => _count++),
                                        background: Colors.SkyBlue,
                                        foreground: Colors.Black,
                                        fontSize: 20)),
                            ]),
                        new SizedBox(height: 12),
                        new Text("Keyed List (ValueKey)", fontSize: 18, color: Colors.Black),
                        new Row(
                            spacing: 8,
                            children:
                            [
                                new Expanded(
                                    child: new Button(
                                        label: "Reverse",
                                        onPressed: () => SetState(() => _items = [.._items.AsEnumerable().Reverse()]),
                                        background: Colors.MediumSeaGreen,
                                        foreground: Colors.White,
                                        fontSize: 13)),
                                new Expanded(
                                    child: new Button(
                                        label: "Insert Head",
                                        onPressed: () => SetState(() => _items.Insert(0, _nextId++)),
                                        background: Colors.SteelBlue,
                                        foreground: Colors.White,
                                        fontSize: 13)),
                                new Expanded(
                                    child: new Button(
                                        label: "Remove Tail",
                                        onPressed: _items.Count == 0
                                            ? null
                                            : () => SetState(() => _items.RemoveAt(_items.Count - 1)),
                                        background: Colors.IndianRed,
                                        foreground: Colors.White,
                                        fontSize: 13)),
                            ]),
                        new Column(
                            spacing: 8,
                            children: [.._items.Select(id => new KeyedListItem(id, key: new ValueKey<int>(id)))]),
                        new SizedBox(height: 12),
                        new Text("GlobalKey Reparent", fontSize: 18, color: Colors.Black),
                        new Row(
                            spacing: 8,
                            children:
                            [
                                new Expanded(
                                    child: new Container(
                                        color: Color.Parse("#FFE8F5E9"),
                                        padding: new Thickness(8),
                                        child: _placeGlobalOnLeft
                                            ? new MovableBadge(key: new GlobalObjectKey<MovableBadgeState>(_globalBadgeIdentity))
                                            : new Text("left slot", color: Colors.Gray))),
                                new Expanded(
                                    child: new Container(
                                        color: Color.Parse("#FFE3F2FD"),
                                        padding: new Thickness(8),
                                        child: !_placeGlobalOnLeft
                                            ? new MovableBadge(key: new GlobalObjectKey<MovableBadgeState>(_globalBadgeIdentity))
                                            : new Text("right slot", color: Colors.Gray))),
                            ]),
                        new Button(
                            label: "Move Global Widget",
                            onPressed: () => SetState(() => _placeGlobalOnLeft = !_placeGlobalOnLeft),
                            background: Colors.SlateGray,
                            foreground: Colors.White,
                            fontSize: 14),
                    ]));
        }
    }

    private sealed class KeyedListItem : StatefulWidget
    {
        private readonly int _id;

        public KeyedListItem(int id, Key? key = null) : base(key)
        {
            _id = id;
        }

        public override State CreateState() => new KeyedListItemState(_id);
    }

    private sealed class KeyedListItemState : State
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

    private sealed class MovableBadge : StatefulWidget
    {
        public MovableBadge(Key? key = null) : base(key)
        {
        }

        public override State CreateState() => new MovableBadgeState();
    }

    private sealed class MovableBadgeState : State
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
}
