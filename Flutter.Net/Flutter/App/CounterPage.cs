using Flutter.Net.Flutter.Framework;

namespace Flutter.Net.Flutter.App;

public class MyApp : StatelessWidget
{
    public override Widget Build(BuildContext context)
    {
        return new Column([
            new Text("Hello, Avalonia!"),
            new CounterWidget()
        ]);
    }
}

public class CounterWidget : StatefulWidget
{
    public override IState CreateState() => new CounterState();
}

public class CounterState : State
{
    private int _count;

    public override Widget Build(BuildContext context)
    {
        return new Column([
            new Text($"Counter: {_count}"),
            new Button("Increment", Increment)
        ]);
    }

    private void Increment() => SetState(() => _count++);
}