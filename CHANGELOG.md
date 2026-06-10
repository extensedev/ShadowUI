# Changelog

All notable changes to ShadowUI are documented here.
Format: [Keep a Changelog](https://keepachangelog.com), versioning: [SemVer](https://semver.org).

## [1.0.0-beta] — 2026-06-10

### Added
- **New controls:** `Popover`, `MultiSelectComboBox` (chips + searchable multi-select dropdown), `AreaChart`, `PieChart` (donut).
- **8 new palettes** (13 total): lifted darks `Charcoal` (new default) / `Graphite` / `Ash`; colored accents `Blue`, `Green`, `Violet`, `Rose`, `Orange`.
- Sonner-style toast stacking: cards stack with peek/scale, expand on hover, max 3 visible.
- `ShadowCalendar`: `FirstDayOfWeek` property; weekday headers and grid offset follow the current culture.
- Localization properties: `ShadowDataTable.FilterPlaceholder/EmptyText/PrevText/NextText/PageInfoFormat`, `CommandPalette.SearchPlaceholder`.
- `.textarea` class demo, NuGet metadata (SourceLink, symbols, icon), multi-targeting `net8.0;net10.0`.

### Changed
- **Default palette is now `Charcoal`** (was `Zinc`).
- ScrollBar: thin resting thumb that expands on hover; scrollbars reserve their own column instead of overlaying content.
- Avalonia dependency declared as range `[12.0.4,13.0.0)`.

### Fixed
- FluentTheme removal fallout: standalone `ControlTheme`s for `ScrollViewer`, `ListBox`/`ListBoxItem`, `GridSplitter`; mouse wheel hit-testing over empty areas; calendar day buttons, carousel dots and chips styling for code-created elements.
- `Slider`/`ProgressBar` collapsed to zero width in shrink containers (restored `MinWidth`).
- Sidebar item spacing (ItemsPresenter was ignoring `ItemsPanel`); chart brushes silently falling back to non-palette colors.

## [0.3.x and earlier]

Initial foundation: 50+ controls, Zinc/Slate/Stone/Gray/Neutral palettes, dark/light theming, AOT compatibility.
