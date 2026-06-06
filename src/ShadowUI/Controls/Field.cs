using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ShadowUI;

/// <summary>Wrapper for any input control: Label on top, content, HintText/ErrorMessage below.</summary>
public class Field : ContentControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<object?> LabelProperty =
        AvaloniaProperty.Register<Field, object?>(nameof(Label));

    public static readonly StyledProperty<string?> HintTextProperty =
        AvaloniaProperty.Register<Field, string?>(nameof(HintText));

    public static readonly StyledProperty<string?> ErrorMessageProperty =
        AvaloniaProperty.Register<Field, string?>(nameof(ErrorMessage));

    public static readonly StyledProperty<bool> IsRequiredProperty =
        AvaloniaProperty.Register<Field, bool>(nameof(IsRequired), defaultValue: false);
#pragma warning restore CS1591

    /// <summary>Field label; may be a string or arbitrary content.</summary>
    public object? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Hint text below the input; hidden when an error is present.</summary>
    public string? HintText
    {
        get => GetValue(HintTextProperty);
        set => SetValue(HintTextProperty, value);
    }

    /// <summary>Error message; when non-empty, activates the :error pseudo-class.</summary>
    public string? ErrorMessage
    {
        get => GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    /// <summary>Whether the field is required; when true, shows an asterisk next to the label.</summary>
    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Field);

    static Field()
    {
        ErrorMessageProperty.Changed.AddClassHandler<Field>((s, _) =>
            s.PseudoClasses.Set(":error", !string.IsNullOrEmpty(s.ErrorMessage)));
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PseudoClasses.Set(":error", !string.IsNullOrEmpty(ErrorMessage));
    }
}
