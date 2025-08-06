namespace Flutter.Foundation;

public abstract record Key;

public abstract record LocalKey : Key;

public abstract record UniqueKey : LocalKey;

public record ValueKey<T>(T Value) : LocalKey;