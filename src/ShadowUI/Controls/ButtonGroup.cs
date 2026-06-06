using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace ShadowUI;

/// <summary>Button group with a shared border and correct corner rounding on edge buttons.</summary>
public class ButtonGroup : ItemsControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ButtonGroup, Orientation>(nameof(Orientation), defaultValue: Orientation.Horizontal);
#pragma warning restore CS1591

    /// <summary>Group orientation: Horizontal (row) or Vertical (column).</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ButtonGroup);

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        var count = Items.Count;

        container.Classes.Remove("button-group-first");
        container.Classes.Remove("button-group-middle");
        container.Classes.Remove("button-group-last");

        if (count == 1)
        {
            container.Classes.Add("button-group-first");
            container.Classes.Add("button-group-last");
        }
        else if (index == 0)
        {
            container.Classes.Add("button-group-first");
        }
        else if (index == count - 1)
        {
            container.Classes.Add("button-group-last");
        }
        else
        {
            container.Classes.Add("button-group-middle");
        }
    }
}
