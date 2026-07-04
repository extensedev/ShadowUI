using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

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

    public static readonly StyledProperty<double> CardWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(CardWidth), defaultValue: double.NaN);

    public static readonly StyledProperty<double> CardMinWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(CardMinWidth), defaultValue: 320d);

    public static readonly StyledProperty<double> CardMaxWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(CardMaxWidth), defaultValue: 480d);

    public static readonly StyledProperty<double> CardHeightProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(CardHeight), defaultValue: double.NaN);

    public static readonly StyledProperty<double> CardMaxHeightProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(CardMaxHeight), defaultValue: double.PositiveInfinity);

    public static readonly StyledProperty<bool> OverlayProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(Overlay), defaultValue: true);
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

    /// <summary>Explicit card width. Default <see cref="double.NaN"/> (auto, sized by content within the min/max bounds).</summary>
    public double CardWidth { get => GetValue(CardWidthProperty); set => SetValue(CardWidthProperty, value); }

    /// <summary>Minimum card width. Default 320.</summary>
    public double CardMinWidth { get => GetValue(CardMinWidthProperty); set => SetValue(CardMinWidthProperty, value); }

    /// <summary>Maximum card width. Default 480.</summary>
    public double CardMaxWidth { get => GetValue(CardMaxWidthProperty); set => SetValue(CardMaxWidthProperty, value); }

    /// <summary>Explicit card height. Default <see cref="double.NaN"/> (auto, sized by content).</summary>
    public double CardHeight { get => GetValue(CardHeightProperty); set => SetValue(CardHeightProperty, value); }

    /// <summary>Maximum card height. Default unbounded; set this to cap tall content.</summary>
    public double CardMaxHeight { get => GetValue(CardMaxHeightProperty); set => SetValue(CardMaxHeightProperty, value); }

    /// <summary>Whether the dimming scrim behind the card is shown. Default true; false keeps the dialog modal but transparent.</summary>
    public bool Overlay { get => GetValue(OverlayProperty); set => SetValue(OverlayProperty, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Dialog);

    private static readonly IBrush ScrimBrush = new SolidColorBrush(Color.Parse("#99000000"));

    static Dialog()
    {
        IsOpenProperty.Changed.AddClassHandler<Dialog>((d, e) =>
        {
            d.PseudoClasses.Set(":open", (bool)(e.NewValue ?? false));
            d.ApplyOpenState();
        });

        OverlayProperty.Changed.AddClassHandler<Dialog>((d, _) => d.ApplyOverlay());
    }

    // Scrim is hosted in the window's OverlayLayer (portal) rather than inline,
    // so the overlay spans the whole window no matter where the Dialog is declared.
    private Panel? _root;
    private Border? _scrim;
    private OverlayLayer? _overlay;

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PseudoClasses.Set(":open", IsOpen);

        DetachScrim();

        _root = e.NameScope.Find<Panel>("PART_Root");
        _scrim = e.NameScope.Find<Border>("PART_Scrim");

        if (_scrim is not null)
        {
            _root?.Children.Remove(_scrim);
            _scrim.AddHandler(PointerPressedEvent, OnScrimPressed, RoutingStrategies.Tunnel);
            _scrim.AddHandler(KeyDownEvent, OnScrimKeyDown);
        }

        var close = e.NameScope.Find<Button>("PART_CloseButton");
        if (close is not null)
            close.AddHandler(Button.ClickEvent, (_, _) => Close());

        AttachScrimToOverlay();
        ApplyOverlay();
        ApplyOpenState();
    }

    private void ApplyOverlay()
    {
        if (_scrim is not null)
            _scrim.Background = Overlay ? ScrimBrush : Brushes.Transparent;
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachScrimToOverlay();
        ApplyOpenState();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachScrim();
    }

    private void AttachScrimToOverlay()
    {
        if (_scrim is null)
            return;

        var layer = OverlayLayer.GetOverlayLayer(this);
        if (layer is null)
            return;

        if (!ReferenceEquals(_overlay, layer))
        {
            _overlay?.Children.Remove(_scrim);
            _overlay = layer;
        }

        if (!_overlay.Children.Contains(_scrim))
            _overlay.Children.Add(_scrim);
    }

    private void DetachScrim()
    {
        if (_scrim is not null)
            _overlay?.Children.Remove(_scrim);
        _overlay = null;
    }

    private void ApplyOpenState()
    {
        if (_scrim is null)
            return;

        var open = IsOpen;
        _scrim.Opacity = open ? 1 : 0;
        _scrim.IsHitTestVisible = open;

        if (open)
            Dispatcher.UIThread.Post(() => _scrim?.Focus(), DispatcherPriority.Input);
    }

    private void OnScrimKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen && CloseOnEscape)
        {
            Close();
            e.Handled = true;
        }
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
