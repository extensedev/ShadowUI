using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ShadowUI;

/// <summary>Modal dialog (shadcn Dialog analogue): scrim overlay + centred card.</summary>
public class Dialog : ContentControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsOpen), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<Dialog, object?>(nameof(Title));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<Dialog, object?>(nameof(Description));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Dialog, object?>(nameof(Footer));

    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(ShowCloseButton), defaultValue: true);

    public static readonly StyledProperty<bool> CloseOnClickOutsideProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(CloseOnClickOutside), defaultValue: true);

    public static readonly StyledProperty<bool> CloseOnEscapeProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(CloseOnEscape), defaultValue: true);
#pragma warning restore CS1591

    /// <summary>Whether the dialog is open.</summary>
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }

    /// <summary>Dialog title.</summary>
    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <summary>Description below the title.</summary>
    public object? Description { get => GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    /// <summary>Footer content (action buttons).</summary>
    public object? Footer { get => GetValue(FooterProperty); set => SetValue(FooterProperty, value); }

    /// <summary>Whether to show the close (×) button in the corner.</summary>
    public bool ShowCloseButton { get => GetValue(ShowCloseButtonProperty); set => SetValue(ShowCloseButtonProperty, value); }

    /// <summary>Whether clicking outside the card closes the dialog (false for AlertDialog).</summary>
    public bool CloseOnClickOutside { get => GetValue(CloseOnClickOutsideProperty); set => SetValue(CloseOnClickOutsideProperty, value); }

    /// <summary>Whether pressing Esc closes the dialog (false for AlertDialog).</summary>
    public bool CloseOnEscape { get => GetValue(CloseOnEscapeProperty); set => SetValue(CloseOnEscapeProperty, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Dialog);

    static Dialog()
    {
        IsOpenProperty.Changed.AddClassHandler<Dialog>((d, e) =>
            d.PseudoClasses.Set(":open", (bool)(e.NewValue ?? false)));
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PseudoClasses.Set(":open", IsOpen);

        var scrim = e.NameScope.Find<Border>("PART_Scrim");
        if (scrim is not null)
            scrim.AddHandler(PointerPressedEvent, OnScrimPressed, RoutingStrategies.Tunnel);

        var close = e.NameScope.Find<Button>("PART_CloseButton");
        if (close is not null)
            close.AddHandler(Button.ClickEvent, (_, _) => Close());
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen && CloseOnEscape)
        {
            Close();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (CloseOnClickOutside && ReferenceEquals(e.Source, sender))
            Close();
    }

    /// <summary>Open the dialog.</summary>
    public void Open() => SetCurrentValue(IsOpenProperty, true);

    /// <summary>Close the dialog.</summary>
    public void Close() => SetCurrentValue(IsOpenProperty, false);
}
