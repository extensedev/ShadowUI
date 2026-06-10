using System;
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

    public static readonly StyledProperty<string> SearchPlaceholderProperty =
        AvaloniaProperty.Register<SearchableComboBox, string>(nameof(SearchPlaceholder), "Search...");
#pragma warning restore CS1591

    /// <summary>Filter text; changing it hides non-matching list items.</summary>
    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>Placeholder for the search field in the dropdown.</summary>
    public string SearchPlaceholder
    {
        get => GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SearchableComboBox);

    static SearchableComboBox()
    {
        SearchTextProperty.Changed.AddClassHandler<SearchableComboBox>((s, _) => s.ApplyFilter());
        IsDropDownOpenProperty.Changed.AddClassHandler<SearchableComboBox>((s, e) =>
        {
            if (!(bool)(e.NewValue ?? false))
                s.SetCurrentValue(SearchTextProperty, null);
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

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
            SetCurrentValue(SearchTextProperty, tb.Text);
    }

    // Filtering via container visibility: Items/ItemsSource are not modified,
    // so it works with both inline items and ItemsSource (AOT-safe).
    private void ApplyFilter()
    {
        var text = SearchText;
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is not { } container)
                continue;
            container.IsVisible = string.IsNullOrEmpty(text)
                || FormatItemText(Items[i] ?? container)?.Contains(text, StringComparison.OrdinalIgnoreCase) == true;
        }
    }

    private static string? FormatItemText(object item) =>
        item is ContentControl cc ? cc.Content?.ToString() : item.ToString();
}
