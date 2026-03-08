using Flutter.Widgets;

namespace Flutter.Net;

public sealed class CounterApp : StatefulWidget
{
    public override State CreateState() => new CounterAppState();

    private sealed class CounterAppState : State
    {
        private CounterAppModel _model = null!;
        private SamplePageId _currentPage = SamplePageId.Counter;

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
                new SampleGalleryScreen(
                    currentPage: _currentPage,
                    onPageSelected: HandlePageSelected));
        }

        private void HandlePageSelected(SamplePageId page)
        {
            if (page == _currentPage)
            {
                return;
            }

            SetState(() => _currentPage = page);
        }
    }
}
