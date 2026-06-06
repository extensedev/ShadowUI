using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ShadowUI;

/// <summary>Generic list item: icon, primary text, secondary text, and trailing action.</summary>
public class ShadowItem : TemplatedControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<ShadowItem, object?>(nameof(Icon));

    public static readonly StyledProperty<string?> PrimaryTextProperty =
        AvaloniaProperty.Register<ShadowItem, string?>(nameof(PrimaryText));

    public static readonly StyledProperty<string?> SecondaryTextProperty =
        AvaloniaProperty.Register<ShadowItem, string?>(nameof(SecondaryText));

    public static readonly StyledProperty<object?> TrailingActionProperty =
        AvaloniaProperty.Register<ShadowItem, object?>(nameof(TrailingAction));
#pragma warning restore CS1591

    /// <summary>Item icon (arbitrary content).</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Primary item text.</summary>
    public string? PrimaryText
    {
        get => GetValue(PrimaryTextProperty);
        set => SetValue(PrimaryTextProperty, value);
    }

    /// <summary>Secondary text (subtitle/description) of the item.</summary>
    public string? SecondaryText
    {
        get => GetValue(SecondaryTextProperty);
        set => SetValue(SecondaryTextProperty, value);
    }

    /// <summary>Trailing action to the right of the item (arbitrary content).</summary>
    public object? TrailingAction
    {
        get => GetValue(TrailingActionProperty);
        set => SetValue(TrailingActionProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ShadowItem);
}
