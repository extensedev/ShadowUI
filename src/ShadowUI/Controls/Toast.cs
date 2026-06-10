using System;
using System.Collections.Generic;
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
/// therefore sizes itself to the overlay and lets the inner stack handle alignment.)
/// <para>Cards stack sonner-style: the newest in front, older ones peek out behind it,
/// offset and scaled down; hovering expands the stack into a list.</para></summary>
public sealed class ToastHost : Panel
{
    private const int MaxVisible = 3;
    private const double PeekOffset = 12;   // how far each back card peeks out
    private const double ScaleStep = 0.05;  // scale-down of each back card
    private const double Spacing = 8;       // gap between cards in the expanded state

    /// <summary>Position of this stack.</summary>
    public ToastPosition Position { get; }

    private readonly Panel _stack = new() { Width = 380, ClipToBounds = false };
    private readonly List<Border> _wrappers = new(); // [0] = newest
    private readonly bool _top;
    private readonly Control _layer;
    private bool _expanded;

    /// <summary>Creates a host for the given position, bound to the overlay layer.</summary>
    public ToastHost(Control overlayLayer, ToastPosition position)
    {
        _layer = overlayLayer;
        Position = position;
        IsHitTestVisible = true;
        ClipToBounds = false;

        _stack.Margin = new Thickness(16);
        _stack.HorizontalAlignment = position switch
        {
            ToastPosition.TopLeft or ToastPosition.BottomLeft => HorizontalAlignment.Left,
            ToastPosition.TopCenter or ToastPosition.BottomCenter => HorizontalAlignment.Center,
            _ => HorizontalAlignment.Right,
        };
        _top = position is ToastPosition.TopLeft or ToastPosition.TopRight or ToastPosition.TopCenter;
        _stack.VerticalAlignment = _top ? VerticalAlignment.Top : VerticalAlignment.Bottom;

        _stack.PointerEntered += (_, _) => { _expanded = true; LayoutCards(); };
        _stack.PointerExited += (_, _) => { _expanded = false; LayoutCards(); };

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

        // Wrapper: the stack transform lives on it, so it does not conflict with the
        // enter/exit animation of the card itself (which also uses RenderTransform)
        var wrapper = new Border
        {
            Child = card,
            VerticalAlignment = _top ? VerticalAlignment.Top : VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RenderTransformOrigin = new RelativePoint(0.5, _top ? 0 : 1, RelativeUnit.Relative),
            Transitions = new Avalonia.Animation.Transitions
            {
                new Avalonia.Animation.TransformOperationsTransition
                {
                    Property = RenderTransformProperty,
                    Duration = TimeSpan.FromMilliseconds(200),
                    Easing = new Avalonia.Animation.Easings.CubicEaseOut(),
                },
                new Avalonia.Animation.DoubleTransition
                {
                    Property = OpacityProperty,
                    Duration = TimeSpan.FromMilliseconds(150),
                },
            },
        };

        _wrappers.Insert(0, wrapper);
        _stack.Children.Add(wrapper);

        LayoutCards();
        // slide/fade in + recompute the expanded offsets after the first layout
        Dispatcher.UIThread.Post(() =>
        {
            card.SetShown(true);
            LayoutCards();
        }, DispatcherPriority.Loaded);

        DispatcherTimer.RunOnce(() => Remove(card), duration);
    }

    private void LayoutCards()
    {
        double offset = 0;
        for (int i = 0; i < _wrappers.Count; i++)
        {
            var w = _wrappers[i];
            w.ZIndex = 1000 - i;

            if (_expanded)
            {
                double y = _top ? offset : -offset;
                w.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Parse(
                    FormattableString.Invariant($"translateY({y}px)"));
                w.Opacity = 1;
                w.IsHitTestVisible = true;

                double h = w.Bounds.Height > 0 ? w.Bounds.Height : w.DesiredSize.Height;
                offset += h + Spacing;
            }
            else
            {
                double y = (_top ? 1 : -1) * i * PeekOffset;
                double scale = Math.Max(1 - ScaleStep * i, 0.7);
                w.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Parse(
                    FormattableString.Invariant($"translateY({y}px) scale({scale})"));

                bool visible = i < MaxVisible;
                w.Opacity = visible ? 1 : 0;
                w.IsHitTestVisible = visible;
            }
        }
    }

    private void Remove(ToastCard card)
    {
        Border? wrapper = null;
        foreach (var w in _wrappers)
        {
            if (ReferenceEquals(w.Child, card)) { wrapper = w; break; }
        }
        if (wrapper is null) return;

        card.SetShown(false);                       // animate out
        DispatcherTimer.RunOnce(() =>
        {
            if (_wrappers.Remove(wrapper))
            {
                _stack.Children.Remove(wrapper);
                LayoutCards();
            }
        }, TimeSpan.FromMilliseconds(180));
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
