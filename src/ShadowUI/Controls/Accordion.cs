using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ShadowUI;

/// <summary>Container for collapsible sections (shadcn Accordion analogue).</summary>
public class Accordion : ItemsControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Accordion);
}

/// <summary>Single accordion section (header + expandable content).</summary>
public class AccordionItem : HeaderedContentControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<AccordionItem, bool>(nameof(IsExpanded));
#pragma warning restore CS1591

    /// <summary>Whether the section is expanded.</summary>
    public bool IsExpanded { get => GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(AccordionItem);

    static AccordionItem()
    {
        IsExpandedProperty.Changed.AddClassHandler<AccordionItem>((s, _) =>
            s.PseudoClasses.Set(":expanded", s.IsExpanded));
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        PseudoClasses.Set(":expanded", IsExpanded);
        var trigger = e.NameScope.Find<Border>("PART_Trigger");
        if (trigger is not null)
            trigger.AddHandler(PointerPressedEvent, (_, _) => IsExpanded = !IsExpanded);
    }
}
