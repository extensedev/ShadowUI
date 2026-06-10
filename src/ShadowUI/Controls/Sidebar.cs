using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace ShadowUI;

/// <summary>Side navigation panel (shadcn Sidebar analogue).</summary>
public class Sidebar : ItemsControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.Register<Sidebar, bool>(nameof(IsCollapsed));

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<Sidebar, object?>(nameof(Header));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Sidebar, object?>(nameof(Footer));

    public static readonly StyledProperty<bool> ShowToggleButtonProperty =
        AvaloniaProperty.Register<Sidebar, bool>(nameof(ShowToggleButton), defaultValue: true);

    public static readonly StyledProperty<double> ExpandedWidthProperty =
        AvaloniaProperty.Register<Sidebar, double>(nameof(ExpandedWidth), defaultValue: 240);

    public static readonly StyledProperty<double> CollapsedWidthProperty =
        AvaloniaProperty.Register<Sidebar, double>(nameof(CollapsedWidth), defaultValue: 52);
#pragma warning restore CS1591

    /// <summary>Whether the sidebar is collapsed to icon-only mode (52 px wide).</summary>
    public bool IsCollapsed
    {
        get => GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    /// <summary>Sidebar header content (logo, title).</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Sidebar footer content (profile, version).</summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>Whether to show the collapse toggle button. false keeps the sidebar always expanded.</summary>
    public bool ShowToggleButton
    {
        get => GetValue(ShowToggleButtonProperty);
        set => SetValue(ShowToggleButtonProperty, value);
    }

    /// <summary>Width of the expanded sidebar (defaults to 240).</summary>
    public double ExpandedWidth
    {
        get => GetValue(ExpandedWidthProperty);
        set => SetValue(ExpandedWidthProperty, value);
    }

    /// <summary>Width of the collapsed (icon-only) sidebar (defaults to 52).</summary>
    public double CollapsedWidth
    {
        get => GetValue(CollapsedWidthProperty);
        set => SetValue(CollapsedWidthProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Sidebar);

    static Sidebar()
    {
        // Gap between groups (an ItemsPanel setter in a ControlTheme is not applied)
        ItemsPanelProperty.OverrideDefaultValue<Sidebar>(
            new Avalonia.Controls.Templates.FuncTemplate<Panel?>(() => new StackPanel { Spacing = 8 }));

        IsCollapsedProperty.Changed.AddClassHandler<Sidebar>((s, _) => s.ApplyCollapsedState());
        ExpandedWidthProperty.Changed.AddClassHandler<Sidebar>((s, _) => s.UpdateWidth());
        CollapsedWidthProperty.Changed.AddClassHandler<Sidebar>((s, _) => s.UpdateWidth());
    }

    /// <summary>Creates the sidebar and sets the initial width (before animation is applied).</summary>
    public Sidebar()
    {
        Width = IsCollapsed ? CollapsedWidth : ExpandedWidth;
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var btn = e.NameScope.Find<Button>("PART_ToggleButton");
        if (btn is not null)
            btn.AddHandler(Button.ClickEvent, OnToggle);
        ApplyCollapsedState();
    }

    private void UpdateWidth() => Width = IsCollapsed ? CollapsedWidth : ExpandedWidth;

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        PropagateCollapsed(container, IsCollapsed);
    }

    private void ApplyCollapsedState()
    {
        PseudoClasses.Set(":collapsed", IsCollapsed);
        UpdateWidth();
        foreach (var item in Items)
        {
            if (item is null) continue;
            var container = ContainerFromItem(item) ?? (item as Control);
            if (container is not null)
                PropagateCollapsed(container, IsCollapsed);
        }
    }

    private static void PropagateCollapsed(Control container, bool collapsed)
    {
        if (container is SidebarItem si)
            si.SetCollapsed(collapsed);
        else if (container is SidebarGroup sg)
            sg.SetCollapsed(collapsed);
    }

    private void OnToggle(object? sender, RoutedEventArgs e) =>
        IsCollapsed = !IsCollapsed;
}
