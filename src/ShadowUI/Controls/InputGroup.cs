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
}
