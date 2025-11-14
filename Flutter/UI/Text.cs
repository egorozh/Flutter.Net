namespace Flutter.UI;

/// A horizontal line used for aligning text.
public enum TextBaseline
{
    /// The horizontal line used to align the bottom of glyphs for alphabetic characters.
    Alphabetic,

    /// The horizontal line used to align ideographic characters.
    Ideographic,
}

public enum TextDirection
{
    /// The text flows from right to left (e.g. Arabic, Hebrew).
    Rtl,

    /// The text flows from left to right (e.g., English, French).
    Ltr
}