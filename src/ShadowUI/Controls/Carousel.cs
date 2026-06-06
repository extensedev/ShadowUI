using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Controls.Shapes;

namespace ShadowUI;

/// <summary>
/// Carousel — sliding-slide component with Prev/Next buttons, dot navigation,
/// and arbitrary content support via ItemTemplate.
/// </summary>
public class Carousel : TemplatedControl
{
#pragma warning disable CS1591
    /// <summary>Current slide index property. Supports two-way binding.</summary>
    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<Carousel, int>(
            nameof(CurrentIndex),
            defaultValue: 0,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Slide list property (arbitrary IList).</summary>
    public static readonly StyledProperty<IList?> ItemsProperty =
        AvaloniaProperty.Register<Carousel, IList?>(nameof(Items));

    /// <summary>Data template property for rendering each slide.</summary>
    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<Carousel, IDataTemplate?>(nameof(ItemTemplate));
#pragma warning restore CS1591

    /// <summary>Current slide index (wraps cyclically).</summary>
    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    /// <summary>Slide collection (arbitrary content).</summary>
    public IList? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>Data template for rendering each slide.</summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Carousel);

    private ContentPresenter? _host;
    private StackPanel? _dots;
    private Button? _prev;
    private Button? _next;

    private EventHandler<RoutedEventArgs>? _prevClickHandler;
    private EventHandler<RoutedEventArgs>? _nextClickHandler;

    static Carousel()
    {
        CurrentIndexProperty.Changed.AddClassHandler<Carousel>((s, _) =>
        {
            s.UpdateDots();
            s.UpdateSlide();
        });

        ItemsProperty.Changed.AddClassHandler<Carousel>((s, _) =>
        {
            s.RebuildDots();
            s.UpdateSlide();
        });
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_prev is not null && _prevClickHandler is not null)
            _prev.RemoveHandler(Button.ClickEvent, _prevClickHandler);
        if (_next is not null && _nextClickHandler is not null)
            _next.RemoveHandler(Button.ClickEvent, _nextClickHandler);

        base.OnApplyTemplate(e);

        _host = e.NameScope.Find<ContentPresenter>("PART_ItemsHost");
        _dots = e.NameScope.Find<StackPanel>("PART_DotsPanel");
        _prev = e.NameScope.Find<Button>("PART_PrevButton");
        _next = e.NameScope.Find<Button>("PART_NextButton");

        _prevClickHandler = (_, _) => NavigatePrev();
        _nextClickHandler = (_, _) => NavigateNext();

        if (_prev is not null)
            _prev.AddHandler(Button.ClickEvent, _prevClickHandler);
        if (_next is not null)
            _next.AddHandler(Button.ClickEvent, _nextClickHandler);

        RebuildDots();
        UpdateSlide();
    }

    /// <summary>
    /// Rebuild dot indicators in PART_DotsPanel: one dot per slide.
    /// The current slide receives the <c>active</c> class.
    /// </summary>
    private void RebuildDots()
    {
        if (_dots is null) return;

        _dots.Children.Clear();

        var items = Items;
        if (items is null || items.Count == 0) return;

        for (int i = 0; i < items.Count; i++)
        {
            int capturedIndex = i;
            var dot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Margin = new Avalonia.Thickness(4),
            };

            if (i == CurrentIndex)
                dot.Classes.Add("active");

            dot.AddHandler(PointerPressedEvent, (_, _) =>
                SetCurrentValue(CurrentIndexProperty, capturedIndex),
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

            _dots.Children.Add(dot);
        }
    }

    /// <summary>
    /// Update dot indicator classes: the dot at <see cref="CurrentIndex"/> receives the <c>active</c> class.
    /// </summary>
    private void UpdateDots()
    {
        if (_dots is null) return;

        var children = _dots.Children;
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is Ellipse dot)
                dot.Classes.Set("active", i == CurrentIndex);
        }
    }

    /// <summary>
    /// Update the current slide display: show the item at <see cref="CurrentIndex"/> via ContentPresenter.
    /// </summary>
    private void UpdateSlide()
    {
        if (_host is null || Items is null || Items.Count == 0) return;

        // T-07-01-02: cyclic clamp — guard against out-of-range
        int count = Items.Count;
        int safeIndex = ((CurrentIndex % count) + count) % count;

        if (ItemTemplate != null)
            _host.ContentTemplate = ItemTemplate;

        _host.Content = Items[safeIndex];
    }

    /// <summary>Navigate to the previous slide (wraps cyclically).</summary>
    private void NavigatePrev()
    {
        var count = Items?.Count ?? 0;
        if (count == 0) return;
        SetCurrentValue(CurrentIndexProperty, (CurrentIndex - 1 + count) % count);
    }

    /// <summary>Navigate to the next slide (wraps cyclically).</summary>
    private void NavigateNext()
    {
        var count = Items?.Count ?? 0;
        if (count == 0) return;
        SetCurrentValue(CurrentIndexProperty, (CurrentIndex + 1) % count);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_prev is not null && _prevClickHandler is not null)
            _prev.RemoveHandler(Button.ClickEvent, _prevClickHandler);
        if (_next is not null && _nextClickHandler is not null)
            _next.RemoveHandler(Button.ClickEvent, _nextClickHandler);
        base.OnDetachedFromVisualTree(e);
    }
}
