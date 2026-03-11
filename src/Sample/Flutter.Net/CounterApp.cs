using System;
using Avalonia.Media;
using Flutter.Rendering;
using Flutter.Widgets;

// Dart parity source (reference): dart_sample/lib/counter_app.dart (exact sample parity)

namespace Flutter.Net;

public sealed class CounterApp : StatefulWidget
{
    public override State CreateState() => new CounterAppState();

    private sealed class CounterAppState : State
    {
        private static readonly FontFamily MaterialBodyFontFamily = ResolveMaterialBodyFontFamily();
        private CounterAppModel _model = null!;

        public override void InitState()
        {
            _model = new CounterAppModel();
        }

        public override void Dispose()
        {
            _model.Dispose();
        }

        public override Widget Build(BuildContext context)
        {
            return new CounterScope(
                _model,
                new DefaultTextStyle(
                    style: new TextStyle(
                        FontFamily: MaterialBodyFontFamily,
                        FontSize: 14,
                        Color: Colors.Black,
                        FontWeight: FontWeight.Normal,
                        FontStyle: FontStyle.Normal,
                        Height: 1.43,
                        LetterSpacing: 0.25),
                    child: new SampleGalleryScreen()));
        }

        private static FontFamily ResolveMaterialBodyFontFamily()
        {
            if (OperatingSystem.IsMacOS())
            {
                return new FontFamily(".AppleSystemUIFont");
            }

            return Avalonia.Media.FontFamily.Default;
        }
    }
}
