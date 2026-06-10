using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace ShadowUI;

/// <summary>Navigation item group with an optional header (shadcn SidebarGroup analogue).</summary>
public class SidebarGroup : ItemsControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<SidebarGroup, object?>(nameof(Header));
#pragma warning restore CS1591

    static SidebarGroup()
    {
        // Gap between items: an ItemsPanel setter in a ControlTheme is not applied
        // to the ItemsPresenter, so the panel is set as the type default.
        // Without the gap, the selected item and a hovered neighbor visually merge.
        ItemsPanelProperty.OverrideDefaultValue<SidebarGroup>(
            new FuncTemplate<Panel?>(() => new StackPanel { Spacing = 8 }));
    }

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
