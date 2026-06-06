using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace ShadowUI;

/// <summary>Expandable nav item with optional sub-items (shadcn SidebarMenuItem+Sub).</summary>
public class SidebarMenuItem : ItemsControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SidebarMenuItem, object?>(nameof(Icon));

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<SidebarMenuItem, object?>(nameof(Header));

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<SidebarMenuItem, bool>(nameof(IsExpanded), defaultValue: true);

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<SidebarMenuItem, bool>(nameof(IsActive));
#pragma warning restore CS1591

    /// <summary>Menu item icon.</summary>
    public object? Icon   { get => GetValue(IconProperty);     set => SetValue(IconProperty, value); }
    /// <summary>Menu item header.</summary>
    public object? Header { get => GetValue(HeaderProperty);   set => SetValue(HeaderProperty, value); }
    /// <summary>Whether the menu item is expanded.</summary>
    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }
    /// <summary>Whether the menu item is active.</summary>
    public bool IsActive   { get => GetValue(IsActiveProperty);   set => SetValue(IsActiveProperty, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SidebarMenuItem);

    static SidebarMenuItem()
    {
        IsExpandedProperty.Changed.AddClassHandler<SidebarMenuItem>((s, _) =>
            s.PseudoClasses.Set(":expanded", s.IsExpanded));
        IsActiveProperty.Changed.AddClassHandler<SidebarMenuItem>((s, _) =>
            s.PseudoClasses.Set(":active", s.IsActive));
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PseudoClasses.Set(":expanded", IsExpanded);
        PseudoClasses.Set(":active", IsActive);
        var toggle = e.NameScope.Find<Button>("PART_Toggle");
        if (toggle is not null)
            toggle.AddHandler(Button.ClickEvent, OnToggle);
    }

    internal void SetCollapsed(bool sidebarCollapsed)
        => PseudoClasses.Set(":sidebar-collapsed", sidebarCollapsed);

    private void OnToggle(object? sender, RoutedEventArgs e)
    {
        if (Items.Count > 0)
            IsExpanded = !IsExpanded;
    }
}
