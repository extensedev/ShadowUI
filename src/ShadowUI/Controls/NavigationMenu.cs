using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace ShadowUI;

/// <summary>Horizontal navigation menu (shadcn NavigationMenu analogue).</summary>
public class NavigationMenu : ItemsControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(NavigationMenu);
}

/// <summary>Navigation menu item with an optional hover flyout (shadcn NavigationMenuItem analogue).</summary>
public class NavigationMenuItem : HeaderedContentControl
{
#pragma warning disable CS1591
    /// <summary>Flyout open state registered property.</summary>
    public static readonly StyledProperty<bool> IsFlyoutOpenProperty =
        AvaloniaProperty.Register<NavigationMenuItem, bool>(
            nameof(IsFlyoutOpen),
            defaultValue: false,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
#pragma warning restore CS1591

    /// <summary>Whether the flyout submenu of this item is open.</summary>
    public bool IsFlyoutOpen
    {
        get => GetValue(IsFlyoutOpenProperty);
        set => SetValue(IsFlyoutOpenProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(NavigationMenuItem);

    private DispatcherTimer? _closeTimer;
    private Border? _triggerPart;
    private Popup? _popupPart;

    static NavigationMenuItem()
    {
        IsFlyoutOpenProperty.Changed.AddClassHandler<NavigationMenuItem>((item, e) =>
        {
            var value = (bool)(e.NewValue ?? false);
            item.PseudoClasses.Set(":flyout-open", value);
            if (item._popupPart is not null)
                item._popupPart.IsOpen = value;
        });
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_triggerPart is not null)
        {
            _triggerPart.RemoveHandler(InputElement.PointerEnteredEvent, OnTriggerPointerEnter);
            _triggerPart.RemoveHandler(InputElement.PointerExitedEvent, OnTriggerPointerLeave);
        }

        _triggerPart = e.NameScope.Find<Border>("PART_Trigger");
        _popupPart = e.NameScope.Find<Popup>("PART_Popup");

        if (_triggerPart is not null)
        {
            _triggerPart.AddHandler(InputElement.PointerEnteredEvent, OnTriggerPointerEnter, RoutingStrategies.Bubble);
            _triggerPart.AddHandler(InputElement.PointerExitedEvent, OnTriggerPointerLeave, RoutingStrategies.Bubble);
        }

        if (_popupPart is not null)
            _popupPart.IsOpen = IsFlyoutOpen;
    }

    private void OnTriggerPointerEnter(object? sender, PointerEventArgs e)
    {
        _closeTimer?.Stop();
        _closeTimer = null;

        // Leaf items (no Content) do not open a flyout
        if (Content is not null)
            IsFlyoutOpen = true;
    }

    private void OnTriggerPointerLeave(object? sender, PointerEventArgs e)
    {
        // T-01A-02: stop previous timer before creating a new one to prevent leak
        _closeTimer?.Stop();
        _closeTimer = null;

        _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _closeTimer.Tick += (_, _) =>
        {
            IsFlyoutOpen = false;
            _closeTimer?.Stop();
            _closeTimer = null;
        };
        _closeTimer.Start();
    }

    /// <summary>Open the flyout submenu. Does nothing if Content is null (leaf item).</summary>
    public void OpenFlyout() => IsFlyoutOpen = Content is not null;

    /// <summary>Close the flyout submenu.</summary>
    public void CloseFlyout() => IsFlyoutOpen = false;
}
