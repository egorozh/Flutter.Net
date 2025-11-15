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
                color: Brushes.White,
                padding: new Thickness(24),
                child: new Column(
                    children:
                    [
                        new Text("Flutter Counter", fontSize: 20, color: Brushes.Black),

                        new SizedBox(height: 12),

                        new Text($"Count: {_count}", fontSize: 16, color: Brushes.DarkSlateBlue),

                        new SizedBox(height: 12),

                        new Row(
                            mainAxisAlignment: MainAxisAlignment.SpaceBetween,
                            crossAxisAlignment: CrossAxisAlignment.Stretch,
                            spacing: 12,
                            children:
                            [
                                new Expanded(
                                    child: new Container(
                                        color: Brushes.LightGray,
                                        padding: new Thickness(12),
                                        child: new Text("-")
                                    )
                                ),

                                new SizedBox(width: 12),

                                new Expanded(new Container(
                                        color: Brushes.SkyBlue,
                                        padding: new Thickness(12),
                                        child: new Text("+")
                                    )
                                )
                            ]
                        )
                    ])
            );
        }
    }
}