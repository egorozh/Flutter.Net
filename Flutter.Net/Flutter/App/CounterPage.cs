using Flutter.Net.Flutter.Framework;

namespace Flutter.Net.Flutter.App;

public class CounterPage : StatefulWidget
{
    protected override State CreateState() => new CounterPageState();
}

public class CounterPageState : State
{
    private int _count = 0;

    internal override Widget Build(IBuildContext context)
    {
        return new Column(children:
        [
            new Text($"Count: {_count}"),
            new ElevatedButton(
                () => SetState(() => _count++),
                new Text("Increment"))
        ]);
    }
}