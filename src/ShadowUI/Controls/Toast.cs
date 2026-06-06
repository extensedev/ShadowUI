using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;

namespace ShadowUI;

/// <summary>Notification type (determines accent color/icon).</summary>
public enum ToastType
{
#pragma warning disable CS1591
    Default,
    Success,
    Warning,
    Info,
    Error,
#pragma warning restore CS1591
}

/// <summary>Single toast notification card (shadcn sonner/toast analogue).</summary>
public class ToastCard : TemplatedControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ToastCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<ToastCard, string?>(nameof(Message));

    public static readonly StyledProperty<ToastType> ToastTypeProperty =
        AvaloniaProperty.Register<ToastCard, ToastType>(nameof(ToastType));
#pragma warning restore CS1591

    /// <summary>Title.</summary>
    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    /// <summary>Message text.</summary>
    public string? Message { get => GetValue(MessageProperty); set => SetValue(MessageProperty, value); }

    /// <summary>Toast type (accent).</summary>
    public ToastType ToastType { get => GetValue(ToastTypeProperty); set => SetValue(ToastTypeProperty, value); }

    /// <summary>Raised when the toast is dismissed (by button or timer).</summary>
    public event EventHandler? Dismissed;

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ToastCard);

    static ToastCard()
    {
        ToastTypeProperty.Changed.AddClassHandler<ToastCard>((c, e) => c.UpdateTypeClass((ToastType)(e.NewValue ?? ToastType.Default)));
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateTypeClass(ToastType);
        var close = e.NameScope.Find<Button>("PART_CloseButton");
        if (close is not null)
            close.AddHandler(Button.ClickEvent, (_, _) => Dismissed?.Invoke(this, EventArgs.Empty));
    }

    private void UpdateTypeClass(ToastType type)
    {
        PseudoClasses.Set(":success", type == ToastType.Success);
        PseudoClasses.Set(":warning", type == ToastType.Warning);
        PseudoClasses.Set(":info", type == ToastType.Info);
        PseudoClasses.Set(":error", type == ToastType.Error);
    }

    internal void SetShown(bool shown) => PseudoClasses.Set(":shown", shown);
}

/// <summary>Corner of the window where toasts appear.</summary>
public enum ToastPosition
{
#pragma warning disable CS1591
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    TopCenter,
    BottomCenter,
#pragma warning restore CS1591
}

/// <summary>Host for toast cards: fills the OverlayLayer and anchors the stack to the desired corner.
/// (OverlayLayer behaves like a Canvas and does not stretch children — the host
/// therefore sizes itself to the overlay and lets the inner stack handle alignment.)</summary>
public sealed class ToastHost : Panel
{
    /// <summary>Position of this stack.</summary>
    public ToastPosition Position { get; }

    private readonly StackPanel _stack = new() { Orientation = Orientation.Vertical, Spacing = 8, Width = 380 };
    private readonly bool _newestOnTop;
    private readonly Control _layer;

    /// <summary>Creates a host for the given position, bound to the overlay layer.</summary>
    public ToastHost(Control overlayLayer, ToastPosition position)
    {
        _layer = overlayLayer;
        Position = position;
        IsHitTestVisible = true;

        _stack.Margin = new Thickness(16);
        _stack.HorizontalAlignment = position switch
        {
            ToastPosition.TopLeft or ToastPosition.BottomLeft => HorizontalAlignment.Left,
            ToastPosition.TopCenter or ToastPosition.BottomCenter => HorizontalAlignment.Center,
            _ => HorizontalAlignment.Right,
        };
        var top = position is ToastPosition.TopLeft or ToastPosition.TopRight or ToastPosition.TopCenter;
        _stack.VerticalAlignment = top ? VerticalAlignment.Top : VerticalAlignment.Bottom;
        _newestOnTop = top;

        Children.Add(_stack);

        // OverlayLayer = Canvas → manually size the host to match the layer
        SyncSize();
        _layer.PropertyChanged += OnLayerPropertyChanged;
    }

    private void OnLayerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty)
            SyncSize();
    }

    private void SyncSize()
    {
        Width = _layer.Bounds.Width;
        Height = _layer.Bounds.Height;
    }

    internal void Push(string? title, string? message, ToastType type, TimeSpan duration)
    {
        var card = new ToastCard { Title = title, Message = message, ToastType = type };
        card.Dismissed += (_, _) => Remove(card);

        if (_newestOnTop)
            _stack.Children.Insert(0, card);
        else
            _stack.Children.Add(card);

        // slide/fade in on the next frame
        Dispatcher.UIThread.Post(() => card.SetShown(true), DispatcherPriority.Render);

        DispatcherTimer.RunOnce(() => Remove(card), duration);
    }

    private void Remove(ToastCard card)
    {
        if (!_stack.Children.Contains(card)) return;
        card.SetShown(false);                       // animate out
        DispatcherTimer.RunOnce(() => _stack.Children.Remove(card), TimeSpan.FromMilliseconds(180));
    }
}

/// <summary>Entry point for showing toast notifications from anywhere in the UI.</summary>
public static class Toast
{
    /// <summary>Default position for all toasts (when not specified in the call).</summary>
    public static ToastPosition DefaultPosition { get; set; } = ToastPosition.BottomRight;

    /// <summary>Show a notification. <paramref name="anchor"/> is any control in the current window.</summary>
    public static void Show(
        Visual anchor,
        string title,
        string? message = null,
        ToastType type = ToastType.Default,
        double seconds = 4,
        ToastPosition? position = null)
    {
        var layer = OverlayLayer.GetOverlayLayer(anchor);
        if (layer is null) return;

        var pos = position ?? DefaultPosition;

        ToastHost? host = null;
        foreach (var child in layer.Children)
        {
            if (child is ToastHost h && h.Position == pos) { host = h; break; }
        }

        if (host is null)
        {
            host = new ToastHost(layer, pos);
            layer.Children.Add(host);
        }

        host.Push(title, message, type, TimeSpan.FromSeconds(seconds));
    }
}
