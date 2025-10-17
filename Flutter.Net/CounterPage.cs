
using Flutter.Widgets;

namespace Flutter.Net;

public class CounterPage : StatefulWidget
{
    public override State CreateState() => new CounterState();
}

public class CounterState : State
{
    private int _count;

    public override Widget Build(BuildContext context)
    {
        return new Column([
            new Text($"Counter: {_count}"),
            //new Button("Increment", Increment)
        ]);
    }

    private void Increment() => SetState(() => _count++);
}