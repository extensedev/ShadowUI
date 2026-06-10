using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ShadowUI;

/// <summary>
/// Popover: a trigger with a floating content card (analogous to shadcn Popover).
/// The trigger is set via <see cref="TriggerContent"/>, the content via Content.
/// </summary>
public class Popover : ContentControl
{
#pragma warning disable CS1591
    /// <summary>TriggerContent registered property.</summary>
    public static readonly StyledProperty<object?> TriggerContentProperty =
        AvaloniaProperty.Register<Popover, object?>(nameof(TriggerContent));

    /// <summary>IsOpen registered property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Popover, bool>(nameof(IsOpen),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Placement registered property.</summary>
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.Register<Popover, PlacementMode>(nameof(Placement),
            defaultValue: PlacementMode.BottomEdgeAlignedLeft);
#pragma warning restore CS1591

    /// <summary>Trigger content (usually a button); clicking it opens/closes the popover.</summary>
    public object? TriggerContent
    {
        get => GetValue(TriggerContentProperty);
        set => SetValue(TriggerContentProperty, value);
    }

    /// <summary>Whether the popover is open. Supports two-way binding.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Placement of the floating card relative to the trigger.</summary>
    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Popover);

    private Popup? _popup;
    private ContentPresenter? _trigger;

    static Popover()
    {
        IsOpenProperty.Changed.AddClassHandler<Popover>((s, e) => s.OnIsOpenChanged(e));
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_trigger is not null)
            _trigger.RemoveHandler(PointerPressedEvent, OnTriggerPressed);
        if (_popup is not null)
            _popup.Closed -= OnPopupClosed;

        base.OnApplyTemplate(e);

        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _trigger = e.NameScope.Find<ContentPresenter>("PART_TriggerPresenter");

        if (_trigger is not null)
            _trigger.AddHandler(PointerPressedEvent, OnTriggerPressed, handledEventsToo: true);
        if (_popup is not null)
        {
            _popup.Closed += OnPopupClosed;
            _popup.IsOpen = IsOpen;
        }
    }

    private void OnTriggerPressed(object? sender, PointerPressedEventArgs e)
    {
        SetCurrentValue(IsOpenProperty, !IsOpen);
    }

    private void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_popup is not null)
            _popup.IsOpen = e.GetNewValue<bool>();
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        // light dismiss: sync the property when closed externally
        SetCurrentValue(IsOpenProperty, false);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_trigger is not null)
            _trigger.RemoveHandler(PointerPressedEvent, OnTriggerPressed);
        if (_popup is not null)
            _popup.Closed -= OnPopupClosed;
        base.OnDetachedFromVisualTree(e);
    }
}
