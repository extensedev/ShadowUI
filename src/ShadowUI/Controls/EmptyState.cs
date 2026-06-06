using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ShadowUI;

/// <summary>Empty-state component: icon, title, description, and optional action button.</summary>
public class EmptyState : TemplatedControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<EmptyState, object?>(nameof(Icon));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<EmptyState, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<EmptyState, string?>(nameof(Description));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<EmptyState, object?>(nameof(ActionContent));

    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<EmptyState, ICommand?>(nameof(ActionCommand));
#pragma warning restore CS1591

    /// <summary>Empty-state icon (arbitrary content, e.g. Viewbox/Path).</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Empty-state title.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Empty-state description.</summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Action button content (hidden when null).</summary>
    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    /// <summary>Command invoked when the action button is clicked.</summary>
    public ICommand? ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(EmptyState);
}
