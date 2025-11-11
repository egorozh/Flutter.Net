using Avalonia.Controls;
using Avalonia.Media;
using Flutter.Rendering;

namespace Flutter;

public sealed class DemoWindow : Window
{
    private AnimationController? _controller;

    public DemoWindow()
    {
        Title = "Flutter-on-C# over Avalonia DC (MVP)";

        Width = 720;
        Height = 480;

        Background = new SolidColorBrush(Color.FromRgb(20, 22, 25));

        var host = new FlutterHost();

        Content = host;

        _controller = new AnimationController(TimeSpan.FromSeconds(2))
        {
            Curve = Curves.EaseInOut
        };

        _controller.Repeat(reverse: true);

        host.SetRootChild(DemoRenderObjectsTree());
    }

    private RenderBox DemoRenderObjectsTree()
    {
        var column = new RenderColumn { Spacing = 16 };

        // Статичный бокс
        column.Children.Add(new RenderColoredBox(Color.FromRgb(58, 98, 180), height: 80, radius: 18));

        // Текст
        column.Children.Add(new RenderParagraph("Анимация рендера: цвет/ширина/высота (repeat + reverse)")
            { FontSize = 22 });

        // Анимируемый бокс: от 200×90 (сине-фиолет) до 420×160 (зеленоватый)
        var animated = new RenderAnimatedBox(
            controller: _controller,
            fromWidth: 200, toWidth: 420,
            fromHeight: 90, toHeight: 160,
            fromColor: Color.FromRgb(96, 84, 200),
            toColor: Color.FromRgb(60, 200, 140)
        );

        column.Children.Add(animated);

        // Ещё текст
        column.Children.Add(
            new RenderParagraph("Дальше можно добавить opacity/transform, а также аниматоры для layout-контейнеров."));

        return column;
    }


    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _controller?.Dispose();
        _controller = null;
    }
}