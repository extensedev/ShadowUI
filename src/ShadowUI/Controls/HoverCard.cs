using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;

namespace ShadowUI;

/// <summary>Preview card (shadcn HoverCard analogue). Appears on hover over the trigger after a delay.</summary>
public class HoverCard : ContentControl
{
#pragma warning disable CS1591
    /// <summary>IsOpen registered property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<HoverCard, bool>(nameof(IsOpen),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>TriggerContent registered property.</summary>
    public static readonly StyledProperty<object?> TriggerContentProperty =
        AvaloniaProperty.Register<HoverCard, object?>(nameof(TriggerContent));

    /// <summary>OpenDelay registered property (ms).</summary>
    public static readonly StyledProperty<int> OpenDelayProperty =
        AvaloniaProperty.Register<HoverCard, int>(nameof(OpenDelay), defaultValue: 300);

    /// <summary>CloseDelay registered property (ms).</summary>
    public static readonly StyledProperty<int> CloseDelayProperty =
        AvaloniaProperty.Register<HoverCard, int>(nameof(CloseDelay), defaultValue: 200);
#pragma warning restore CS1591

    /// <summary>Whether the card is open.</summary>
    public bool IsOpen { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }

    /// <summary>Trigger content (the element the cursor hovers over).</summary>
    public object? TriggerContent { get => GetValue(TriggerContentProperty); set => SetValue(TriggerContentProperty, value); }

    /// <summary>Open delay in milliseconds (defaults to 300).</summary>
    public int OpenDelay { get => GetValue(OpenDelayProperty); set => SetValue(OpenDelayProperty, value); }

    /// <summary>Close delay in milliseconds (defaults to 200).</summary>
    public int CloseDelay { get => GetValue(CloseDelayProperty); set => SetValue(CloseDelayProperty, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(HoverCard);

    private DispatcherTimer? _openTimer;
    private DispatcherTimer? _closeTimer;
    private Popup? _popupPart;

    static HoverCard()
    {
        IsOpenProperty.Changed.AddClassHandler<HoverCard>((hc, e) =>
        {
            hc.PseudoClasses.Set(":open", (bool)(e.NewValue ?? false));
            hc._popupPart?.SetCurrentValue(Popup.IsOpenProperty, (bool)(e.NewValue ?? false));
        });
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _popupPart = e.NameScope.Find<Popup>("PART_Popup");

        var trigger = e.NameScope.Find<Border>("PART_Trigger");
        if (trigger is not null)
        {
            trigger.AddHandler(PointerEnteredEvent, OnTriggerEnter, Avalonia.Interactivity.RoutingStrategies.Bubble);
            trigger.AddHandler(PointerExitedEvent, OnTriggerLeave, Avalonia.Interactivity.RoutingStrategies.Bubble);
        }

        PseudoClasses.Set(":open", IsOpen);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        // T-01C-02: stop timers on detach to prevent leaks
        _openTimer?.Stop();
        _openTimer = null;
        _closeTimer?.Stop();
        _closeTimer = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnTriggerEnter(object? sender, PointerEventArgs e)
    {
        _closeTimer?.Stop();
        _closeTimer = null;

        if (Content is null) return;

        _openTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(OpenDelay) };
        _openTimer.Tick += (_, _) =>
        {
            IsOpen = true;
            _openTimer?.Stop();
        };
        _openTimer.Start();
    }

    private void OnTriggerLeave(object? sender, PointerEventArgs e)
    {
        _openTimer?.Stop();
        _openTimer = null;

        _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(CloseDelay) };
        _closeTimer.Tick += (_, _) =>
        {
            IsOpen = false;
            _closeTimer?.Stop();
        };
        _closeTimer.Start();
    }

    /// <summary>
    /// Opens the card only when Content is not null.
    /// Called from the open timer; internal for testing.
    /// </summary>
    internal void OpenIfHasContent()
    {
        if (Content is not null)
            IsOpen = true;
    }
}
