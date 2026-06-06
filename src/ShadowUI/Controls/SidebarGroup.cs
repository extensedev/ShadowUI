using System;
using Avalonia;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Navigation item group with an optional header (shadcn SidebarGroup analogue).</summary>
public class SidebarGroup : ItemsControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<SidebarGroup, object?>(nameof(Header));
#pragma warning restore CS1591

    /// <summary>Group header (string or arbitrary content).</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SidebarGroup);

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is SidebarItem si && _collapsed)
            si.SetCollapsed(true);
    }

    private bool _collapsed;

    internal void SetCollapsed(bool collapsed)
    {
        _collapsed = collapsed;
        foreach (var item in Items)
        {
            if (item is null) continue;
            var container = ContainerFromItem(item) ?? (item as Control);
            if (container is SidebarItem si)
                si.SetCollapsed(collapsed);
            else if (container is SidebarMenuItem smi)
                smi.SetCollapsed(collapsed);
        }
    }
}
