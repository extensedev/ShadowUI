using System;
using Avalonia;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Navigation item in a Sidebar (shadcn SidebarMenuButton analogue).</summary>
public class SidebarItem : Button
{
#pragma warning disable CS1591
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SidebarItem, object?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<SidebarItem, bool>(nameof(IsActive));
#pragma warning restore CS1591

    /// <summary>Icon on the left (Path, Image, TextBlock, etc.).</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Whether this is the active/current page.</summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SidebarItem);

    static SidebarItem()
    {
        IsActiveProperty.Changed.AddClassHandler<SidebarItem>((s, _) =>
            s.PseudoClasses.Set(":active", s.IsActive));
    }

    internal void SetCollapsed(bool collapsed) =>
        PseudoClasses.Set(":sidebar-collapsed", collapsed);
}
