using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace ShadowUI;

/// <summary>Attached properties for <see cref="TabControl"/>.</summary>
public static class Tabs
{
    /// <summary>When true, the tab bar uses the underline style (underlines the active tab)
    /// instead of a segmented container. Example: <c>shadui:Tabs.Underline="True"</c>.</summary>
    public static readonly AttachedProperty<bool> UnderlineProperty =
        AvaloniaProperty.RegisterAttached<TabControl, bool>("Underline", typeof(Tabs));

    /// <summary>When true, the content area always reserves the height of the tallest tab page,
    /// so switching tabs does not change the TabControl height (no layout jumping).
    /// Applies to tabs whose <see cref="TabItem"/>.Content is a control (the typical XAML case).
    /// Example: <c>shadui:Tabs.UniformContentHeight="True"</c>.</summary>
    public static readonly AttachedProperty<bool> UniformContentHeightProperty =
        AvaloniaProperty.RegisterAttached<TabControl, bool>("UniformContentHeight", typeof(Tabs));

#pragma warning disable CS1591
    public static bool GetUnderline(TabControl control) => control.GetValue(UnderlineProperty);
    public static void SetUnderline(TabControl control, bool value) => control.SetValue(UnderlineProperty, value);

    public static bool GetUniformContentHeight(TabControl control) => control.GetValue(UniformContentHeightProperty);
    public static void SetUniformContentHeight(TabControl control, bool value) => control.SetValue(UniformContentHeightProperty, value);
#pragma warning restore CS1591

    static Tabs()
    {
        UniformContentHeightProperty.Changed.AddClassHandler<TabControl>(OnUniformContentHeightChanged);
    }

    private static void OnUniformContentHeightChanged(TabControl tabControl, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.GetNewValue<bool>())
        {
            tabControl.TemplateApplied += OnTemplateApplied;
            tabControl.SelectionChanged += OnSelectionChanged;
            if (FindContentHost(tabControl) is { } presenter)
                Hook(tabControl, presenter);
        }
        else
        {
            tabControl.TemplateApplied -= OnTemplateApplied;
            tabControl.SelectionChanged -= OnSelectionChanged;
            if (FindContentHost(tabControl) is { } presenter)
            {
                presenter.SizeChanged -= OnContentHostSizeChanged;
                presenter.MinHeight = 0;
            }
        }
    }

    private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        var tabControl = (TabControl)sender!;
        if (e.NameScope.Find<ContentPresenter>("PART_SelectedContentHost") is { } presenter)
            Hook(tabControl, presenter);
    }

    private static void Hook(TabControl tabControl, ContentPresenter presenter)
    {
        presenter.SizeChanged -= OnContentHostSizeChanged;
        presenter.SizeChanged += OnContentHostSizeChanged;
        UpdateMinHeight(tabControl, presenter);
    }

    private static void OnContentHostSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // height changes are a consequence of MinHeight itself — only width affects wrapping
        if (!e.WidthChanged)
            return;
        var presenter = (ContentPresenter)sender!;
        if (presenter.TemplatedParent is TabControl tabControl && GetUniformContentHeight(tabControl))
            UpdateMinHeight(tabControl, presenter);
    }

    private static void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var tabControl = (TabControl)sender!;
        if (GetUniformContentHeight(tabControl) && FindContentHost(tabControl) is { } presenter)
            UpdateMinHeight(tabControl, presenter);
    }

    /// <summary>Measures every non-selected tab page out-of-tree (contents are logical children of
    /// their TabItem, so themes/resources resolve) and reserves the tallest height on the host.</summary>
    private static void UpdateMinHeight(TabControl tabControl, ContentPresenter presenter)
    {
        var width = presenter.Bounds.Width;
        if (width <= 0 || double.IsNaN(width))
            return;

        double max = 0;
        foreach (var item in tabControl.Items)
        {
            if ((item as TabItem)?.Content is not Control content)
                continue;
            // the live page is measured by the presenter itself; anything still parented is off-limits
            if (ReferenceEquals(content, presenter.Child) || content.GetVisualParent() is not null)
                continue;
            content.Measure(new Size(width, double.PositiveInfinity));
            max = Math.Max(max, content.DesiredSize.Height);
        }

        presenter.MinHeight = max + presenter.Padding.Top + presenter.Padding.Bottom;
    }

    private static ContentPresenter? FindContentHost(TabControl tabControl)
    {
        foreach (var visual in tabControl.GetVisualDescendants())
        {
            if (visual is ContentPresenter { Name: "PART_SelectedContentHost" } presenter
                && presenter.TemplatedParent == tabControl)
                return presenter;
        }
        return null;
    }
}
