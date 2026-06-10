# ShadowUI — Component Reference

All components are drop-in: adding `ShadowUITheme` automatically styles built-in Avalonia
controls (`Button`, `TextBox`, `CheckBox`…) and unlocks custom controls from the `shadui:` namespace.

```xml
<Window xmlns:shadui="using:ShadowUI" ...>
```

Variants and sizes are set via `Classes` (like utility classes in shadcn).

---

## Button

```xml
<Button Content="Default" />
<Button Classes="secondary"   Content="Secondary" />
<Button Classes="destructive" Content="Destructive" />
<Button Classes="outline"     Content="Outline" />
<Button Classes="ghost"       Content="Ghost" />
<Button Classes="link"        Content="Link" />

<!-- sizes -->
<Button Classes="sm" Content="Small" />
<Button Classes="lg" Content="Large" />
```

### Icon Button

Square button for icons — use the `icon` class, combinable with any variant:

```xml
<Button Classes="ghost icon" ToolTip.Tip="Settings">
  <Viewbox Width="18" Height="18">
    <Path Data="..." Stretch="None" Width="24" Height="24"
          Stroke="{DynamicResource ShadowForegroundBrush}"
          StrokeThickness="2" StrokeLineCap="Round" />
  </Viewbox>
</Button>

<Button Classes="outline icon" .../>
<Button Classes="destructive icon" .../>
```

---

## Badge

```xml
<shadui:Badge Content="Default" />
<shadui:Badge Classes="secondary"   Content="Secondary" />
<shadui:Badge Classes="destructive" Content="Destructive" />
<shadui:Badge Classes="outline"     Content="Outline" />

<!-- semantic -->
<shadui:Badge Classes="success" Content="Success" />
<shadui:Badge Classes="warning" Content="Warning" />
<shadui:Badge Classes="info"    Content="Info" />
```

`success/warning/info` colors come from semantic tokens
(`ShadowSuccessBrush`, `ShadowWarningBrush`, `ShadowInfoBrush`) — palette-independent,
with Light and Dark variants.

---

## Card

```xml
<shadui:Card Width="320">
  <StackPanel Spacing="6">
    <shadui:CardTitle Content="Card Title" />
    <shadui:CardDescription Content="Description below the title." />
    <Separator />
    <TextBlock Classes="p" Text="Card content." />
  </StackPanel>
</shadui:Card>
```

---

## Forms

```xml
<Label Content="Email" />
<TextBox PlaceholderText="you@example.com" />
<TextBox Classes="textarea" PlaceholderText="Multiline…" />

<CheckBox Content="Accept terms" IsChecked="True" />
<ToggleSwitch Content="Notifications" IsChecked="True" />

<RadioButton GroupName="g" Content="One" IsChecked="True" />
<RadioButton GroupName="g" Content="Two" />

<ToggleButton Content="Bold" />
<ToggleButton Classes="outline" Content="Italic" />

<Slider Minimum="0" Maximum="100" Value="40" />
<ProgressBar Minimum="0" Maximum="100" Value="65" />

<ComboBox PlaceholderText="Select…">
  <ComboBoxItem>Option A</ComboBoxItem>
  <ComboBoxItem>Option B</ComboBoxItem>
</ComboBox>
```

---

## Tabs

By default — segmented tabs (muted background container, active tab has a border highlight without fill), matching current shadcn style:

```xml
<TabControl>
  <TabItem Header="Overview"><TextBlock Text="…" /></TabItem>
  <TabItem Header="Analytics"><TextBlock Text="…" /></TabItem>
</TabControl>

<!-- legacy: active tab with background fill + shadow -->
<TabControl Classes="legacytabs"> … </TabControl>

<!-- underline: tabs with underline indicator -->
<TabControl shadui:Tabs.Underline="True"> … </TabControl>

<!-- large equal-width tabs (border; fill = largetabs legacytabs) -->
<TabControl Classes="largetabs"> … </TabControl>
```

- default — border highlight (current shadcn);
- `Classes="legacytabs"` — active tab fill (classic look);
- `shadui:Tabs.Underline="True"` — underline indicator;
- `Classes="largetabs"` — large equal tabs (border), `largetabs legacytabs` — large with fill.

`shadui:Tabs.UniformContentHeight="True"` — the content area reserves the height of the tallest tab page, so switching tabs no longer resizes the `TabControl`:

```xml
<TabControl shadui:Tabs.UniformContentHeight="True"> … </TabControl>
```

---

## Popover / Menu / Context Menu

```xml
<Button Content="Popover">
  <Button.Flyout>
    <Flyout Placement="Bottom"> … </Flyout>
  </Button.Flyout>
</Button>

<Button Content="Menu">
  <Button.Flyout>
    <MenuFlyout>
      <MenuItem Header="Profile" />
      <MenuItem Classes="separator" />
      <MenuItem Header="Sign Out" />
    </MenuFlyout>
  </Button.Flyout>
</Button>

<Border>
  <Border.ContextMenu>
    <ContextMenu><MenuItem Header="Back" /></ContextMenu>
  </Border.ContextMenu>
</Border>
```

---

## Dialog

`shadui:Dialog` — full-window overlay + centered card. Closes on the X button, click outside, or `Esc`.

Place the dialog in the **root container** of your window (e.g. root `Grid`) so it overlays the entire UI:

```xml
<Grid>
  <DockPanel> … main UI … </DockPanel>

  <shadui:Dialog x:Name="ConfirmDialog"
                 Title="Delete Project?"
                 Description="This action cannot be undone.">
    <!-- body (Content) -->
    <StackPanel Spacing="8">
      <Label Content="Project name" />
      <TextBox PlaceholderText="my-project" />
    </StackPanel>

    <!-- footer with actions -->
    <shadui:Dialog.Footer>
      <StackPanel Orientation="Horizontal" Spacing="8">
        <Button Classes="outline" Content="Cancel" Click="OnCancel" />
        <Button Classes="destructive" Content="Delete" Click="OnDelete" />
      </StackPanel>
    </shadui:Dialog.Footer>
  </shadui:Dialog>
</Grid>
```

Control from code:

```csharp
ConfirmDialog.Open();    // or ConfirmDialog.IsOpen = true;
ConfirmDialog.Close();
```

| Property | Description |
|----------|-------------|
| `IsOpen` | open/closed state (TwoWay) |
| `Title` | title text |
| `Description` | subtitle below the title (wraps) |
| `Content` | dialog body |
| `Footer` | action area (right-aligned) |
| `ShowCloseButton` | show X button (default `true`) |

---

## Toast / Notifications

Transient notifications in the bottom-right corner. Stackable, auto-dismissing,
close button, colored accent by type. Triggered via static `Toast.Show`.

```csharp
using ShadowUI;

// anchor — any control in the current window (e.g. this)
Toast.Show(this, "Saved", "Changes applied.");
Toast.Show(this, "Done",    "Project deployed.",          ToastType.Success);
Toast.Show(this, "Warning", "Running out of space.",       ToastType.Warning);
Toast.Show(this, "Update",  "A new version is available.", ToastType.Info);
Toast.Show(this, "Error",   "Failed to connect.",          ToastType.Error);

// duration in seconds (default 4)
Toast.Show(this, "Long", "Stays for 10 seconds.", seconds: 10);

// position (default bottom-right)
Toast.Show(this, "Top", "Top-right corner.", position: ToastPosition.TopRight);

// change default position globally
Toast.DefaultPosition = ToastPosition.TopCenter;
```

`ToastType`: `Default`, `Success`, `Warning`, `Info`, `Error` — sets the accent bar color.

`ToastPosition`: `BottomRight` (default), `BottomLeft`, `TopRight`, `TopLeft`,
`TopCenter`, `BottomCenter`. Each position has its own stack; hosts are created
automatically in the window's `OverlayLayer` on first call.

---

## Sidebar

```xml
<shadui:Sidebar x:Name="Nav">
  <shadui:Sidebar.Header>
    <TextBlock Text="ShadowUI" FontWeight="SemiBold" />
  </shadui:Sidebar.Header>

  <shadui:SidebarGroup Header="Platform">
    <shadui:SidebarItem Content="Home" IsActive="True">
      <shadui:SidebarItem.Icon>
        <Viewbox Width="14" Height="14"><Path Data="…" /></Viewbox>
      </shadui:SidebarItem.Icon>
    </shadui:SidebarItem>

    <!-- expandable item with sub-items -->
    <shadui:SidebarMenuItem Header="Projects" IsExpanded="True">
      <shadui:SidebarMenuItem.Icon>
        <Viewbox Width="14" Height="14"><Path Data="…" /></Viewbox>
      </shadui:SidebarMenuItem.Icon>
      <shadui:SidebarItem Content="Website" />
      <shadui:SidebarItem Content="API" />
    </shadui:SidebarMenuItem>
  </shadui:SidebarGroup>

  <shadui:Sidebar.Footer>
    <shadui:Avatar Fallback="JD" />
  </shadui:Sidebar.Footer>
</shadui:Sidebar>
```

- `Sidebar.IsCollapsed` — icon mode (52px wide): labels hide, icons center, sub-menus collapse. Built-in hamburger button in the header toggles this.
- `Sidebar.ShowToggleButton` — show the collapse button (default `true`). Set to `False` to always keep the sidebar expanded.
- `Sidebar.ExpandedWidth` / `Sidebar.CollapsedWidth` — widths for expanded (240) and collapsed (52) states, with animated transition.

```xml
<shadui:Sidebar ExpandedWidth="280" CollapsedWidth="64"> … </shadui:Sidebar>
```

- `SidebarItem` — leaf item (`Content`, `Icon`, `IsActive`).
- `SidebarGroup` — group with optional `Header`.
- `SidebarMenuItem` — expandable item (`Header`, `Icon`, `IsExpanded`) with nested `SidebarItem` children; draws a vertical guide line on the left.

---

## TitleBar

`shadui:TitleBar` replaces the native title bar with a custom one styled like the sidebar
(same background, min/max/close buttons with inset hover). Drag, minimize, maximize, and close are built-in.

```xml
<DockPanel>
  <shadui:TitleBar DockPanel.Dock="Top" Title="My App" />
  <!-- window content -->
</DockPanel>
```

With icon:

```xml
<shadui:TitleBar Title="My App">
  <shadui:TitleBar.Icon>
    <Image Source="/Assets/icon.png" />
  </shadui:TitleBar.Icon>
</shadui:TitleBar>

<!-- or SVG icon -->
<shadui:TitleBar Title="My App">
  <shadui:TitleBar.Icon>
    <Viewbox>
      <Path Data="M12 2L2 7l10 5 10-5-10-5z"
            Fill="{DynamicResource ShadowForegroundBrush}" />
    </Viewbox>
  </shadui:TitleBar.Icon>
</shadui:TitleBar>
```

| Property | Description |
|----------|-------------|
| `Title` | title text |
| `Icon` | icon to the left of title (`Image`, `Viewbox`, `Path`, or any visual) |
| `ShowMaximize` | show maximize/restore button (default `true`) |

---

## Alert

```xml
<shadui:Alert Title="Notice" Description="Notification text." />
<shadui:Alert Classes="info"        Title="Info"    Description="…" />
<shadui:Alert Classes="success"     Title="Success" Description="…" />
<shadui:Alert Classes="warning"     Title="Warning" Description="…" />
<shadui:Alert Classes="destructive" Title="Error"   Description="…" />
```

Properties: `Title`, `Description`, `Icon`. Variants color the border and title with semantic color.

---

## AlertDialog

Like `Dialog`, but **does not close** on click outside or `Esc` (no X button) —
only via explicit buttons. Place in the root `Grid`.

```xml
<shadui:AlertDialog x:Name="Confirm"
                    Title="Are you sure?"
                    Description="This action cannot be undone.">
  <shadui:AlertDialog.Footer>
    <StackPanel Orientation="Horizontal" Spacing="8">
      <Button Classes="outline" Content="Cancel" Click="OnCancel" />
      <Button Classes="destructive" Content="Continue" Click="OnConfirm" />
    </StackPanel>
  </shadui:AlertDialog.Footer>
</shadui:AlertDialog>
```

`Confirm.Open()` / `Confirm.Close()`. Same properties as `Dialog`, plus
`CloseOnClickOutside` and `CloseOnEscape` (both `false` on AlertDialog).

---

## Accordion

```xml
<shadui:Accordion>
  <shadui:AccordionItem Header="Section 1" IsExpanded="True">
    <TextBlock Text="Content of the first section." />
  </shadui:AccordionItem>
  <shadui:AccordionItem Header="Section 2">
    <TextBlock Text="Content of the second section." />
  </shadui:AccordionItem>
</shadui:Accordion>
```

`AccordionItem`: `Header`, `IsExpanded`, `Content`. Chevron rotates, sections are independent.

---

## Breadcrumb

Composed from `BreadcrumbItem` and `BreadcrumbSeparator` (like shadcn):

```xml
<shadui:Breadcrumb>
  <shadui:BreadcrumbItem Content="Home" Click="..." />
  <shadui:BreadcrumbSeparator />
  <shadui:BreadcrumbItem Content="Section" Click="..." />
  <shadui:BreadcrumbSeparator />
  <shadui:BreadcrumbItem Content="Current" Classes="current" />
</shadui:Breadcrumb>
```

`BreadcrumbItem` is a link (`Button`); `current` class marks the active page.

---

## Spinner

```xml
<shadui:Spinner Classes="sm" />
<shadui:Spinner />
<shadui:Spinner Classes="lg primary" />
```

Sizes: `sm` / (default) / `lg`; class `primary` — accent color.

---

## Toggle Group

Single-selection (radio-like) — segments sharing a `GroupName`:

```xml
<shadui:ToggleGroup>
  <shadui:ToggleGroupItem GroupName="align" Content="Left"   IsChecked="True" />
  <shadui:ToggleGroupItem GroupName="align" Content="Center" />
  <shadui:ToggleGroupItem GroupName="align" Content="Right"  />
</shadui:ToggleGroup>
```

---

## Color Picker

```xml
<shadui:ColorPicker SelectedColor="#2563EB" />
```

Swatch button + popover with a full spectrum (`ColorView`): hue/saturation field,
sliders, hex input. `SelectedColor` is TwoWay.

---

## Table (base styles)

Style classes applied to `Border` + `Grid` (shared column definitions via `ColumnDefinitions`):

```xml
<Border Classes="table">
  <StackPanel>
    <Border Classes="thead">
      <Grid ColumnDefinitions="*,*,Auto">
        <TextBlock Grid.Column="0" Classes="th" Text="Name" />
        <TextBlock Grid.Column="1" Classes="th" Text="Status" />
        <TextBlock Grid.Column="2" Classes="th" Text="Amount" />
      </Grid>
    </Border>
    <Border Classes="tr">
      <Grid ColumnDefinitions="*,*,Auto">
        <TextBlock Grid.Column="0" Classes="td" Text="Ivan" />
        <TextBlock Grid.Column="1" Classes="td" Text="Active" />
        <TextBlock Grid.Column="2" Classes="td" Text="$250" />
      </Grid>
    </Border>
    <Border Classes="tr last"> … </Border>  <!-- last: no bottom border -->
  </StackPanel>
</Border>
```

Classes: `table` (wrapper), `thead` (header), `tr` / `tr last` (rows with hover highlight), `th` / `td` (cells).

---

## ShadowDataTable

Full-featured data table with column sorting, row filtering, and pagination:

```xml
<shadui:ShadowDataTable x:Name="MyTable" PageSize="10" />
```

```csharp
MyTable.Columns = new[]
{
    new DataTableColumn { Header = "Name",   CanSort = true  },
    new DataTableColumn { Header = "Role",   CanSort = true  },
    new DataTableColumn { Header = "Status", CanSort = true  },
    new DataTableColumn { Header = "Date",   CanSort = false },
};
MyTable.Rows = new[]
{
    new[] { "Alice", "Admin",  "Active", "2025-06-01" },
    new[] { "Bob",   "Editor", "Active", "2025-05-15" },
};
```

| Property | Description |
|----------|-------------|
| `Columns` | `IEnumerable<DataTableColumn>` — column definitions |
| `Rows` | `IEnumerable<string[]>` — row data |
| `PageSize` | rows per page (default 10) |

---

## ShadowPagination

Standalone pagination component:

```xml
<shadui:ShadowPagination x:Name="Pager"
                         TotalPages="10"
                         CurrentPage="3"
                         CurrentPageChanged="OnPageChanged" />
```

---

## CommandPalette

⌘K command palette with fuzzy search and keyboard navigation (↑↓ Enter Esc).
Place in the root `Grid`:

```xml
<shadui:CommandPalette x:Name="Palette" />
```

```csharp
Palette.Items = new List<CommandItem>
{
    new() { Header = "Open File",  Description = "Ctrl+O", Group = "File" },
    new() { Header = "Save File",  Description = "Ctrl+S", Group = "File" },
    new() { Header = "Copy",       Description = "Ctrl+C", Group = "Edit" },
    new() { Header = "Dark Theme", Description = "",       Group = "Settings" },
};

// open programmatically (e.g. on Ctrl+K)
Palette.Open();
```

---

## Sheet / Drawer

Slide-in panel from any side. Place in the root `Grid`:

```xml
<shadui:Sheet x:Name="RightPanel"
              Title="Panel Title"
              Description="Optional description."
              Side="Right">
  <!-- panel content -->
  <TextBlock Text="Panel content here." />
</shadui:Sheet>
```

`Side`: `Right` (default), `Left`, `Top`, `Bottom`.

```csharp
RightPanel.Open();
RightPanel.Close();
```

---

## Collapsible

```xml
<shadui:CollapsibleTrigger x:Name="Trigger">
  <Button Classes="outline" Content="Toggle ▸" />
</shadui:CollapsibleTrigger>
<shadui:Collapsible x:Name="Panel">
  <TextBlock Text="Hidden content revealed on trigger click." />
</shadui:Collapsible>
```

Link them in code-behind (when placed as siblings):

```csharp
Trigger.AddHandler(PointerPressedEvent,
    (_, _) => Panel.IsExpanded = !Panel.IsExpanded,
    RoutingStrategies.Bubble);
```

---

## SearchableComboBox

ComboBox with item filtering by typed input:

```xml
<shadui:SearchableComboBox PlaceholderText="Select a fruit...">
  <ComboBoxItem>Apple</ComboBoxItem>
  <ComboBoxItem>Banana</ComboBoxItem>
  <ComboBoxItem>Cherry</ComboBoxItem>
</shadui:SearchableComboBox>
```

---

## OtpInput

One-time password input — focus moves automatically between digits:

```xml
<shadui:OtpInput x:Name="Otp" Length="6" InputType="Digit" />
```

```csharp
string code = Otp.Value; // e.g. "123456"
```

`InputType`: `Digit` (numbers only), `Any` (any character).

---

## InputGroup

TextBox with prefix and/or suffix decorators:

```xml
<shadui:InputGroup Width="280">
  <shadui:InputGroup.Prefix>
    <TextBlock Text="@" Foreground="{DynamicResource ShadowMutedForegroundBrush}" />
  </shadui:InputGroup.Prefix>
  <TextBox PlaceholderText="username" />
  <shadui:InputGroup.Suffix>
    <TextBlock Text=".com" Foreground="{DynamicResource ShadowMutedForegroundBrush}" />
  </shadui:InputGroup.Suffix>
</shadui:InputGroup>
```

---

## ButtonGroup

Groups buttons with shared borders — only outer edges are rounded, no double borders:

```xml
<shadui:ButtonGroup>
  <Button Classes="outline" Content="Left" />
  <Button Classes="outline" Content="Center" />
  <Button Classes="outline" Content="Right" />
</shadui:ButtonGroup>
```

---

## Field

Control wrapper with Label, hint text, required marker, and error message:

```xml
<shadui:Field Label="Email"
              HintText="We won't share your address."
              IsRequired="True">
  <TextBox PlaceholderText="name@example.com" />
</shadui:Field>
```

Show / clear validation error:

```csharp
MyField.ErrorMessage = "Invalid email address.";
MyField.ErrorMessage = null; // clear
```

---

## ShadowCalendar / DatePicker

```xml
<!-- single date selection -->
<shadui:ShadowCalendar />

<!-- date range selection -->
<shadui:ShadowCalendar Mode="Range" />

<!-- text input + calendar popup -->
<shadui:DatePicker Width="240" />
```

`ShadowCalendar` properties: `SelectedDate`, `SelectedStartDate`, `SelectedEndDate` (for Range mode), `Mode` (`Single` / `Range`).

---

## NavigationMenu

```xml
<shadui:NavigationMenu>
  <shadui:NavigationMenuItem Header="Products">
    <StackPanel Spacing="8">
      <TextBlock Text="Overview" />
      <TextBlock Text="Documentation" />
    </StackPanel>
  </shadui:NavigationMenuItem>
  <shadui:NavigationMenuItem Header="Company" />
</shadui:NavigationMenu>
```

---

## Menubar

```xml
<shadui:Menubar>
  <shadui:MenubarItem Header="File">
    <shadui:MenubarItem.Items>
      <MenuItem Header="New" />
      <MenuItem Header="Open" />
      <MenuItem Classes="separator" />
      <MenuItem Header="Exit" />
    </shadui:MenubarItem.Items>
  </shadui:MenubarItem>
  <shadui:MenubarItem Header="Edit">
    <shadui:MenubarItem.Items>
      <MenuItem Header="Undo" />
      <MenuItem Header="Redo" />
    </shadui:MenubarItem.Items>
  </shadui:MenubarItem>
</shadui:Menubar>
```

---

## HoverCard

Shows a card on hover, hides on mouse leave:

```xml
<shadui:HoverCard>
  <shadui:HoverCard.TriggerContent>
    <Button Classes="outline" Content="Hover me" />
  </shadui:HoverCard.TriggerContent>
  <StackPanel Spacing="4">
    <TextBlock Classes="large" Text="HoverCard" />
    <TextBlock Classes="muted" Text="Appears on hover." />
  </StackPanel>
</shadui:HoverCard>
```

---

## MultiSelectComboBox

Multi-select combobox: selected items render as removable chips, the dropdown has a search box
and stays open while toggling items (shadcn Combobox multi-select pattern):

```xml
<shadui:MultiSelectComboBox x:Name="Frameworks"
                            Width="320"
                            PlaceholderText="Select frameworks..." />
```

```csharp
Frameworks.ItemsSource = new[] { "Avalonia", "WPF", "MAUI" };
Frameworks.SelectedItems.Add("Avalonia");           // программный выбор
Frameworks.SelectionChanged += (_, _) => { ... };   // реакция на изменения
```

Properties: `ItemsSource`, `SelectedItems` (ObservableCollection), `IsDropDownOpen` (two-way),
`PlaceholderText`, `SearchPlaceholder`, `MaxDropDownHeight`.

---

## Popover

Click-toggled floating card anchored to a trigger (light dismiss on outside click):

```xml
<shadui:Popover>
  <shadui:Popover.TriggerContent>
    <Button Content="Open Popover" />
  </shadui:Popover.TriggerContent>
  <StackPanel Spacing="8" Width="240">
    <TextBlock Classes="large" Text="Dimensions" />
    <TextBlock Classes="muted" Text="Set the layer dimensions." />
    <TextBox PlaceholderText="Width" />
  </StackPanel>
</shadui:Popover>
```

Properties: `IsOpen` (two-way), `Placement` (default `BottomEdgeAlignedLeft`).

---

## Resizable

Two panels with a draggable divider:

```xml
<shadui:Resizable>
  <shadui:Resizable.FirstContent>
    <Border Padding="16">
      <TextBlock Classes="h4" Text="Left Panel" />
    </Border>
  </shadui:Resizable.FirstContent>
  <shadui:Resizable.SecondContent>
    <Border Padding="16">
      <TextBlock Classes="h4" Text="Right Panel" />
    </Border>
  </shadui:Resizable.SecondContent>
</shadui:Resizable>
```

`Orientation`: `Horizontal` (default, vertical divider) / `Vertical` (horizontal divider).

---

## Carousel

```xml
<shadui:Carousel x:Name="Slider" Height="200" Width="480" />
```

```csharp
Slider.Items = new object[]
{
    "Slide 1 content",
    "Slide 2 content",
    // or any UIElement
};
```

Prev/next buttons and dot navigation are built-in.

---

## BarChart / LineChart

```xml
<shadui:BarChart  x:Name="Bar"  Height="200" Width="480" />
<shadui:LineChart x:Name="Line" Height="200" Width="480" />
```

```csharp
var points = new[]
{
    new ChartPoint("Jan", 42),
    new ChartPoint("Feb", 68),
    new ChartPoint("Mar", 55),
};
Bar.Points  = points;
Line.Points = points;
```

Hover tooltip is built-in.

---

## AreaChart / PieChart

```xml
<shadui:AreaChart x:Name="Area" Height="200" Width="480" />
<shadui:PieChart  x:Name="Pie"  Height="220" Width="220" />
```

```csharp
Area.Points = points;                       // line + 25% fill
Pie.Points = new[]
{
    new ChartPoint("Chrome", 62),
    new ChartPoint("Safari", 19),
    new ChartPoint("Firefox", 8),
};
```

`PieChart` is a donut by default (`InnerRadiusRatio="0.6"`; set `0` for a solid pie).
Slices cycle through `ShadowChart1..5Brush`. Hover tooltip is built-in.

---

## EmptyState

```xml
<shadui:EmptyState Title="No Data"
                   Description="Nothing here yet. Add your first item."
                   ActionContent="Add Item">
  <shadui:EmptyState.Icon>
    <Viewbox Width="40" Height="40">
      <Path Data="M 12 2 L 12 22 M 2 12 L 22 12" ... />
    </Viewbox>
  </shadui:EmptyState.Icon>
</shadui:EmptyState>
```

---

## ShadowItem

Universal list item with icon, primary/secondary text, and trailing action:

```xml
<shadui:ShadowItem PrimaryText="Alice Smith"
                   SecondaryText="alice@example.com">
  <shadui:ShadowItem.Icon>
    <Viewbox Width="20" Height="20"><Path Data="…" /></Viewbox>
  </shadui:ShadowItem.Icon>
  <shadui:ShadowItem.TrailingAction>
    <Button Classes="ghost sm" Content="···" />
  </shadui:ShadowItem.TrailingAction>
</shadui:ShadowItem>
```

---

## Styled built-in controls

These are standard Avalonia controls — adding `ShadowUITheme` styles them automatically,
no `shadui:` prefix needed. They have no custom API beyond Avalonia's own.

### TreeView

```xml
<TreeView>
  <TreeViewItem Header="src" IsExpanded="True">
    <TreeViewItem Header="Controls" />
    <TreeViewItem Header="Themes" />
  </TreeViewItem>
  <TreeViewItem Header="tests" />
</TreeView>
```

ListBox-style rows with accent hover/selection and indent guides.

### Expander

```xml
<Expander Header="Advanced settings" IsExpanded="True">
  <TextBlock Text="Hidden until expanded." />
</Expander>
```

Bordered card with a rotating chevron; all four `ExpandDirection`s (`Down`/`Up`/`Left`/`Right`).

### SplitButton / ToggleSplitButton

Two-segment button — primary action + dropdown chevron:

```xml
<SplitButton Content="Save">
  <SplitButton.Flyout>
    <MenuFlyout>
      <MenuItem Header="Save As…" />
      <MenuItem Header="Save All" />
    </MenuFlyout>
  </SplitButton.Flyout>
</SplitButton>

<!-- ToggleSplitButton: checked segment = solid primary -->
<ToggleSplitButton Content="Bold" />
```

### DropDownButton

Outline trigger with chevron; `sm` / `lg` / `ghost` classes:

```xml
<DropDownButton Content="Options">
  <DropDownButton.Flyout>
    <MenuFlyout>
      <MenuItem Header="Rename" />
      <MenuItem Header="Delete" />
    </MenuFlyout>
  </DropDownButton.Flyout>
</DropDownButton>

<DropDownButton Classes="ghost sm" Content="More" />
```

### HyperlinkButton

The shadcn `link` look with hover underline:

```xml
<HyperlinkButton Content="Documentation" NavigateUri="https://example.com" />
```

### TabStrip

Pill-list tabs (same look as the ShadowUI `TabControl`), without content panels:

```xml
<TabStrip>
  <TabStripItem Content="All" />
  <TabStripItem Content="Active" />
  <TabStripItem Content="Archived" />
</TabStrip>
```

### SplitView

Collapsible side pane (all placements / display modes, light-dismiss scrim):

```xml
<SplitView IsPaneOpen="True" DisplayMode="CompactInline" CompactPaneLength="52" OpenPaneLength="240">
  <SplitView.Pane>
    <TextBlock Text="Pane content" />
  </SplitView.Pane>
  <TextBlock Text="Main content" />
</SplitView>
```

### GroupBox

Labeled card (no WinForms border-gap):

```xml
<GroupBox Header="Connection">
  <StackPanel Spacing="8">
    <TextBox Watermark="Host" />
    <NumericUpDown Value="5432" />
  </StackPanel>
</GroupBox>
```

### Menu

Native menu bar (top-level items open downward); submenu items use the standard `MenuItem` theme:

```xml
<Menu>
  <MenuItem Header="File">
    <MenuItem Header="New" />
    <MenuItem Header="Open" />
    <Separator />
    <MenuItem Header="Exit" />
  </MenuItem>
  <MenuItem Header="Edit">
    <MenuItem Header="Undo" />
    <MenuItem Header="Redo" />
  </MenuItem>
</Menu>
```

### Also styled

`RepeatButton` (inherits the full `Button` look + variant/size classes),
`ButtonSpinner`, `SelectableTextBlock` (visible selection brush + I-beam cursor),
`HeaderedContentControl` (plain header + content layout) — all drop-in, no extra markup.

---

## Misc

```xml
<shadui:Avatar Fallback="JD" />
<shadui:Skeleton Width="160" Height="40" />
<shadui:Kbd Content="Ctrl" />
<shadui:AspectRatio Ratio="1.777"> … </shadui:AspectRatio>
```

---

## Design Tokens (DynamicResource)

Core brushes for custom markup:

| Key | Purpose |
|-----|---------|
| `ShadowBackgroundBrush` / `ShadowForegroundBrush` | background / text |
| `ShadowPrimaryBrush` / `ShadowPrimaryForegroundBrush` | primary accent |
| `ShadowSecondaryBrush`, `ShadowMutedBrush`, `ShadowAccentBrush` | secondary surfaces |
| `ShadowDestructiveBrush`, `ShadowSuccessBrush`, `ShadowWarningBrush`, `ShadowInfoBrush` | semantic status |
| `ShadowBorderBrush`, `ShadowInputBrush` | borders / inputs |
| `ShadowSidebarBrush`, `ShadowSidebarForegroundBrush`, `ShadowSidebarAccentBrush`, `ShadowSidebarBorderBrush` | sidebar |
| `ShadowRadiusSm` / `ShadowRadiusMd` / `ShadowRadiusLg` / `ShadowRadiusXl` | corner radii |
| `ShadowShadowXs` / `ShadowShadowSm` / `ShadowShadowMd` | box shadows |
