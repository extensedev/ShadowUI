using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace ShadowUI;

/// <summary>Single-selection toggle group (shadcn ToggleGroup type=single analogue).
/// Place <see cref="ToggleGroupItem"/> elements with the same GroupName inside.</summary>
public class ToggleGroup : StackPanel
{
    /// <summary>Creates a horizontal segmented group.</summary>
    public ToggleGroup()
    {
        Orientation = Orientation.Horizontal;
        Spacing = 0;
        VerticalAlignment = VerticalAlignment.Center;
    }
}

/// <summary>Toggle group segment (radio-like, single selection via GroupName).</summary>
public class ToggleGroupItem : RadioButton
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ToggleGroupItem);
}
