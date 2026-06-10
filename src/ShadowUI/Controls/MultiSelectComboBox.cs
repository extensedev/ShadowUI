using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace ShadowUI;

/// <summary>
/// Multi-select combobox: selected items are shown as chips with a remove button,
/// and the searchable dropdown stays open on selection (analogous to shadcn Combobox multi-select).
/// </summary>
public class MultiSelectComboBox : TemplatedControl
{
#pragma warning disable CS1591
    /// <summary>Items registered property.</summary>
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, IEnumerable?>(nameof(ItemsSource));

    /// <summary>IsDropDownOpen registered property.</summary>
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, bool>(nameof(IsDropDownOpen),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>PlaceholderText registered property.</summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, string?>(nameof(PlaceholderText),
            defaultValue: "Select items...");

    /// <summary>SearchText registered property.</summary>
    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, string?>(nameof(SearchText));

    /// <summary>SearchPlaceholder registered property.</summary>
    public static readonly StyledProperty<string> SearchPlaceholderProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, string>(nameof(SearchPlaceholder),
            defaultValue: "Search...");

    /// <summary>MaxDropDownHeight registered property.</summary>
    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        AvaloniaProperty.Register<MultiSelectComboBox, double>(nameof(MaxDropDownHeight), 384);
#pragma warning restore CS1591

    /// <summary>Items source for selection.</summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Whether the dropdown is open. Supports two-way binding.</summary>
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>Placeholder shown when nothing is selected.</summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>List filter text.</summary>
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

    /// <summary>Maximum height of the dropdown list.</summary>
    public double MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>Selected items. Can be modified externally (Add/Remove/Clear).</summary>
    public ObservableCollection<object> SelectedItems { get; } = new();

    /// <summary>Raised when the set of selected items changes.</summary>
    public event EventHandler? SelectionChanged;

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(MultiSelectComboBox);

    private Border? _trigger;
    private Popup? _popup;
    private ListBox? _listBox;
    private TextBox? _searchBox;
    private WrapPanel? _chipsPanel;
    private TextBlock? _placeholder;
    private bool _syncing;

    static MultiSelectComboBox()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MultiSelectComboBox>((s, _) => s.OnItemsSourceChanged());
        SearchTextProperty.Changed.AddClassHandler<MultiSelectComboBox>((s, _) => s.ApplyFilter());
        IsDropDownOpenProperty.Changed.AddClassHandler<MultiSelectComboBox>((s, e) =>
        {
            if (s._popup is not null)
                s._popup.IsOpen = (bool)(e.NewValue ?? false);
            if (e.NewValue is false)
                s.SetCurrentValue(SearchTextProperty, null);
        });
    }

    /// <summary>Subscription to external SelectedItems changes.</summary>
    public MultiSelectComboBox()
    {
        SelectedItems.CollectionChanged += OnSelectedItemsChanged;
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_trigger is not null)
            _trigger.RemoveHandler(PointerPressedEvent, OnTriggerPressed);
        if (_listBox is not null)
            _listBox.SelectionChanged -= OnListBoxSelectionChanged;
        if (_searchBox is not null)
            _searchBox.RemoveHandler(TextBox.TextChangedEvent, OnSearchTextChanged);
        if (_popup is not null)
            _popup.Closed -= OnPopupClosed;

        base.OnApplyTemplate(e);

        _trigger = e.NameScope.Find<Border>("PART_Border");
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _listBox = e.NameScope.Find<ListBox>("PART_ListBox");
        _searchBox = e.NameScope.Find<TextBox>("PART_SearchBox");
        _chipsPanel = e.NameScope.Find<WrapPanel>("PART_ChipsPanel");
        _placeholder = e.NameScope.Find<TextBlock>("PART_Placeholder");

        if (_trigger is not null)
            _trigger.AddHandler(PointerPressedEvent, OnTriggerPressed);
        if (_listBox is not null)
        {
            _listBox.ItemsSource = ItemsSource;
            _listBox.SelectionChanged += OnListBoxSelectionChanged;
            SyncListBoxFromSelection();
        }
        if (_searchBox is not null)
            _searchBox.AddHandler(TextBox.TextChangedEvent, OnSearchTextChanged);
        if (_popup is not null)
        {
            _popup.Closed += OnPopupClosed;
            _popup.IsOpen = IsDropDownOpen;
        }

        RebuildChips();
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    private void OnItemsSourceChanged()
    {
        if (_listBox is not null)
            _listBox.ItemsSource = ItemsSource;
    }

    private void OnTriggerPressed(object? sender, PointerPressedEventArgs e)
    {
        // clicks on chip remove buttons must not open the popup
        if (e.Source is Visual v && v.FindAncestorOfType<Button>(includeSelf: true) is { Classes: var c } && c.Contains("chip-remove"))
            return;
        SetCurrentValue(IsDropDownOpenProperty, !IsDropDownOpen);
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
            SetCurrentValue(SearchTextProperty, tb.Text);
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_syncing) return;
        _syncing = true;

        foreach (var item in e.RemovedItems)
        {
            if (item is not null)
                SelectedItems.Remove(item);
        }
        foreach (var item in e.AddedItems)
        {
            if (item is not null && !SelectedItems.Contains(item))
                SelectedItems.Add(item);
        }

        _syncing = false;
        RebuildChips();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_syncing) return;
        _syncing = true;
        SyncListBoxFromSelection();
        _syncing = false;
        RebuildChips();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SyncListBoxFromSelection()
    {
        if (_listBox is null) return;
        _listBox.SelectedItems?.Clear();
        foreach (var item in SelectedItems)
            _listBox.SelectedItems?.Add(item);
    }

    /// <summary>Chips are built from code-behind: text + remove button.</summary>
    private void RebuildChips()
    {
        if (_chipsPanel is null) return;

        _chipsPanel.Children.Clear();

        foreach (var item in SelectedItems)
        {
            var captured = item;

            var removeButton = new Button
            {
                Padding = new Thickness(2),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Height = 16,
                Width = 16,
                CornerRadius = new CornerRadius(9999),
                Cursor = new Cursor(StandardCursorType.Hand),
                Content = new Avalonia.Controls.Shapes.Path
                {
                    Data = Geometry.Parse("M 1 1 L 7 7 M 7 1 L 1 7"),
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1.5,
                    StrokeLineCap = PenLineCap.Round,
                    Width = 8,
                    Height = 8,
                    Stretch = Stretch.None,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
            removeButton.Classes.Add("chip-remove");
            removeButton.Click += (_, args) =>
            {
                SelectedItems.Remove(captured);
                args.Handled = true;
            };

            var chipContent = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                Children =
                {
                    new TextBlock
                    {
                        Text = FormatItemText(captured),
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                    },
                    removeButton,
                },
            };

            var chip = new Border { Child = chipContent };
            chip.Classes.Add("multiselect-chip");
            _chipsPanel.Children.Add(chip);
        }

        if (_placeholder is not null)
            _placeholder.IsVisible = SelectedItems.Count == 0;
    }

    // Filtering via container visibility: ItemsSource is not modified (AOT-safe)
    private void ApplyFilter()
    {
        if (_listBox is null) return;
        var text = SearchText;
        for (var i = 0; i < _listBox.ItemCount; i++)
        {
            if (_listBox.ContainerFromIndex(i) is not { } container)
                continue;
            container.IsVisible = string.IsNullOrEmpty(text)
                || FormatItemText(_listBox.Items[i] ?? container)?.Contains(text, StringComparison.OrdinalIgnoreCase) == true;
        }
    }

    private static string? FormatItemText(object item) =>
        item is ContentControl cc ? cc.Content?.ToString() : item.ToString();
}
