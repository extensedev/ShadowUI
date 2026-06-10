using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ShadowUI;

/// <summary>A single command entry for the command palette.</summary>
public class CommandItem
{
    /// <summary>Command title.</summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>Short description or hint.</summary>
    public string? Description { get; set; }

    /// <summary>Group name; null means ungrouped.</summary>
    public string? Group { get; set; }

    /// <summary>Action invoked when the command is selected.</summary>
    public Action? Execute { get; set; }
}

/// <summary>Command group with a header and nested items.</summary>
public class CommandGroup
{
    /// <summary>Group header.</summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>Commands in this group.</summary>
    public IReadOnlyList<CommandItem> Items { get; set; } = Array.Empty<CommandItem>();
}

/// <summary>⌘K-style command palette: full-screen overlay with a search field and grouped command list.</summary>
public class CommandPalette : TemplatedControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<CommandPalette, bool>(nameof(IsOpen),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<IReadOnlyList<CommandItem>> ItemsProperty =
        AvaloniaProperty.Register<CommandPalette, IReadOnlyList<CommandItem>>(nameof(Items),
            defaultValue: Array.Empty<CommandItem>());

    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<CommandPalette, string?>(nameof(SearchText));

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<CommandPalette, int>(nameof(SelectedIndex), defaultValue: -1);

    public static readonly StyledProperty<string> SearchPlaceholderProperty =
        AvaloniaProperty.Register<CommandPalette, string>(nameof(SearchPlaceholder),
            defaultValue: "Type a command or search...");
#pragma warning restore CS1591

    /// <summary>Whether the palette is open.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>All available commands.</summary>
    public IReadOnlyList<CommandItem> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>Current search query.</summary>
    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>Search field placeholder.</summary>
    public string SearchPlaceholder
    {
        get => GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    /// <summary>Index of the selected command in the filtered list.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CommandPalette);

    static CommandPalette()
    {
        IsOpenProperty.Changed.AddClassHandler<CommandPalette>((cp, e) =>
        {
            var newValue = (bool)(e.NewValue ?? false);
            cp.PseudoClasses.Set(":open", newValue);
            if (newValue)
            {
                cp.SetCurrentValue(SelectedIndexProperty, -1);
                cp.SetCurrentValue(SearchTextProperty, null);
                cp._searchBox?.Focus();
            }
        });

        ItemsProperty.Changed.AddClassHandler<CommandPalette>((cp, _) => cp.ApplyFilter());
        SearchTextProperty.Changed.AddClassHandler<CommandPalette>((cp, _) => cp.ApplyFilter());
    }

    private ListBox? _listBox;
    private TextBox? _searchBox;
    private Border? _scrimBorder;
    private IReadOnlyList<CommandItem> _filteredItems = Array.Empty<CommandItem>();

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_searchBox is not null)
            _searchBox.RemoveHandler(TextBox.TextChangedEvent, OnSearchTextChanged);
        if (_scrimBorder is not null)
            _scrimBorder.RemoveHandler(PointerPressedEvent, OnScrimPressed);

        base.OnApplyTemplate(e);

        _scrimBorder = e.NameScope.Find<Border>("PART_Scrim");
        _searchBox   = e.NameScope.Find<TextBox>("PART_SearchBox");
        _listBox     = e.NameScope.Find<ListBox>("PART_ItemsListBox");
        var closeBtn = e.NameScope.Find<Button>("PART_CloseButton");

        if (_scrimBorder is not null)
            _scrimBorder.AddHandler(PointerPressedEvent, OnScrimPressed, RoutingStrategies.Tunnel);

        if (_searchBox is not null)
            _searchBox.AddHandler(TextBox.TextChangedEvent, OnSearchTextChanged);

        if (closeBtn is not null)
            closeBtn.AddHandler(Button.ClickEvent, (_, _) => Close());

        ApplyFilter();
        SetCurrentValue(IsOpenProperty, IsOpen);
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsOpen)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    e.Handled = true;
                    break;

                case Key.Up:
                {
                    var idx = SelectedIndex - 1;
                    if (idx < 0) idx = 0;
                    SetCurrentValue(SelectedIndexProperty, idx);
                    e.Handled = true;
                    break;
                }

                case Key.Down:
                {
                    var max = _filteredItems.Count - 1;
                    var idx = SelectedIndex + 1;
                    if (idx > max) idx = max;
                    SetCurrentValue(SelectedIndexProperty, idx);
                    e.Handled = true;
                    break;
                }

                case Key.Enter:
                    if (SelectedIndex >= 0 && SelectedIndex < _filteredItems.Count)
                    {
                        _filteredItems[SelectedIndex].Execute?.Invoke();
                        Close();
                        e.Handled = true;
                    }
                    break;
            }
        }

        base.OnKeyDown(e);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
            SetCurrentValue(SearchTextProperty, tb.Text);
    }

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ReferenceEquals(e.Source, sender))
            Close();
    }

    private void ApplyFilter()
    {
        var items = Items;
        if (items is null || items.Count == 0)
        {
            _filteredItems = Array.Empty<CommandItem>();
            if (_listBox is not null)
                _listBox.ItemsSource = _filteredItems;
            SetCurrentValue(SelectedIndexProperty, -1);
            return;
        }

        var searchText = SearchText;
        if (string.IsNullOrEmpty(searchText))
        {
            _filteredItems = items.ToList();
        }
        else
        {
            _filteredItems = items
                .Where(item =>
                    item.Header.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    (item.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        if (_listBox is not null)
            _listBox.ItemsSource = _filteredItems;

        SetCurrentValue(SelectedIndexProperty, -1);
    }

    private IEnumerable<CommandGroup> GetGroupedItems()
    {
        return _filteredItems
            .GroupBy(item => item.Group ?? string.Empty)
            .Select(g => new CommandGroup { Header = g.Key, Items = g.ToList() });
    }

    /// <summary>Open the command palette.</summary>
    public void Open() => SetCurrentValue(IsOpenProperty, true);

    /// <summary>Close the command palette.</summary>
    public void Close() => SetCurrentValue(IsOpenProperty, false);

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_searchBox is not null)
            _searchBox.RemoveHandler(TextBox.TextChangedEvent, OnSearchTextChanged);
        if (_scrimBorder is not null)
            _scrimBorder.RemoveHandler(PointerPressedEvent, OnScrimPressed);
        base.OnDetachedFromVisualTree(e);
    }
}
