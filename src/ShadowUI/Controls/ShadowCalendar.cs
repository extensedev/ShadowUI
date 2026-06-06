using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace ShadowUI;

/// <summary>Date selection mode for ShadowCalendar.</summary>
public enum CalendarMode
{
    /// <summary>Single date selection.</summary>
    Single,
    /// <summary>Range selection: start and end date.</summary>
    Range,
}

/// <summary>Custom calendar with a month grid, single and range date selection (shadcn Calendar analogue).</summary>
public class ShadowCalendar : TemplatedControl
{
#pragma warning disable CS1591
    /// <summary>SelectedDate registered property.</summary>
    public static readonly StyledProperty<DateTime?> SelectedDateProperty =
        AvaloniaProperty.Register<ShadowCalendar, DateTime?>(nameof(SelectedDate),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Mode registered property.</summary>
    public static readonly StyledProperty<CalendarMode> ModeProperty =
        AvaloniaProperty.Register<ShadowCalendar, CalendarMode>(nameof(Mode),
            defaultValue: CalendarMode.Single);

    /// <summary>SelectedStartDate registered property.</summary>
    public static readonly StyledProperty<DateTime?> SelectedStartDateProperty =
        AvaloniaProperty.Register<ShadowCalendar, DateTime?>(nameof(SelectedStartDate),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>SelectedEndDate registered property.</summary>
    public static readonly StyledProperty<DateTime?> SelectedEndDateProperty =
        AvaloniaProperty.Register<ShadowCalendar, DateTime?>(nameof(SelectedEndDate),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>DisplayMonth registered property.</summary>
    public static readonly StyledProperty<DateTime> DisplayMonthProperty =
        AvaloniaProperty.Register<ShadowCalendar, DateTime>(nameof(DisplayMonth),
            defaultValue: new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
#pragma warning restore CS1591

    /// <summary>Selected date (Single mode, TwoWay).</summary>
    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    /// <summary>Selection mode: Single or Range.</summary>
    public CalendarMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>Range start date (Range mode, TwoWay).</summary>
    public DateTime? SelectedStartDate
    {
        get => GetValue(SelectedStartDateProperty);
        set => SetValue(SelectedStartDateProperty, value);
    }

    /// <summary>Range end date (Range mode, TwoWay).</summary>
    public DateTime? SelectedEndDate
    {
        get => GetValue(SelectedEndDateProperty);
        set => SetValue(SelectedEndDateProperty, value);
    }

    /// <summary>Currently displayed month (always the first day of the month).</summary>
    public DateTime DisplayMonth
    {
        get => GetValue(DisplayMonthProperty);
        set => SetValue(DisplayMonthProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ShadowCalendar);

    private UniformGrid? _daysGrid;
    private readonly List<Button> _dayButtons = new();
    private TextBlock? _monthYearText;
    private Button? _prevButton;
    private Button? _nextButton;

    static ShadowCalendar()
    {
        DisplayMonthProperty.Changed.AddClassHandler<ShadowCalendar>((s, _) => s.RefreshDayGrid());
        SelectedDateProperty.Changed.AddClassHandler<ShadowCalendar>((s, _) => s.RefreshDayGrid());
        SelectedStartDateProperty.Changed.AddClassHandler<ShadowCalendar>((s, _) => s.RefreshDayGrid());
        SelectedEndDateProperty.Changed.AddClassHandler<ShadowCalendar>((s, _) => s.RefreshDayGrid());
        ModeProperty.Changed.AddClassHandler<ShadowCalendar>((s, _) =>
        {
            s.SetCurrentValue(SelectedDateProperty, (DateTime?)null);
            s.SetCurrentValue(SelectedStartDateProperty, (DateTime?)null);
            s.SetCurrentValue(SelectedEndDateProperty, (DateTime?)null);
            s.RefreshDayGrid();
        });
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_prevButton is not null)
            _prevButton.RemoveHandler(Button.ClickEvent, OnPrevClick);
        if (_nextButton is not null)
            _nextButton.RemoveHandler(Button.ClickEvent, OnNextClick);

        _prevButton = e.NameScope.Find<Button>("PART_PrevButton");
        _nextButton = e.NameScope.Find<Button>("PART_NextButton");
        _monthYearText = e.NameScope.Find<TextBlock>("PART_MonthYearText");
        _daysGrid = e.NameScope.Find<UniformGrid>("PART_DaysGrid");

        if (_prevButton is not null)
            _prevButton.AddHandler(Button.ClickEvent, OnPrevClick);
        if (_nextButton is not null)
            _nextButton.AddHandler(Button.ClickEvent, OnNextClick);

        BuildDayGrid();
    }

    private void OnPrevClick(object? sender, RoutedEventArgs e)
    {
        DisplayMonth = DisplayMonth.AddMonths(-1);
    }

    private void OnNextClick(object? sender, RoutedEventArgs e)
    {
        DisplayMonth = DisplayMonth.AddMonths(1);
    }

    private void BuildDayGrid()
    {
        if (_daysGrid is null) return;

        foreach (var btn in _dayButtons)
            btn.RemoveHandler(Button.ClickEvent, OnDayButtonClick);

        _daysGrid.Children.Clear();
        _dayButtons.Clear();

        // 6 weeks × 7 days = 42 buttons
        for (int i = 0; i < 42; i++)
        {
            var button = new Button { Tag = i };
            button.AddHandler(Button.ClickEvent, OnDayButtonClick);
            _daysGrid.Children.Add(button);
            _dayButtons.Add(button);
        }

        RefreshDayGrid();
    }

    private void RefreshDayGrid()
    {
        if (_daysGrid is null) return;

        var firstDayOfMonth = new DateTime(DisplayMonth.Year, DisplayMonth.Month, 1);
        // Offset: Monday = 0, Sunday = 6
        int offset = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

        for (int i = 0; i < _dayButtons.Count; i++)
        {
            var button = _dayButtons[i];
            var date = firstDayOfMonth.AddDays(i - offset);
            button.Tag = date;
            button.Content = date.Day.ToString();
            UpdateDayButtonClasses(button, date);
        }

        if (_monthYearText is not null)
            _monthYearText.Text = DisplayMonth.ToString("MMMM yyyy", CultureInfo.CurrentUICulture);
    }

    private void UpdateDayButtonClasses(Button btn, DateTime date)
    {
        btn.Classes.Remove("calendar-day-other-month");
        btn.Classes.Remove("calendar-day-today");
        btn.Classes.Remove("calendar-day-selected");
        btn.Classes.Remove("calendar-day-range-start");
        btn.Classes.Remove("calendar-day-range-end");
        btn.Classes.Remove("calendar-day-in-range");

        if (date.Month != DisplayMonth.Month)
            btn.Classes.Add("calendar-day-other-month");

        if (date.Date == DateTime.Today)
            btn.Classes.Add("calendar-day-today");

        if (Mode == CalendarMode.Single)
        {
            if (SelectedDate.HasValue && date.Date == SelectedDate.Value.Date)
                btn.Classes.Add("calendar-day-selected");
        }
        else if (Mode == CalendarMode.Range)
        {
            if (SelectedStartDate.HasValue && date.Date == SelectedStartDate.Value.Date)
            {
                btn.Classes.Add("calendar-day-range-start");
                btn.Classes.Add("calendar-day-selected");
            }

            if (SelectedEndDate.HasValue && date.Date == SelectedEndDate.Value.Date)
            {
                btn.Classes.Add("calendar-day-range-end");
                btn.Classes.Add("calendar-day-selected");
            }

            if (SelectedStartDate.HasValue && SelectedEndDate.HasValue
                && date.Date > SelectedStartDate.Value.Date
                && date.Date < SelectedEndDate.Value.Date)
            {
                btn.Classes.Add("calendar-day-in-range");
            }
        }
    }

    private void OnDayButtonClick(object? sender, RoutedEventArgs e)
    {
        // T-04-01-01: verify Tag type before use
        if (sender is not Button btn || btn.Tag is not DateTime date) return;

        if (Mode == CalendarMode.Single)
        {
            if (SelectedDate.HasValue && date.Date == SelectedDate.Value.Date)
                SetCurrentValue(SelectedDateProperty, (DateTime?)null);
            else
                SetCurrentValue(SelectedDateProperty, (DateTime?)date.Date);
        }
        else if (Mode == CalendarMode.Range)
        {
            if (SelectedStartDate is null)
            {
                SetCurrentValue(SelectedStartDateProperty, (DateTime?)date.Date);
                SetCurrentValue(SelectedEndDateProperty, (DateTime?)null);
            }
            else if (SelectedEndDate is null && date.Date >= SelectedStartDate.Value.Date)
            {
                SetCurrentValue(SelectedEndDateProperty, (DateTime?)date.Date);
            }
            else
            {
                // Third click — reset and start a new selection
                SetCurrentValue(SelectedStartDateProperty, (DateTime?)date.Date);
                SetCurrentValue(SelectedEndDateProperty, (DateTime?)null);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_prevButton is not null)
            _prevButton.RemoveHandler(Button.ClickEvent, OnPrevClick);
        if (_nextButton is not null)
            _nextButton.RemoveHandler(Button.ClickEvent, OnNextClick);

        foreach (var btn in _dayButtons)
            btn.RemoveHandler(Button.ClickEvent, OnDayButtonClick);

        _dayButtons.Clear();
        base.OnDetachedFromVisualTree(e);
    }
}
