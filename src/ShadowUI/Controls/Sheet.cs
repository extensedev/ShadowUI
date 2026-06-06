using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ShadowUI;

/// <summary>Side from which the Sheet slides in.</summary>
public enum SheetSide
{
    /// <summary>Slides in from the right.</summary>
    Right,
    /// <summary>Slides in from the bottom (Drawer).</summary>
    Bottom,
}

/// <summary>Sliding overlay panel (shadcn Sheet / Drawer analogue): scrim + animated panel from the side or bottom.</summary>
public class Sheet : ContentControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Sheet, bool>(nameof(IsOpen), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<SheetSide> SideProperty =
        AvaloniaProperty.Register<Sheet, SheetSide>(nameof(Side), defaultValue: SheetSide.Right);

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<Sheet, object?>(nameof(Title));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<Sheet, object?>(nameof(Description));

    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<Sheet, bool>(nameof(ShowCloseButton), defaultValue: true);

    public static readonly StyledProperty<bool> CloseOnClickOutsideProperty =
        AvaloniaProperty.Register<Sheet, bool>(nameof(CloseOnClickOutside), defaultValue: true);

    public static readonly StyledProperty<bool> CloseOnEscapeProperty =
        AvaloniaProperty.Register<Sheet, bool>(nameof(CloseOnEscape), defaultValue: true);
#pragma warning restore CS1591

    /// <summary>Whether the panel is open.</summary>
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }

    /// <summary>Side from which the panel slides in (Right or Bottom).</summary>
    public SheetSide Side { get => GetValue(SideProperty); set => SetValue(SideProperty, value); }

    /// <summary>Panel title.</summary>
    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <summary>Description below the title.</summary>
    public object? Description { get => GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    /// <summary>Whether to show the close (×) button in the corner.</summary>
    public bool ShowCloseButton { get => GetValue(ShowCloseButtonProperty); set => SetValue(ShowCloseButtonProperty, value); }

    /// <summary>Whether clicking on the scrim closes the panel.</summary>
    public bool CloseOnClickOutside { get => GetValue(CloseOnClickOutsideProperty); set => SetValue(CloseOnClickOutsideProperty, value); }

    /// <summary>Whether pressing Esc closes the panel.</summary>
    public bool CloseOnEscape { get => GetValue(CloseOnEscapeProperty); set => SetValue(CloseOnEscapeProperty, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Sheet);

    static Sheet()
    {
        IsOpenProperty.Changed.AddClassHandler<Sheet>((d, e) =>
            d.PseudoClasses.Set(":open", (bool)(e.NewValue ?? false)));

        SideProperty.Changed.AddClassHandler<Sheet>((d, e) =>
        {
            var side = (SheetSide)(e.NewValue ?? SheetSide.Right);
            d.PseudoClasses.Set(":side-right",  side == SheetSide.Right);
            d.PseudoClasses.Set(":side-bottom", side == SheetSide.Bottom);
        });
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PseudoClasses.Set(":open", IsOpen);
        PseudoClasses.Set(":side-right",  Side == SheetSide.Right);
        PseudoClasses.Set(":side-bottom", Side == SheetSide.Bottom);

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

    /// <summary>Open the panel.</summary>
    public void Open() => SetCurrentValue(IsOpenProperty, true);

    /// <summary>Close the panel.</summary>
    public void Close() => SetCurrentValue(IsOpenProperty, false);
}
