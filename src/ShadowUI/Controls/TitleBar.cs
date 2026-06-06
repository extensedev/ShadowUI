using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ShadowUI;

/// <summary>Custom window title bar in shadcn style (Tauri desktop title bar analogue).</summary>
public class TitleBar : TemplatedControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<TitleBar, string?>(nameof(Title));

    public static readonly StyledProperty<bool> ShowMaximizeProperty =
        AvaloniaProperty.Register<TitleBar, bool>(nameof(ShowMaximize), defaultValue: true);

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<TitleBar, object?>(nameof(Icon));

    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<TitleBar, double>(nameof(IconSize), defaultValue: 16d);
#pragma warning restore CS1591

    /// <summary>Window title displayed in the title bar.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Whether to show the maximize/restore button. Defaults to true.</summary>
    public bool ShowMaximize
    {
        get => GetValue(ShowMaximizeProperty);
        set => SetValue(ShowMaximizeProperty, value);
    }

    /// <summary>Icon to the left of the title. Accepts Image, Viewbox, Path, or any visual element.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Icon size in pixels. Defaults to 16.</summary>
    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TitleBar);

    private Button? _minBtn;
    private Button? _maxBtn;
    private Button? _restoreBtn;
    private Button? _closeBtn;
    private Border? _dragArea;
    private Window? _window;

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _window = TopLevel.GetTopLevel(this) as Window;
        if (_window is null) return;

        // Remove the native title bar (keep a resize border) and extend the client
        // area to the very top so our custom bar is the only thing there — no white
        // native caption strip above it.
        _window.WindowDecorations = WindowDecorations.BorderOnly;
        _window.ExtendClientAreaToDecorationsHint = true;
        _window.ExtendClientAreaTitleBarHeightHint = -1;

        _window.PropertyChanged += OnWindowPropertyChanged;
        PseudoClasses.Set(":maximized", _window.WindowState == WindowState.Maximized);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_window is not null)
        {
            _window.PropertyChanged -= OnWindowPropertyChanged;
            _window = null;
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _minBtn?.RemoveHandler(Button.ClickEvent, OnMinimize);
        _maxBtn?.RemoveHandler(Button.ClickEvent, OnToggleMaximize);
        _restoreBtn?.RemoveHandler(Button.ClickEvent, OnToggleMaximize);
        _closeBtn?.RemoveHandler(Button.ClickEvent, OnClose);
        if (_dragArea is not null)
            _dragArea.PointerPressed -= OnDragPointerPressed;

        _minBtn = e.NameScope.Find<Button>("PART_MinimizeButton");
        _maxBtn = e.NameScope.Find<Button>("PART_MaximizeButton");
        _restoreBtn = e.NameScope.Find<Button>("PART_RestoreButton");
        _closeBtn = e.NameScope.Find<Button>("PART_CloseButton");
        _dragArea = e.NameScope.Find<Border>("PART_DragArea");

        _minBtn?.AddHandler(Button.ClickEvent, OnMinimize);
        _maxBtn?.AddHandler(Button.ClickEvent, OnToggleMaximize);
        _restoreBtn?.AddHandler(Button.ClickEvent, OnToggleMaximize);
        _closeBtn?.AddHandler(Button.ClickEvent, OnClose);
        if (_dragArea is not null)
            _dragArea.PointerPressed += OnDragPointerPressed;
    }

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty)
            PseudoClasses.Set(":maximized", (WindowState)(e.NewValue ?? WindowState.Normal) == WindowState.Maximized);
    }

    private void OnDragPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            _window?.BeginMoveDrag(e);
    }

    private void OnMinimize(object? sender, RoutedEventArgs e)
    {
        if (_window is not null) _window.WindowState = WindowState.Minimized;
    }

    private void OnToggleMaximize(object? sender, RoutedEventArgs e)
    {
        if (_window is null) return;
        _window.WindowState = _window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnClose(object? sender, RoutedEventArgs e) => _window?.Close();
}
