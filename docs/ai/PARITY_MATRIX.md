# Sample Parity Matrix

Scope: structural parity of sample routes/modules between C# and Dart samples.

Last checked: 2026-03-10

Status legend:

- `done`: feature exists in both samples and is wired in route map/menu.
- `verify-runtime`: structural parity exists, but interactive behavior still needs runtime validation.
- `missing`: present only on one side.

## Route and Feature Parity

| Feature | C# sample | Dart sample | Status | Notes |
| --- | --- | --- | --- | --- |
| App bootstrap | `src/Sample/Flutter.Net/CounterApp.cs` | `dart_sample/lib/counter_app.dart` | done | Counter model + scope app root present on both sides. |
| Route constants + route data | `src/Sample/Flutter.Net/SampleGalleryScreen.cs` | `dart_sample/lib/sample_routes.dart` | done | Same route set including navigator details route. |
| Sample gallery menu | `src/Sample/Flutter.Net/SampleGalleryScreen.cs` | `dart_sample/lib/sample_gallery_screen.dart` | done | Same demo page set and menu flow. |
| Counter page | `src/Sample/Flutter.Net/CounterScreen.cs` | `dart_sample/lib/counter_screen.dart` | verify-runtime | Structural match; runtime behavior should be periodically rechecked. |
| Navigator demo | `src/Sample/Flutter.Net/NavigatorDemoPage.cs` | `dart_sample/lib/navigator_demo_page.dart` | verify-runtime | Route operations and observer flows are mirrored. |
| ListView separated demo | `src/Sample/Flutter.Net/ListViewSeparatedDemoPage.cs` | `dart_sample/lib/list_view_separated_demo_page.dart` | verify-runtime | |
| ListView fixed extent demo | `src/Sample/Flutter.Net/ListViewFixedExtentDemoPage.cs` | `dart_sample/lib/list_view_fixed_extent_demo_page.dart` | verify-runtime | |
| ListView reverse demo | `src/Sample/Flutter.Net/ListViewReverseDemoPage.cs` | `dart_sample/lib/list_view_reverse_demo_page.dart` | verify-runtime | |
| GridView demo | `src/Sample/Flutter.Net/GridViewDemoPage.cs` | `dart_sample/lib/grid_view_demo_page.dart` | verify-runtime | |
| Custom slivers demo | `src/Sample/Flutter.Net/CustomSliversDemoPage.cs` | `dart_sample/lib/custom_slivers_demo_page.dart` | verify-runtime | |
| Scrollbar demo | `src/Sample/Flutter.Net/ScrollbarDemoPage.cs` | `dart_sample/lib/scrollbar_demo_page.dart` | verify-runtime | |
| Editable text demo | `src/Sample/Flutter.Net/EditableTextDemoPage.cs` | `dart_sample/lib/editable_text_demo_page.dart` | verify-runtime | Includes baseline input flow plus multiline Notes demo (`Enter` newline, Up/Down caret travel), seed action, and escaped live value summary. |
| Shared counter widgets | `src/Sample/Flutter.Net/CounterWidgets.cs` | `dart_sample/lib/counter_widgets.dart` | verify-runtime | Includes keyed/movable/keep-alive helper widgets. |
| Counter state container | `src/Sample/Flutter.Net/CounterAppModel.cs`, `src/Sample/Flutter.Net/CounterScope.cs` | `dart_sample/lib/counter_app_model.dart`, `dart_sample/lib/counter_scope.dart` | done | |

## When Updating Sample Features

1. Update C# sample (`src/Sample/Flutter.Net/*`).
2. Apply equivalent Dart change (`dart_sample/lib/*`) in the same iteration.
3. Update this matrix status/notes.
4. Record shipped parity changes in `CHANGELOG.md`.
