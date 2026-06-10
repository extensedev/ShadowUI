using System;
using Avalonia;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Input wrapper with prefix/suffix slots and correct edge corner rounding.</summary>
public class InputGroup : ContentControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<object?> PrefixProperty =
        AvaloniaProperty.Register<InputGroup, object?>(nameof(Prefix));

    public static readonly StyledProperty<object?> SuffixProperty =
        AvaloniaProperty.Register<InputGroup, object?>(nameof(Suffix));
#pragma warning restore CS1591

    /// <summary>Arbitrary content in the left slot (icon, text, etc.).</summary>
    public object? Prefix
    {
        get => GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    /// <summary>Arbitrary content in the right slot (icon, text, etc.).</summary>
    public object? Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(InputGroup);

    static InputGroup()
    {
        PrefixProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdateContentCorners());
        SuffixProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdateContentCorners());
        ContentProperty.Changed.AddClassHandler<InputGroup>((g, _) => g.UpdateContentCorners());
    }

    // The inner field must not keep its own rounded corners next to the slots:
    // edge rounding stays only on the outer sides of the group (same idea as ButtonGroup).
    private void UpdateContentCorners()
    {
        if (Content is not StyledElement c)
            return;
        c.Classes.Remove("input-group-first");
        c.Classes.Remove("input-group-middle");
        c.Classes.Remove("input-group-last");
        bool hasPrefix = Prefix is not null;
        bool hasSuffix = Suffix is not null;
        if (hasPrefix && hasSuffix) c.Classes.Add("input-group-middle");
        else if (hasPrefix) c.Classes.Add("input-group-last");
        else if (hasSuffix) c.Classes.Add("input-group-first");
    }
}
