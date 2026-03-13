using Avalonia.Media;
using Flutter.Widgets;

namespace Flutter.Material;

// Dart parity source (reference): flutter/packages/flutter/lib/src/material/theme_data.dart (approximate)

public sealed record MaterialTextTheme
{
    private static readonly FontFamily DefaultBodyFontFamily = ResolveDefaultBodyFontFamily();

    public MaterialTextTheme(TextStyle? bodyMedium = null)
    {
        BodyMedium = bodyMedium ?? DefaultBodyMedium;
    }

    public TextStyle BodyMedium { get; init; }

    public static TextStyle DefaultBodyMedium { get; } = new(
        FontFamily: DefaultBodyFontFamily,
        FontSize: 14,
        Color: Colors.Black,
        FontWeight: FontWeight.Normal,
        FontStyle: FontStyle.Normal,
        Height: 1.43,
        LetterSpacing: 0.25);

    public static MaterialTextTheme Fallback { get; } = new();

    private static FontFamily ResolveDefaultBodyFontFamily()
    {
        if (OperatingSystem.IsMacOS())
        {
            return new FontFamily(".AppleSystemUIFont");
        }

        return Avalonia.Media.FontFamily.Default;
    }
}

public sealed record ThemeData
{
    public ThemeData(
        MaterialTextTheme? textTheme = null,
        Color? scaffoldBackgroundColor = null,
        Color? canvasColor = null,
        Color? primaryColor = null,
        Color? onPrimaryColor = null,
        Color? onSurfaceColor = null,
        Color? outlineColor = null,
        Color? surfaceContainerLowColor = null,
        ButtonStyle? textButtonStyle = null,
        ButtonStyle? elevatedButtonStyle = null,
        ButtonStyle? outlinedButtonStyle = null)
    {
        TextTheme = textTheme ?? MaterialTextTheme.Fallback;
        ScaffoldBackgroundColor = scaffoldBackgroundColor ?? Colors.White;
        CanvasColor = canvasColor ?? Colors.White;
        PrimaryColor = primaryColor ?? Colors.Blue;
        OnPrimaryColor = onPrimaryColor ?? Colors.White;
        OnSurfaceColor = onSurfaceColor ?? Colors.Black;
        OutlineColor = outlineColor ?? Color.Parse("#FF79747E");
        SurfaceContainerLowColor = surfaceContainerLowColor ?? Color.Parse("#FFF7F2FA");
        TextButtonStyle = textButtonStyle;
        ElevatedButtonStyle = elevatedButtonStyle;
        OutlinedButtonStyle = outlinedButtonStyle;
    }

    public MaterialTextTheme TextTheme { get; init; }

    public Color ScaffoldBackgroundColor { get; init; }

    public Color CanvasColor { get; init; }

    public Color PrimaryColor { get; init; }

    public Color OnPrimaryColor { get; init; }

    public Color OnSurfaceColor { get; init; }

    public Color OutlineColor { get; init; }

    public Color SurfaceContainerLowColor { get; init; }

    public ButtonStyle? TextButtonStyle { get; init; }

    public ButtonStyle? ElevatedButtonStyle { get; init; }

    public ButtonStyle? OutlinedButtonStyle { get; init; }

    public static ThemeData Light { get; } = new();
}
