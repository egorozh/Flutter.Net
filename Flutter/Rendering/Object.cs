namespace Flutter.Rendering;

/// <summary>
/// An abstract set of layout constraints.
/// </summary>
public interface IConstraints
{
    /// <summary>
    /// Whether there is exactly one size possible given these constraints.
    /// </summary>
    bool IsTight { get; }

    /// <summary>
    /// Whether the constraint is expressed in a consistent manner.
    /// </summary>
    bool IsNormalized { get; }
}