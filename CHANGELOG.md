# Changelog

All notable changes to ShadowUI are documented here.
Format: [Keep a Changelog](https://keepachangelog.com), versioning: [SemVer](https://semver.org).

## [Unreleased]

## [1.0.7] — 2026-06-29

### Added
- **`Separator` control (ShadowUI).** A thin divider with `Orientation` (horizontal/vertical), `Thickness`, and `FadeEdges` — when set, both ends fade to transparent via an axis-aware opacity mask that composes with any `Background` brush (including a custom `LinearGradientBrush`). The built-in Avalonia `Separator` theme stays a plain solid line.
- **`Spinner` `.loader` variant.** A lucide-style 8-spoke sunburst that spins continuously (Tailwind `animate-spin` equivalent).
- **`TextBox` inner-left content slot.** `InnerLeftContent` renders an icon inside the field; the slot is padded off the rounded corner and collapses when empty, so a plain `TextBox` keeps its 12px text inset unchanged.

### Fixed
- **`Collapsible` now actually expands.** The trigger resolved its target only by walking visual ancestors, but the demo placed trigger and container as siblings, so clicks no-op'd. Reworked to the shadcn structure: `Collapsible` hosts the header (`Trigger` slot) plus content, `CollapsibleTrigger` is a transparent `asChild` wrapper over a ghost `Button` (single hover surface), the rounded panel takes a muted background while open, and the chevron rotates 180° from the trigger's `:expanded` state.
- **Title bar double-click toggles maximize/restore.** A single press still begins the move-drag, but the maximize double-click is now handled via `DoubleTapped` — the Win32 move-drag modal loop was swallowing the second click.
- **`NumericUpDown` hover no longer double-pulses.** Dropped the background `BrushTransition`; the animated fade pulsed as the pointer crossed the gap between the adjacent up/down buttons, reading as a double animation. Hover tint is now instant, like the shadcn v4 buttons.
- **Scrollbar thumb no longer "tears" while being dragged.** The bar is only 10px wide, so during a vertical drag the captured pointer drifts off it, the viewer loses `:pointerover`, `AllowAutoHide` flips off, and the thumb shrank 8→4px and dimmed mid-drag. The `:pressed` state now pins the thumb expanded and opaque, so a drag keeps it stable regardless of pointer-over.

## [1.0.6] — 2026-06-14

### Fixed
- **`TextBox` / `SelectableTextBlock` selected text now inverts to dark.** The selection band painted correctly but the text on top kept its light foreground — a stale `TextRunCache` meant the selected run never re-rendered with the inverted brush. The cache is now busted on selection change so selected glyphs flip to the dark foreground over the primary band.
- **`OtpInput` digit now centers in the fixed-size cell.** The shared `TextBox` template insets `PART_Border` by the 3px focus-ring reserve and keeps the 12,8 field padding — on a fixed 40×40 OTP cell that clipped and offset the single digit. `.otp-cell` now drops the reserve inset (the ring renders outward into the 8px inter-cell gap) and zeroes the inner-content margin; the cell sets `FontSize`/`Padding`/`HorizontalContentAlignment` so the glyph sits dead-center.
- **`SidebarMenuItem` (expandable item) hover now matches plain `SidebarItem`s.** The toggle row is a default `Button`, whose own theme painted its inner border with `ShadowPrimaryHoverBrush` (a bright near-white in dark palettes) — overriding the intended sidebar accent and flashing a stray highlight on hover. The toggle's border now uses `ShadowSidebarAccentBrush`, identical to the subtle hover on non-expandable items.

## [1.0.5] — 2026-06-12

### Added
- **Visible text selection in `TextBox` and `SelectableTextBlock`.** Selected text now shows a primary-colored selection band with inverted (dark) foreground, so highlighted text stays legible.

## [1.0.4] — 2026-06-11

### Documentation
- README install snippet updated to the current version (`PackageReference` pin).

## [1.0.3] — 2026-06-11

### Added
- **`Button` `active` class** — persistent selected state (active tab, toggled tool button) that works on top of any variant: `Classes="ghost active"`. Declared after all variants, so it wins regardless of class order. Previously combining two variant classes (e.g. `ghost` + `secondary`) silently resolved to whichever was defined later in the theme — variants are now documented as mutually exclusive, `active` is the supported way to mark selection.
- Hover tokens in all 13 palettes: `ShadowPrimaryHoverBrush` (primary @ 90%), `ShadowSecondaryHoverBrush` (secondary @ 80%), `ShadowDestructiveButtonHoverBrush` (destructive @ 90%).
- **`ColorPicker` `swatch` format** — `Classes="swatch"` collapses the trigger to a single square colour chip (no hex label) for opening the palette.

### Changed
- **`Button` hover no longer dims the whole button** (`Opacity 0.9` + 0.15s fade): the animated dim pulsed when the pointer crossed gaps between adjacent buttons and dimmed the label along with the background. Hover/press now instantly tint the background per variant (shadcn v4 behavior): default/secondary/destructive use the new hover tokens, outline/ghost keep the accent fill, link only underlines.
- **Input focus ring is now reserved inside the control bounds.** The 3px ring previously rendered *outside* the control via a negative margin (relying on `ClipToBounds=False`), which ancestor containers (a page `ScrollViewer`, `Card`, or a field flush against a padded edge) still clipped — so the ring was "swallowed" on the left/top. The ring band is now reserved inside the box (`TextBox`): the input's outer box grows ~6px (W/H) while the visible field size is unchanged, and the ring renders fully in any layout. New token `ShadowFocusRingReserve`.
- **`ColorPicker` trigger restyled as an input field.** It was a default `Button`, so hover tinted it with the primary brush (a stray colour fill); it now matches Select/inputs — transparent background, input border, subtle input hover, focus ring.

### Fixed
- **`TextBox` no longer shrinks width on the first keystroke** when a `Watermark` is set — the placeholder now reserves its width (hidden via opacity instead of collapsing), so an auto-sized field stays put. An explicit `Width` still takes priority.
- **Vertical text centring** — `Badge`, `TabItem`/`TabStripItem`, `ListBoxItem`, `ShadowItem`, `ComboBoxItem`, `ToolTip`, `MenubarItem`, the top-level `Menu` item, and `Table` data cells sat ~1px below optical centre (the content line box centres, leaving the cap-to-baseline glyph slightly low); top padding trimmed by 1px to centre them, matching the existing `Button` treatment.

### Documentation
- Documented the wave-2 styled built-in controls in `README.md` and `docs/components.md` (`TreeView`, `Expander`, `SplitButton`/`ToggleSplitButton`, `DropDownButton`, `HyperlinkButton`, `TabStrip`, `SplitView`, `GroupBox`, `Menu`, plus `RepeatButton`/`ButtonSpinner`/`SelectableTextBlock`/`HeaderedContentControl`), and the `Tabs.UniformContentHeight` attached property.

## [1.0.2] — 2026-06-11

### Fixed
- **`.textarea` focus ring is now 2px** (was the shared 3px): a full-width 3px ring around a tall multiline box read heavy; the ring geometry is adjusted to match (`Margin -2`, outer radius `Md`). The 1px resting border is unchanged, and other inputs keep the 3px ring.

## [1.0.1] — 2026-06-11

### Added
- **Second wave of standalone-theme coverage** — themes for every remaining basic Avalonia control that rendered nothing without FluentTheme:
  - `Menu` — native menu bar with a dedicated top-level item theme (dropdown opens down; the default `MenuItem` theme stays for submenu items).
  - `RepeatButton` — inherits the full `Button` look including variant/size classes.
  - `ButtonSpinner` (standalone) — input-style frame with the NumericUpDown spin buttons.
  - `HyperlinkButton` — the shadcn "link" variant with hover underline.
  - `DropDownButton` — outline trigger with chevron; `.sm`/`.lg`/`.ghost` classes.
  - `SplitButton`/`ToggleSplitButton` — outline two-segment button, checked = solid primary.
  - `Expander` — bordered card with rotating-chevron trigger, all four expand directions.
  - `TabStrip`/`TabStripItem` — same pill-list look as the ShadowUI `TabControl`.
  - `GroupBox` — labeled card (no WinForms border-gap).
  - `SplitView` — structural FluentTheme port (all placements/display modes, pane separator, light-dismiss scrim).
  - `TreeView`/`TreeViewItem` — ListBox-like rows with accent hover/selection and indent.
  - `SelectableTextBlock` — visible selection brush + I-beam cursor.
  - `HeaderedContentControl` — plain header + content layout.
- `Tabs.UniformContentHeight` attached property — the content area reserves the height of the tallest tab page, so switching tabs no longer changes the TabControl height.

### Changed
- **All field-like controls share one (transparent) background:** `TextBox`, `NumericUpDown`, `SearchableComboBox` and `MultiSelectComboBox` switched from `ShadowInputBackgroundBrush` to `Transparent`, matching `ComboBox` and `DatePicker`; hover fill (`ShadowInputHoverBrush`) is unchanged.
- `.textarea` corner radius reduced to `ShadowRadiusSm` — radius Md on a tall multiline box reads larger than on a 36px field.

### Fixed
- **`ColorPicker` hex label showed garbage for named colors** (`#CK` for black, `#TE` for white, `#NSPARENT` for transparent): the trigger label was derived from `Color.ToString()`, which returns known-color *names*, not always `#aarrggbb`. The label is now formatted from the R/G/B components directly.
- **Text sat 1px below optical center in `Button` (all variants/sizes) and `Toggle`:** with an even-height content slot the 17px Inter text line rounds downward; vertical padding is now asymmetric (top 1px smaller) so the slot height is odd and the line lands exactly on center.
- **`InputGroup` looked broken:** the inner `TextBox` kept its full rounded corners (and visible gaps) next to the prefix/suffix slots; the group now squares the adjacent corners via `input-group-first/middle/last` classes (same approach as `ButtonGroup`).
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
