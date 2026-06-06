# ShadowUI

A port of [shadcn/ui](https://ui.shadcn.com) for [Avalonia UI](https://avaloniaui.net) 12 / .NET 10.
50+ components · Light/Dark theme · 5 color palettes · Native AOT compatible.

## Screenshots

<p align="center">
  <img src="docs/screenshots/buttons-input.png" width="680" alt="ShadowUI Gallery" />
</p>

## Requirements

- .NET 10
- Avalonia 12.0.4+

## Installation

### NuGet Package Manager

```
Install-Package ShadowUI
```

### .NET CLI

```bash
dotnet add package ShadowUI
```

### PackageReference

```xml
<PackageReference Include="ShadowUI" Version="0.1.1-alpha" />
```

## Getting Started

**1. Add the theme to your `App.axaml`:**

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:shadui="using:ShadowUI"
             RequestedThemeVariant="Dark">
  <Application.Styles>
    <FluentTheme />
    <shadui:ShadowUITheme BaseColor="Zinc" />
  </Application.Styles>
</Application>
```

**2. Add the namespace to your views:**

```xml
<Window xmlns:shadui="using:ShadowUI" ...>
```

**3. Use components:**

```xml
<shadui:Card>
  <StackPanel Spacing="8">
    <shadui:CardTitle Content="Welcome" />
    <shadui:CardDescription Content="Get started with ShadowUI." />
    <Button Content="Save" />
    <Button Classes="secondary" Content="Cancel" />
  </StackPanel>
</shadui:Card>
```

**Switch theme at runtime:**

```csharp
Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
```

**Switch color palette at runtime:**

```csharp
// get the theme instance
var theme = Application.Current!.Styles.OfType<ShadowUITheme>().First();
theme.BaseColor = BaseColor.Slate;
```

Color palettes (`BaseColor`): `Zinc` (default), `Slate`, `Stone`, `Gray`, `Neutral`.

## Components

### Base
Button (6 variants + icon), Badge (+ success/warning/info), Card, Separator, Label,
TextBox / Textarea, CheckBox, Switch, RadioButton, Toggle, ToggleGroup, Slider,
ProgressBar, Avatar, Skeleton, Kbd, Tooltip, AspectRatio, Spinner, ColorPicker

### Navigation & Overlays
Tabs (underline / legacy / large), ComboBox (Select), Popover (Flyout),
Menu / DropdownMenu / ContextMenu, NavigationMenu, Menubar, HoverCard,
**Sidebar** (icon-collapsed mode, expandable groups), **TitleBar** (custom window title bar),
**Dialog**, **AlertDialog**, **Toast / Notifications** (6 positions, 5 types),
**CommandPalette** (⌘K, fuzzy search, keyboard nav), Sheet / Drawer, ScrollBar

### Forms & Input
SearchableComboBox, OtpInput, InputGroup, ButtonGroup, Field, ColorPicker

### Data & Tables
ShadowDataTable (sort, filter, pagination), ShadowPagination, Resizable,
Table (base styles)

### Content
Accordion, Alert (5 variants), AlertDialog, Breadcrumb, Collapsible,
EmptyState, ShadowItem

### Date & Time
ShadowCalendar (Single / Range), DatePicker

### Visual & Charts
Carousel (prev/next + dot navigation), BarChart, LineChart

## Design Tokens

Key `DynamicResource` brushes for custom markup:

| Key | Purpose |
|-----|---------|
| `ShadowBackgroundBrush` / `ShadowForegroundBrush` | background / text |
| `ShadowPrimaryBrush` / `ShadowPrimaryForegroundBrush` | primary accent |
| `ShadowMutedBrush`, `ShadowAccentBrush` | secondary surfaces |
| `ShadowDestructiveBrush`, `ShadowSuccessBrush`, `ShadowWarningBrush`, `ShadowInfoBrush` | semantic status |
| `ShadowBorderBrush`, `ShadowInputBrush` | borders / inputs |
| `ShadowSidebarBrush`, `ShadowSidebarForegroundBrush` | sidebar surfaces |
| `ShadowRadiusSm` / `ShadowRadiusMd` / `ShadowRadiusLg` / `ShadowRadiusXl` | corner radii |
| `ShadowShadowXs` / `ShadowShadowSm` / `ShadowShadowMd` | box shadows |

## Documentation

Full component reference with code examples — [`docs/components.md`](docs/components.md).

## Gallery

Run the interactive component gallery:

```bash
dotnet run --project samples/ShadowUI.Gallery/ShadowUI.Gallery.csproj
```

## Native AOT

```bash
dotnet publish tests/ShadowUI.AotSmokeTest/ShadowUI.AotSmokeTest.csproj -r win-x64 -c Release
```

> On Windows, Native AOT requires MSVC (C++ build tools). Run from Developer Command Prompt
> or add `…\Microsoft Visual Studio\Installer` to your `PATH`.

## Build & Test

```bash
dotnet build ShadowUI.slnx -c Debug
dotnet test tests/ShadowUI.UnitTests/ShadowUI.UnitTests.csproj
```
