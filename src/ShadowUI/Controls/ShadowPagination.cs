using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace ShadowUI;

/// <summary>Pagination component in shadcn style: Prev / page buttons with «…» / Next.</summary>
public class ShadowPagination : TemplatedControl
{
#pragma warning disable CS1591
    /// <summary>TotalPages registered property.</summary>
    public static readonly StyledProperty<int> TotalPagesProperty =
        AvaloniaProperty.Register<ShadowPagination, int>(nameof(TotalPages), defaultValue: 1);

    /// <summary>CurrentPage registered property.</summary>
    public static readonly StyledProperty<int> CurrentPageProperty =
        AvaloniaProperty.Register<ShadowPagination, int>(nameof(CurrentPage), defaultValue: 1,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>SiblingCount registered property.</summary>
    public static readonly StyledProperty<int> SiblingCountProperty =
        AvaloniaProperty.Register<ShadowPagination, int>(nameof(SiblingCount), defaultValue: 1);
#pragma warning restore CS1591

    /// <summary>Total number of pages.</summary>
    public int TotalPages
    {
        get => GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    /// <summary>Current page (TwoWay).</summary>
    public int CurrentPage
    {
        get => GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    /// <summary>Number of pages shown on each side of the current page.</summary>
    public int SiblingCount
    {
        get => GetValue(SiblingCountProperty);
        set => SetValue(SiblingCountProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ShadowPagination);

    /// <summary>Raised when the page changes. Argument is the selected page number.</summary>
    public event EventHandler<int>? PageChanged;

    private Button? _prevButton;
    private Button? _nextButton;
    private StackPanel? _pagesPanel;

    static ShadowPagination()
    {
        TotalPagesProperty.Changed.AddClassHandler<ShadowPagination>((s, _) => s.BuildPages());
        CurrentPageProperty.Changed.AddClassHandler<ShadowPagination>((s, _) => s.BuildPages());
        SiblingCountProperty.Changed.AddClassHandler<ShadowPagination>((s, _) => s.BuildPages());
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_prevButton is not null)
            _prevButton.RemoveHandler(Button.ClickEvent, OnPrevClick);
        if (_nextButton is not null)
            _nextButton.RemoveHandler(Button.ClickEvent, OnNextClick);

        base.OnApplyTemplate(e);

        _prevButton  = e.NameScope.Find<Button>("PART_PrevButton");
        _nextButton  = e.NameScope.Find<Button>("PART_NextButton");
        _pagesPanel  = e.NameScope.Find<StackPanel>("PART_PagesPanel");

        if (_prevButton is not null)
            _prevButton.AddHandler(Button.ClickEvent, OnPrevClick);
        if (_nextButton is not null)
            _nextButton.AddHandler(Button.ClickEvent, OnNextClick);

        BuildPages();
    }

    /// <summary>Rebuilds the page button list with «…» ellipsis for skipped ranges.</summary>
    private void BuildPages()
    {
        if (_pagesPanel is null) return;

        _pagesPanel.Children.Clear();

        var total   = Math.Max(1, TotalPages);
        var current = Math.Clamp(CurrentPage, 1, total);
        var sibling = Math.Max(0, SiblingCount);

        var visibleSet = new SortedSet<int>();
        visibleSet.Add(1);
        visibleSet.Add(total);
        for (int p = Math.Max(1, current - sibling); p <= Math.Min(total, current + sibling); p++)
            visibleSet.Add(p);

        int prev = 0;
        foreach (var pageNum in visibleSet)
        {
            if (prev > 0 && pageNum - prev > 1)
            {
                var ellipsis = new TextBlock
                {
                    Text              = "…",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin            = new Thickness(4, 0),
                };
                ellipsis.Classes.Add("muted");
                _pagesPanel.Children.Add(ellipsis);
            }

            var btn = new Button { Content = pageNum.ToString() };
            if (pageNum == current)
            {
                btn.Classes.Add("secondary");
            }
            else
            {
                btn.Classes.Add("outline");
                btn.Classes.Add("sm");
            }
            btn.Tag = pageNum;

            int capturedPage = pageNum;
            btn.Click += (_, _) => OnPageClick(capturedPage);

            _pagesPanel.Children.Add(btn);
            prev = pageNum;
        }

        if (_prevButton is not null)
            _prevButton.IsEnabled = current > 1;
        if (_nextButton is not null)
            _nextButton.IsEnabled = current < total;
    }

    private void OnPageClick(int page)
    {
        SetCurrentValue(CurrentPageProperty, page);
        PageChanged?.Invoke(this, page);
        BuildPages();
    }

    private void OnPrevClick(object? sender, RoutedEventArgs e)
    {
        var newPage = Math.Max(1, CurrentPage - 1);
        SetCurrentValue(CurrentPageProperty, newPage);
        PageChanged?.Invoke(this, newPage);
        BuildPages();
    }

    private void OnNextClick(object? sender, RoutedEventArgs e)
    {
        var newPage = Math.Min(TotalPages, CurrentPage + 1);
        SetCurrentValue(CurrentPageProperty, newPage);
        PageChanged?.Invoke(this, newPage);
        BuildPages();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_prevButton is not null)
            _prevButton.RemoveHandler(Button.ClickEvent, OnPrevClick);
        if (_nextButton is not null)
            _nextButton.RemoveHandler(Button.ClickEvent, OnNextClick);
        base.OnDetachedFromVisualTree(e);
    }
}
