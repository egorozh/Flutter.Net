using Xunit;

namespace Flutter.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SchedulerTestCollection
{
    public const string Name = "Scheduler serial";
}
