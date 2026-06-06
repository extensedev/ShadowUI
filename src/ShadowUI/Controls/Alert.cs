using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ShadowUI;

/// <summary>Informational banner (shadcn Alert analogue). Variants via Classes:
/// default / info / success / warning / destructive.</summary>
public class Alert : TemplatedControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<Alert, object?>(nameof(Icon));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(Title));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<Alert, object?>(nameof(Description));
#pragma warning restore CS1591

    /// <summary>Icon on the left.</summary>
    public object? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }

    /// <summary>Title.</summary>
    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <summary>Description text.</summary>
    public object? Description { get => GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Alert);
}
