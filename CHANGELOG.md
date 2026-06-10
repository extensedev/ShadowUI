# Changelog

All notable changes to ShadowUI are documented here.
Format: [Keep a Changelog](https://keepachangelog.com), versioning: [SemVer](https://semver.org).

## [1.0.1] — 2026-06-11

### Fixed
- **Standalone-theme gap:** added infrastructure `ControlTheme`s previously provided by FluentTheme — `ItemsControl`, `PathIcon` and `TransitioningContentControl` rendered nothing in apps without `FluentTheme`. `PathIcon` inherits `Foreground` from its parent (button variants tint their icons) and defaults to 16×16. `ContentControl`/`UserControl` need no theme (Avalonia ships a built-in default template); `Expander`/`SplitButton` remain out of scope until they get a proper shadcn-styled port.

## [1.0.0] — 2026-06-10

### Added
- `TitleBar` customization: `ShowTitle`, `ShowMinimize`, `ShowClose` (alongside `ShowMaximize`), and a `RightContent` slot for custom buttons next to the window controls; the `TitleBarButton` theme is reusable for them.
- ScrollBar: clicking/holding the track pages up/down (transparent `RepeatButton`s in the `Track`).
- ScrollViewer `gutter` class: scrollbar gets a reserved column instead of overlaying — used by ComboBox, SearchableComboBox and CommandPalette dropdowns.

### Changed
- Scrollbars overlay the content (no reserved gutter, content no longer shifts when a bar appears); resting thumb is a thin always-visible pill that widens while the pointer is over the viewer (behavior modeled on ShadUI).
- Typography: shadcn letter-spacing on `h1`–`h4`; removed `LineHeight` values smaller than the natural line height (they sank baselines and clipped descenders); `code` is now SemiBold.
- DatePicker/ShadowCalendar visual parity pass: popover surface, borderless calendar inside the popup, Lucide icons in exact pixel coordinates.

### Fixed
- **Windows 11 minimize/maximize/close animations** with the custom `TitleBar`: Avalonia strips `WS_CAPTION` in extended-client-area mode (a Windows 10 workaround), which disables DWM transitions; the style bit is now forced back via `Win32Properties.AddWindowStylesCallback` on Windows 11+.
- Scrollbar thumb fighting the smooth-scroll glide: grabbing the thumb (or any pointer press inside the viewer) now stops the wheel glide immediately instead of pulling the offset back to a stale target.
- Scrollbar thumb hover-expansion never triggered (inline template `Margin` outranked the style setter).

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
