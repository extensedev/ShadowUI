using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ShadowUI;

/// <summary>ComboBox with a built-in text filter: the list narrows as you type.</summary>
public class SearchableComboBox : ComboBox
{
#pragma warning disable CS1591
    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<SearchableComboBox, string?>(nameof(SearchText));
#pragma warning restore CS1591

    /// <summary>Filter text; changes trigger an ItemsSource recalculation.</summary>
    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SearchableComboBox);

    private List<object?> _allItems = new();
    private bool _updatingItems = false;

    static SearchableComboBox()
    {
        SearchTextProperty.Changed.AddClassHandler<SearchableComboBox>((s, _) => s.ApplyFilter());
        IsDropDownOpenProperty.Changed.AddClassHandler<SearchableComboBox>((s, e) =>
        {
            if (!(bool)(e.NewValue ?? false))
                s.ClearSearch();
        });
        ItemsSourceProperty.Changed.AddClassHandler<SearchableComboBox>((s, e) =>
        {
            if (s._updatingItems) return;
            s._allItems = (e.NewValue as IEnumerable)?.Cast<object?>().ToList() ?? new List<object?>();
        });
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var searchBox = e.NameScope.Find<TextBox>("PART_SearchBox");
        if (searchBox is not null)
            searchBox.AddHandler(TextBox.TextChangedEvent, OnSearchTextChanged);
    }

    private void OnSearchTextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
            SetCurrentValue(SearchTextProperty, tb.Text);
    }

    private void ApplyFilter()
    {
        if (_updatingItems) return;
        var text = SearchText;
        if (string.IsNullOrEmpty(text))
        {
            SetItemsFiltered(_allItems);
            return;
        }
        var filtered = _allItems
            .Where(item => item?.ToString()?.Contains(text, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();
        SetItemsFiltered(filtered);
    }

    private void SetItemsFiltered(IEnumerable<object?> items)
    {
        _updatingItems = true;
        SetCurrentValue(ItemsControl.ItemsSourceProperty, items.ToList());
        _updatingItems = false;
    }

    private void ClearSearch()
    {
        SetCurrentValue(SearchTextProperty, null);
        SetItemsFiltered(_allItems);
    }
}
