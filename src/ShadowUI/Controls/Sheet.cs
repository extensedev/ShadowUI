using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Transformation;

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
    private const double DefaultRightSize = 400;
    private const double DefaultBottomSize = 360;

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

    public static readonly StyledProperty<bool> OverlayProperty =
        AvaloniaProperty.Register<Sheet, bool>(nameof(Overlay), defaultValue: true);

    public static readonly StyledProperty<bool> IsResizableProperty =
        AvaloniaProperty.Register<Sheet, bool>(nameof(IsResizable), defaultValue: true);

    public static readonly StyledProperty<double> PanelSizeProperty =
        AvaloniaProperty.Register<Sheet, double>(nameof(PanelSize), defaultValue: double.NaN);

    public static readonly StyledProperty<double> PanelMinSizeProperty =
        AvaloniaProperty.Register<Sheet, double>(nameof(PanelMinSize), defaultValue: 240d);

    public static readonly StyledProperty<double> PanelMaxSizeProperty =
        AvaloniaProperty.Register<Sheet, double>(nameof(PanelMaxSize), defaultValue: double.PositiveInfinity);

    public static readonly StyledProperty<CornerRadius> PanelCornerRadiusProperty =
        AvaloniaProperty.Register<Sheet, CornerRadius>(nameof(PanelCornerRadius));

    public static readonly StyledProperty<Thickness> PanelMarginProperty =
        AvaloniaProperty.Register<Sheet, Thickness>(nameof(PanelMargin));
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

    /// <summary>Whether the dimming scrim behind the panel is shown. Default true; false keeps the panel modal but transparent.</summary>
    public bool Overlay { get => GetValue(OverlayProperty); set => SetValue(OverlayProperty, value); }

    /// <summary>Whether the panel can be resized by dragging its inner edge (width for Right, height for Bottom).</summary>
    public bool IsResizable { get => GetValue(IsResizableProperty); set => SetValue(IsResizableProperty, value); }

    /// <summary>Panel size along its resize axis (width for Right, height for Bottom). Default <see cref="double.NaN"/> (400 right / 360 bottom).</summary>
    public double PanelSize { get => GetValue(PanelSizeProperty); set => SetValue(PanelSizeProperty, value); }

    /// <summary>Minimum panel size when resizing. Default 240.</summary>
    public double PanelMinSize { get => GetValue(PanelMinSizeProperty); set => SetValue(PanelMinSizeProperty, value); }

    /// <summary>Maximum panel size when resizing. Default unbounded (capped to the window minus the panel margin).</summary>
    public double PanelMaxSize { get => GetValue(PanelMaxSizeProperty); set => SetValue(PanelMaxSizeProperty, value); }

    /// <summary>Corner radius of the sliding panel. Defaults to the theme's large radius.</summary>
    public CornerRadius PanelCornerRadius { get => GetValue(PanelCornerRadiusProperty); set => SetValue(PanelCornerRadiusProperty, value); }

    /// <summary>Outer margin around the sliding panel so it does not stick to the window edges. Default none.</summary>
    public Thickness PanelMargin { get => GetValue(PanelMarginProperty); set => SetValue(PanelMarginProperty, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Sheet);

    private Border? _panel;
    private bool _resizing;
    private Point _dragStart;
    private double _dragStartSize;

    static Sheet()
    {
        IsOpenProperty.Changed.AddClassHandler<Sheet>((d, e) =>
        {
            d.PseudoClasses.Set(":open", (bool)(e.NewValue ?? false));
            d.ApplyPanelLayout();
        });

        SideProperty.Changed.AddClassHandler<Sheet>((d, e) =>
        {
            var side = (SheetSide)(e.NewValue ?? SheetSide.Right);
            d.PseudoClasses.Set(":side-right",  side == SheetSide.Right);
            d.PseudoClasses.Set(":side-bottom", side == SheetSide.Bottom);
            d.ApplyPanelLayout();
        });

        OverlayProperty.Changed.AddClassHandler<Sheet>((d, e) =>
            d.PseudoClasses.Set(":no-overlay", !(bool)(e.NewValue ?? true)));

        PanelSizeProperty.Changed.AddClassHandler<Sheet>((d, _) => d.ApplyPanelLayout());
        PanelMarginProperty.Changed.AddClassHandler<Sheet>((d, _) => d.ApplyPanelLayout());
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PseudoClasses.Set(":open", IsOpen);
        PseudoClasses.Set(":side-right",  Side == SheetSide.Right);
        PseudoClasses.Set(":side-bottom", Side == SheetSide.Bottom);
        PseudoClasses.Set(":no-overlay", !Overlay);

        var scrim = e.NameScope.Find<Border>("PART_Scrim");
        if (scrim is not null)
            scrim.AddHandler(PointerPressedEvent, OnScrimPressed, RoutingStrategies.Tunnel);

        var close = e.NameScope.Find<Button>("PART_CloseButton");
        if (close is not null)
            close.AddHandler(Button.ClickEvent, (_, _) => Close());

        _panel = e.NameScope.Find<Border>("PART_Panel");

        var grip = e.NameScope.Find<Border>("PART_ResizeGrip");
        if (grip is not null)
        {
            grip.AddHandler(PointerPressedEvent, OnGripPressed, RoutingStrategies.Bubble);
            grip.AddHandler(PointerMovedEvent, OnGripMoved, RoutingStrategies.Bubble);
            grip.AddHandler(PointerReleasedEvent, OnGripReleased, RoutingStrategies.Bubble);
        }

        ApplyPanelLayout();
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

    // Width (Right) / Height (Bottom) plus the slide transform are driven from code so the closed
    // offset always matches the current panel size — otherwise a resized panel would reopen from the
    // wrong position. The panel is docked (Right / Bottom), so growing it extends toward the center.
    private void ApplyPanelLayout()
    {
        if (_panel is null)
            return;

        var size = ResolveSize();
        if (Side == SheetSide.Bottom)
        {
            _panel.Height = size;
            _panel.Width = double.NaN;
        }
        else
        {
            _panel.Width = size;
            _panel.Height = double.NaN;
        }

        // The panel is always realized (the Sheet is not IsVisible=False), so it already sits at its
        // closed position off-screen; flipping to open simply lets the transition slide it in.
        SetPanelTransform(IsOpen, size);
    }

    private void SetPanelTransform(bool open, double size)
    {
        if (_panel is null)
            return;

        var margin = PanelMargin;
        // Closed offset clears the panel, its margin and a buffer for the box-shadow blur, so nothing
        // (not even the shadow) peeks in while the always-realized panel is hidden.
        const double shadowClearance = 24;
        var op = Side == SheetSide.Bottom
            ? (open ? "translateY(0px)" : $"translateY({(size + margin.Bottom + shadowClearance).ToString(CultureInfo.InvariantCulture)}px)")
            : (open ? "translateX(0px)" : $"translateX({(size + margin.Right + shadowClearance).ToString(CultureInfo.InvariantCulture)}px)");
        _panel.RenderTransform = TransformOperations.Parse(op);
    }

    private double ResolveSize()
    {
        var s = PanelSize;
        if (double.IsNaN(s))
            s = Side == SheetSide.Bottom ? DefaultBottomSize : DefaultRightSize;
        return ClampSize(s);
    }

    private double ClampSize(double v)
    {
        var min = PanelMinSize;
        if (!double.IsNaN(min) && min > 0)
            v = Math.Max(v, min);

        var max = PanelMaxSize;
        if (double.IsNaN(max) || double.IsInfinity(max))
        {
            var margin = PanelMargin;
            var avail = Side == SheetSide.Bottom
                ? Bounds.Height - margin.Top - margin.Bottom
                : Bounds.Width - margin.Left - margin.Right;
            max = avail > 0 ? avail - 16 : double.PositiveInfinity;
        }
        if (!double.IsInfinity(max))
            v = Math.Min(v, max);

        return v;
    }

    private void OnGripPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsResizable)
            return;
        _resizing = true;
        _dragStart = e.GetPosition(this);
        _dragStartSize = ResolveSize();
        e.Pointer.Capture(sender as IInputElement);
        e.Handled = true;
    }

    private void OnGripMoved(object? sender, PointerEventArgs e)
    {
        if (!_resizing)
            return;
        var p = e.GetPosition(this);
        // Dragging toward the window center (left for Right, up for Bottom) enlarges the panel.
        var delta = Side == SheetSide.Bottom ? _dragStart.Y - p.Y : _dragStart.X - p.X;
        SetCurrentValue(PanelSizeProperty, ClampSize(_dragStartSize + delta));
    }

    private void OnGripReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_resizing)
            return;
        _resizing = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    /// <summary>Open the panel.</summary>
    public void Open() => SetCurrentValue(IsOpenProperty, true);

    /// <summary>Close the panel.</summary>
    public void Close() => SetCurrentValue(IsOpenProperty, false);
}
