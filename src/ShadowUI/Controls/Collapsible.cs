using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace ShadowUI;

/// <summary>Lightweight toggle container: shows/hides content via IsExpanded.</summary>
public class Collapsible : ContentControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<Collapsible, bool>(nameof(IsExpanded),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
#pragma warning restore CS1591

    /// <summary>Whether the container is expanded.</summary>
    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Collapsible);

    static Collapsible()
    {
        IsExpandedProperty.Changed.AddClassHandler<Collapsible>((s, _) =>
            s.PseudoClasses.Set(":expanded", s.IsExpanded));
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PseudoClasses.Set(":expanded", IsExpanded);
    }
}

/// <summary>Trigger for Collapsible: click toggles IsExpanded on the parent container.</summary>
public class CollapsibleTrigger : ContentControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CollapsibleTrigger);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        // handledEventsToo: true — Button inside trigger marks PointerPressed as Handled,
        // so without this flag left-click never reaches CollapsibleTrigger.
        AddHandler(PointerPressedEvent, OnTriggerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private void OnTriggerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
            return;

        var parent = this.GetVisualParent();
        while (parent is not null)
        {
            if (parent is Collapsible collapsible)
            {
                collapsible.IsExpanded = !collapsible.IsExpanded;
                e.Handled = true;
                return;
            }
            parent = parent.GetVisualParent();
        }
        // Graceful degradation: Collapsible not found — ignore
    }
}
