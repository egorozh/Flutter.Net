# AGENTS.md

This file defines expectations for coding agents working in this repository.

## Project Snapshot

- Platform: .NET 10
- UI stack: Avalonia
- Purpose: Flutter-like widget/rendering layer implemented in C#
- Main library: `src/Flutter`
- Example hosts: `src/Sample/*`
- Main solution: `src/Flutter.Net.sln`

## Repository Map

- `src/Flutter`: core framework (`Foundation`, `Widgets`, `Rendering`, `UI`, scheduler/ticker pipeline).
- `src/Sample/Flutter.Net`: shared sample app/widgets.
- `src/Sample/Flutter.Net.Desktop`: desktop entry point.
- `src/Sample/Flutter.Net.Browser`: WebAssembly host.
- `src/Sample/Flutter.Net.Android`: Android host.
- `src/Sample/Flutter.Net.iOS`: iOS host.

## Environment Requirements

- .NET SDK 10 preview (projects target `net10.0` and platform-specific TFMs).
- Avalonia tooling/workloads for browser/mobile targets where applicable.

## Common Commands

Run from repository root:

```bash
dotnet restore src/Flutter.Net.sln
dotnet build src/Flutter.Net.sln -c Debug
dotnet run --project src/Sample/Flutter.Net.Desktop/Flutter.Net.Desktop.csproj
dotnet run --project src/Sample/Flutter.Net.Browser/Flutter.Net.Browser.csproj
```

Platform-specific builds:

```bash
dotnet build src/Sample/Flutter.Net.Android/Flutter.Net.Android.csproj -c Debug
dotnet build src/Sample/Flutter.Net.iOS/Flutter.Net.iOS.csproj -c Debug
```

## Change Guidelines

1. Keep core API and behavior changes focused in `src/Flutter` unless sample host updates are required.
2. Respect existing architecture boundaries: `Widget` -> `Element` -> Avalonia control/render object.
3. Preserve lifecycle contracts (`CreateElement`, mount/update/rebuild flow, render object attachment).
4. Keep nullability correctness (`Nullable` is enabled) and avoid introducing nullable warnings.
5. Avoid broad dependency/framework upgrades unless explicitly requested.

## Validation Checklist

1. Build the full solution: `dotnet build src/Flutter.Net.sln -c Debug`.
2. For UI behavior changes, run desktop sample and verify startup/rendering.
3. For browser/mobile changes, build the affected sample project(s).
4. There is no dedicated automated test project yet; add focused tests when introducing non-trivial logic.
