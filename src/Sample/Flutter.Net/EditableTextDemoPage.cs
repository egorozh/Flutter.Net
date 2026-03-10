using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;
using Flutter.Widgets;

// Dart parity source (reference): dart_sample/lib/editable_text_demo_page.dart (exact sample parity)

namespace Flutter.Net;

public sealed class EditableTextDemoPage : StatefulWidget
{
    public override State CreateState()
    {
        return new EditableTextDemoPageState();
    }
}

internal sealed class EditableTextDemoPageState : State
{
    private TextEditingController _nameController = null!;
    private TextEditingController _notesController = null!;
    private bool _enabled = true;
    private string _lastChange = "(none)";

    public override void InitState()
    {
        _nameController = new TextEditingController();
        _notesController = new TextEditingController();
    }

    public override void Dispose()
    {
        _nameController.Dispose();
        _notesController.Dispose();
    }

    public override Widget Build(BuildContext context)
    {
        return new Column(
            crossAxisAlignment: CrossAxisAlignment.Stretch,
            spacing: 10,
            children:
            [
                new Text("EditableText + Focus/IME", fontSize: 20, color: Colors.Black),
                new Text(
                    "Baseline text input flow: tap field, type, Backspace, and Tab between fields.",
                    fontSize: 14,
                    color: Colors.DimGray),
                new Row(
                    spacing: 8,
                    children:
                    [
                        new SizedBox(
                            width: 120,
                            child: new CounterTapButton(
                                label: _enabled ? "Disable" : "Enable",
                                onTap: () => SetState(() => _enabled = !_enabled),
                                background: Color.Parse("#FFDCE3ED"),
                                foreground: Colors.Black,
                                fontSize: 12,
                                padding: new Thickness(10, 8))),
                        new SizedBox(
                            width: 120,
                            child: new CounterTapButton(
                                label: "Clear",
                                onTap: () => SetState(() =>
                                {
                                    _nameController.Clear();
                                    _notesController.Clear();
                                    _lastChange = "(cleared)";
                                }),
                                background: Color.Parse("#FFE9F5EC"),
                                foreground: Colors.Black,
                                fontSize: 12,
                                padding: new Thickness(10, 8))),
                    ]),
                new Text($"last change: {_lastChange}", fontSize: 12, color: Colors.DarkSlateGray),
                new Text("Name", fontSize: 12, color: Colors.DimGray),
                new EditableText(
                    controller: _nameController,
                    enabled: _enabled,
                    placeholder: "Type your name",
                    onChanged: value => SetState(() => _lastChange = $"name = {value}")),
                new Text("Notes", fontSize: 12, color: Colors.DimGray),
                new EditableText(
                    controller: _notesController,
                    enabled: _enabled,
                    placeholder: "Type a short note",
                    onChanged: value => SetState(() => _lastChange = $"notes = {value}")),
                new Text(
                    $"current: name='{_nameController.Text}', notes='{_notesController.Text}'",
                    fontSize: 12,
                    color: Colors.Black),
            ]);
    }
}
