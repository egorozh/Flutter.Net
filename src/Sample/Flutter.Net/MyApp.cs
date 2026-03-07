using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;
using Flutter.Widgets;

namespace Flutter.Net;

public sealed class CounterApp : StatefulWidget
{
    public override State CreateState() => new CounterState();

    private sealed class CounterState : State
    {
        private int _count;

        public override Widget Build(BuildContext context)
        {
            return new Container(
                color: Colors.White,
                padding: new Thickness(24),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.Stretch,
                    spacing: 12,
                    children:
                    [
                        new Text("Flutter Counter", fontSize: 24, color: Colors.Black),
                        new Text($"Count: {_count}", fontSize: 18, color: Colors.DarkSlateBlue),
                        new Row(
                            spacing: 12,
                            mainAxisAlignment: MainAxisAlignment.Start,
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
                                        fontSize: 20))
                            ])
                    ])
            );
        }
    }
}
