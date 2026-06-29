using System;
using System.Linq;
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

    public static readonly StyledProperty<object?> TriggerProperty =
        AvaloniaProperty.Register<Collapsible, object?>(nameof(Trigger));
#pragma warning restore CS1591

    /// <summary>Whether the container is expanded.</summary>
    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }

    /// <summary>The always-visible header that toggles expansion — typically a <see cref="CollapsibleTrigger"/>.</summary>
    public object? Trigger { get => GetValue(TriggerProperty); set => SetValue(TriggerProperty, value); }

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

/// <summary>
/// Behavioral wrapper for a Collapsible header (the shadcn <c>asChild</c> pattern): it renders no
/// surface of its own — clicking its content toggles the target Collapsible, and the trigger mirrors
/// the expanded state as the <c>:expanded</c> pseudo-class so child content (e.g. a chevron) can react.
/// </summary>
public class CollapsibleTrigger : ContentControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CollapsibleTrigger);

    private Collapsible? _target;
    private bool _handlerAttached;

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (!_handlerAttached)
        {
            // handledEventsToo: true — content inside the trigger (e.g. a Button) may mark
            // PointerPressed as Handled, so without this flag left-click never reaches the trigger.
            AddHandler(PointerPressedEvent, OnTriggerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
            _handlerAttached = true;
        }
        BindTarget();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachTarget();
    }

    private void BindTarget()
    {
        DetachTarget();
        _target = FindCollapsible();
        if (_target is not null)
            _target.PropertyChanged += OnTargetPropertyChanged;
        // Mirror the target's expanded state as a pseudo-class so the icon can react via styles.
        PseudoClasses.Set(":expanded", _target?.IsExpanded ?? false);
    }

    private void DetachTarget()
    {
        if (_target is not null)
            _target.PropertyChanged -= OnTargetPropertyChanged;
        _target = null;
    }

    private void OnTargetPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Collapsible.IsExpandedProperty)
            PseudoClasses.Set(":expanded", e.GetNewValue<bool>());
    }

    private void OnTriggerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
            return;

        if ((_target ??= FindCollapsible()) is { } collapsible)
        {
            collapsible.IsExpanded = !collapsible.IsExpanded;
            e.Handled = true;
        }
        // Graceful degradation: Collapsible not found — ignore
    }

    /// <summary>
    /// Resolves the Collapsible this trigger controls: first an ancestor (trigger nested inside
    /// the Collapsible), then a sibling sharing the same visual parent (trigger and Collapsible
    /// placed side by side in a container — the common shadcn layout).
    /// </summary>
    internal Collapsible? FindCollapsible()
    {
        for (var ancestor = this.GetVisualParent(); ancestor is not null; ancestor = ancestor.GetVisualParent())
            if (ancestor is Collapsible collapsible)
                return collapsible;

        return this.GetVisualParent()?
            .GetVisualChildren()
            .OfType<Collapsible>()
            .FirstOrDefault();
    }
}
