using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;

namespace ShadowUI;

/// <summary>Resizable split container: two panels separated by a draggable GridSplitter.</summary>
public class Resizable : TemplatedControl
{
#pragma warning disable CS1591
    /// <summary>FirstContent registered property.</summary>
    public static readonly StyledProperty<object?> FirstContentProperty =
        AvaloniaProperty.Register<Resizable, object?>(nameof(FirstContent));

    /// <summary>SecondContent registered property.</summary>
    public static readonly StyledProperty<object?> SecondContentProperty =
        AvaloniaProperty.Register<Resizable, object?>(nameof(SecondContent));

    /// <summary>Orientation registered property.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Resizable, Orientation>(nameof(Orientation),
            defaultValue: Orientation.Horizontal);
#pragma warning restore CS1591

    /// <summary>Content of the first panel (left in Horizontal, top in Vertical).</summary>
    public object? FirstContent
    {
        get => GetValue(FirstContentProperty);
        set => SetValue(FirstContentProperty, value);
    }

    /// <summary>Content of the second panel (right in Horizontal, bottom in Vertical).</summary>
    public object? SecondContent
    {
        get => GetValue(SecondContentProperty);
        set => SetValue(SecondContentProperty, value);
    }

    /// <summary>Splitter orientation: Horizontal — panels side by side; Vertical — panels top/bottom.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Resizable);

    private Grid?             _grid;
    private ContentPresenter? _firstPresenter;
    private ContentPresenter? _secondPresenter;
    private GridSplitter?     _splitter;

    static Resizable()
    {
        OrientationProperty.Changed.AddClassHandler<Resizable>((s, _) => s.ConfigureLayout());

        FirstContentProperty.Changed.AddClassHandler<Resizable>((s, _) =>
        {
            if (s._firstPresenter is not null)
                s._firstPresenter.Content = s.FirstContent;
        });

        SecondContentProperty.Changed.AddClassHandler<Resizable>((s, _) =>
        {
            if (s._secondPresenter is not null)
                s._secondPresenter.Content = s.SecondContent;
        });
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _grid            = e.NameScope.Find<Grid>("PART_Grid");
        _firstPresenter  = e.NameScope.Find<ContentPresenter>("PART_FirstPresenter");
        _splitter        = e.NameScope.Find<GridSplitter>("PART_Splitter");
        _secondPresenter = e.NameScope.Find<ContentPresenter>("PART_SecondPresenter");

        if (_firstPresenter is not null)
            _firstPresenter.Content = FirstContent;
        if (_secondPresenter is not null)
            _secondPresenter.Content = SecondContent;

        ConfigureLayout();
    }

    /// <summary>Configures Grid layout based on the current orientation.</summary>
    private void ConfigureLayout()
    {
        if (_grid is null) return;

        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();

        if (Orientation == Orientation.Horizontal)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(6, GridUnitType.Pixel));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            if (_firstPresenter is not null)
            {
                Grid.SetColumn(_firstPresenter, 0);
                Grid.SetRow(_firstPresenter, 0);
            }
            if (_splitter is not null)
            {
                Grid.SetColumn(_splitter, 1);
                Grid.SetRow(_splitter, 0);
                _splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
                _splitter.Cursor = new Cursor(StandardCursorType.SizeWestEast);
            }
            if (_secondPresenter is not null)
            {
                Grid.SetColumn(_secondPresenter, 2);
                Grid.SetRow(_secondPresenter, 0);
            }
        }
        else
        {
            _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            _grid.RowDefinitions.Add(new RowDefinition(6, GridUnitType.Pixel));
            _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            if (_firstPresenter is not null)
            {
                Grid.SetRow(_firstPresenter, 0);
                Grid.SetColumn(_firstPresenter, 0);
            }
            if (_splitter is not null)
            {
                Grid.SetRow(_splitter, 1);
                Grid.SetColumn(_splitter, 0);
                _splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
                _splitter.Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
            }
            if (_secondPresenter is not null)
            {
                Grid.SetRow(_secondPresenter, 2);
                Grid.SetColumn(_secondPresenter, 0);
            }
        }
    }
}
