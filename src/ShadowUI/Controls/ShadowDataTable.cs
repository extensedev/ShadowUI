using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace ShadowUI;

/// <summary>Table column definition: header text and sort capability.</summary>
public class DataTableColumn
{
    /// <summary>Column header text.</summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>Whether sorting is enabled for this column.</summary>
    public bool CanSort { get; set; } = true;
}

/// <summary>Column sort direction.</summary>
public enum SortDirection
{
    /// <summary>No sort applied.</summary>
    None,
    /// <summary>Ascending sort.</summary>
    Ascending,
    /// <summary>Descending sort.</summary>
    Descending,
}

/// <summary>
/// Interactive data table in shadcn DataTable style.
/// Supports column sorting, full-text filter across all cells,
/// and pagination.
/// </summary>
public class ShadowDataTable : TemplatedControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<IReadOnlyList<DataTableColumn>> ColumnsProperty =
        AvaloniaProperty.Register<ShadowDataTable, IReadOnlyList<DataTableColumn>>(
            nameof(Columns), defaultValue: Array.Empty<DataTableColumn>());

    public static readonly StyledProperty<IReadOnlyList<string[]>> RowsProperty =
        AvaloniaProperty.Register<ShadowDataTable, IReadOnlyList<string[]>>(
            nameof(Rows), defaultValue: Array.Empty<string[]>());

    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<ShadowDataTable, int>(nameof(PageSize), defaultValue: 0);

    public static readonly StyledProperty<int> CurrentPageProperty =
        AvaloniaProperty.Register<ShadowDataTable, int>(nameof(CurrentPage), defaultValue: 1);

    public static readonly StyledProperty<string?> FilterTextProperty =
        AvaloniaProperty.Register<ShadowDataTable, string?>(nameof(FilterText));
#pragma warning restore CS1591

    /// <summary>Column definitions.</summary>
    public IReadOnlyList<DataTableColumn> Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    /// <summary>Data rows. Each string[] is one row; indices correspond to columns.</summary>
    public IReadOnlyList<string[]> Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    /// <summary>Rows per page. 0 disables pagination and shows all rows.</summary>
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>Current page (1-based).</summary>
    public int CurrentPage
    {
        get => GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    /// <summary>Filter text. Filters rows across all cells (OrdinalIgnoreCase Contains).</summary>
    public string? FilterText
    {
        get => GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ShadowDataTable);

    private TextBox? _filterBox;
    private Grid? _headerGrid;
    private StackPanel? _rowsPanel;
    private Border? _paginationBar;
    private Button? _prevButton;
    private Button? _nextButton;
    private TextBlock? _pageInfo;

    private int _sortColumnIndex = -1;
    private SortDirection _sortDirection = SortDirection.None;

    private IReadOnlyList<string[]> _filteredRows = Array.Empty<string[]>();
    private IReadOnlyList<string[]> _displayRows = Array.Empty<string[]>();
    private int _totalPages = 1;

    static ShadowDataTable()
    {
        ColumnsProperty.Changed.AddClassHandler<ShadowDataTable>((s, _) => s.RefreshTable());
        RowsProperty.Changed.AddClassHandler<ShadowDataTable>((s, _) => s.ApplyFilter());
        FilterTextProperty.Changed.AddClassHandler<ShadowDataTable>((s, _) => s.ApplyFilter());
        PageSizeProperty.Changed.AddClassHandler<ShadowDataTable>((s, _) => s.ApplyFilter());
        CurrentPageProperty.Changed.AddClassHandler<ShadowDataTable>((s, _) => s.RebuildRowsForCurrentPage());
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_filterBox is not null)
            _filterBox.RemoveHandler(TextBox.TextChangedEvent, OnFilterTextChanged);
        if (_prevButton is not null)
            _prevButton.RemoveHandler(Button.ClickEvent, OnPrevPage);
        if (_nextButton is not null)
            _nextButton.RemoveHandler(Button.ClickEvent, OnNextPage);

        base.OnApplyTemplate(e);

        _filterBox     = e.NameScope.Find<TextBox>("PART_FilterBox");
        _headerGrid    = e.NameScope.Find<Grid>("PART_HeaderGrid");
        _rowsPanel     = e.NameScope.Find<StackPanel>("PART_RowsPanel");
        _paginationBar = e.NameScope.Find<Border>("PART_PaginationBar");
        _prevButton    = e.NameScope.Find<Button>("PART_PrevButton");
        _nextButton    = e.NameScope.Find<Button>("PART_NextButton");
        _pageInfo      = e.NameScope.Find<TextBlock>("PART_PageInfo");

        if (_filterBox is not null)
            _filterBox.AddHandler(TextBox.TextChangedEvent, OnFilterTextChanged);
        if (_prevButton is not null)
            _prevButton.AddHandler(Button.ClickEvent, OnPrevPage);
        if (_nextButton is not null)
            _nextButton.AddHandler(Button.ClickEvent, OnNextPage);

        RefreshTable();
    }

    private void OnFilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
            SetCurrentValue(FilterTextProperty, tb.Text);
    }

    private void OnPrevPage(object? sender, RoutedEventArgs e)
    {
        if (CurrentPage > 1)
            SetCurrentValue(CurrentPageProperty, CurrentPage - 1);
    }

    private void OnNextPage(object? sender, RoutedEventArgs e)
    {
        if (CurrentPage < _totalPages)
            SetCurrentValue(CurrentPageProperty, CurrentPage + 1);
    }

    private void OnSortColumn(int colIndex)
    {
        if (_sortColumnIndex != colIndex)
        {
            _sortColumnIndex = colIndex;
            _sortDirection = SortDirection.Ascending;
        }
        else
        {
            _sortDirection = _sortDirection switch
            {
                SortDirection.None       => SortDirection.Ascending,
                SortDirection.Ascending  => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _                        => SortDirection.None,
            };
            if (_sortDirection == SortDirection.None)
                _sortColumnIndex = -1;
        }
        BuildHeaders();
        ApplyFilter();
    }

    /// <summary>Rebuild the header and apply filter/sort/pagination.</summary>
    private void RefreshTable()
    {
        BuildHeaders();
        ApplyFilter();
    }

    /// <summary>Build header buttons/textblocks in PART_HeaderGrid.</summary>
    private void BuildHeaders()
    {
        if (_headerGrid is null) return;

        _headerGrid.Children.Clear();
        _headerGrid.ColumnDefinitions.Clear();

        var cols = Columns ?? Array.Empty<DataTableColumn>();

        for (int i = 0; i < cols.Count; i++)
        {
            _headerGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            var col = cols[i];
            bool isSorted = _sortColumnIndex == i;
            string suffix = isSorted && _sortDirection == SortDirection.Ascending  ? " ↑"
                          : isSorted && _sortDirection == SortDirection.Descending ? " ↓"
                          : string.Empty;

            if (col.CanSort)
            {
                int capturedIndex = i;
                var btn = new Button
                {
                    Content = col.Header + suffix,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding = new Avalonia.Thickness(14, 10),
                    FontSize = 13,
                };
                btn.Classes.Add("ghost");
                btn.Classes.Add("th");
                btn.Click += (_, _) => OnSortColumn(capturedIndex);
                Grid.SetColumn(btn, i);
                _headerGrid.Children.Add(btn);
            }
            else
            {
                var tb = new TextBlock { Text = col.Header };
                tb.Classes.Add("th");
                Grid.SetColumn(tb, i);
                _headerGrid.Children.Add(tb);
            }
        }
    }

    /// <summary>Apply filter, sort, and recalculate pagination.</summary>
    private void ApplyFilter()
    {
        var rows = Rows ?? Array.Empty<string[]>();
        var filterText = FilterText;

        if (string.IsNullOrEmpty(filterText))
        {
            _filteredRows = rows;
        }
        else
        {
            _filteredRows = rows
                .Where(row => row.Any(cell =>
                    cell != null && cell.Contains(filterText, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        if (_sortColumnIndex >= 0)
        {
            int idx = _sortColumnIndex;
            if (_sortDirection == SortDirection.Ascending)
            {
                _filteredRows = _filteredRows
                    .OrderBy(r => idx < r.Length ? r[idx] : null, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            else if (_sortDirection == SortDirection.Descending)
            {
                _filteredRows = _filteredRows
                    .OrderByDescending(r => idx < r.Length ? r[idx] : null, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        int pageSize = PageSize;
        if (pageSize <= 0)
        {
            _totalPages = 1;
            _displayRows = _filteredRows;
            if (_paginationBar is not null)
                _paginationBar.IsVisible = false;
        }
        else
        {
            _totalPages = (_filteredRows.Count + pageSize - 1) / pageSize;
            if (_totalPages == 0) _totalPages = 1;

            int cp = Math.Clamp(CurrentPage, 1, _totalPages);
            if (cp != CurrentPage)
                SetCurrentValue(CurrentPageProperty, cp);

            _displayRows = _filteredRows.Skip((cp - 1) * pageSize).Take(pageSize).ToList();

            if (_paginationBar is not null)
                _paginationBar.IsVisible = true;
        }

        UpdatePaginationUi(pageSize);

        BuildRows();
    }

    /// <summary>Rebuild the current page slice without re-filtering or re-sorting.</summary>
    private void RebuildRowsForCurrentPage()
    {
        int pageSize = PageSize;
        if (pageSize <= 0) return;

        int cp = Math.Clamp(CurrentPage, 1, _totalPages);
        _displayRows = _filteredRows.Skip((cp - 1) * pageSize).Take(pageSize).ToList();

        UpdatePaginationUi(pageSize);
        BuildRows();
    }

    /// <summary>Update the PageInfo text and Prev/Next button states.</summary>
    private void UpdatePaginationUi(int pageSize)
    {
        if (_pageInfo is not null)
            _pageInfo.Text = pageSize > 0 ? $"Page {CurrentPage} of {_totalPages}" : string.Empty;

        if (_prevButton is not null)
            _prevButton.IsEnabled = CurrentPage > 1;
        if (_nextButton is not null)
            _nextButton.IsEnabled = CurrentPage < _totalPages;
    }

    /// <summary>Build data rows in PART_RowsPanel from _displayRows.</summary>
    private void BuildRows()
    {
        if (_rowsPanel is null) return;

        _rowsPanel.Children.Clear();

        var cols = Columns ?? Array.Empty<DataTableColumn>();
        int colCount = cols.Count;

        if (_displayRows.Count == 0)
        {
            var emptyBorder = new Border { Padding = new Avalonia.Thickness(14, 20) };
            var emptyText = new TextBlock
            {
                Text = "No data",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            emptyText.Classes.Add("muted");
            emptyBorder.Child = emptyText;
            _rowsPanel.Children.Add(emptyBorder);
            return;
        }

        for (int i = 0; i < _displayRows.Count; i++)
        {
            var row = _displayRows[i];
            bool isLast = i == _displayRows.Count - 1;

            var rowGrid = new Grid();
            for (int j = 0; j < colCount; j++)
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            for (int j = 0; j < colCount; j++)
            {
                var cell = new TextBlock { Text = j < row.Length ? row[j] : string.Empty };
                cell.Classes.Add("td");
                Grid.SetColumn(cell, j);
                rowGrid.Children.Add(cell);
            }

            var rowBorder = new Border();
            rowBorder.Classes.Add("tr");
            if (isLast)
                rowBorder.Classes.Add("last");
            rowBorder.Child = rowGrid;
            _rowsPanel.Children.Add(rowBorder);
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_filterBox is not null)
            _filterBox.RemoveHandler(TextBox.TextChangedEvent, OnFilterTextChanged);
        if (_prevButton is not null)
            _prevButton.RemoveHandler(Button.ClickEvent, OnPrevPage);
        if (_nextButton is not null)
            _nextButton.RemoveHandler(Button.ClickEvent, OnNextPage);
        base.OnDetachedFromVisualTree(e);
    }
}
