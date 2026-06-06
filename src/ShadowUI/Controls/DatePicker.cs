using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ShadowUI;

/// <summary>Date picker: formatted text input with a popup calendar.</summary>
public class DatePicker : TemplatedControl
{
#pragma warning disable CS1591
    /// <summary>SelectedDate registered property.</summary>
    public static readonly StyledProperty<DateTime?> SelectedDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(SelectedDate),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>DateFormat registered property.</summary>
    public static readonly StyledProperty<string> DateFormatProperty =
        AvaloniaProperty.Register<DatePicker, string>(nameof(DateFormat),
            defaultValue: "yyyy-MM-dd");

    /// <summary>Watermark registered property.</summary>
    public static readonly StyledProperty<string?> WatermarkProperty =
        AvaloniaProperty.Register<DatePicker, string?>(nameof(Watermark),
            defaultValue: "Pick a date…");
#pragma warning restore CS1591

    /// <summary>Selected date (null means none selected). Supports two-way binding.</summary>
    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    /// <summary>Date display and input format (defaults to "yyyy-MM-dd").</summary>
    public string DateFormat
    {
        get => GetValue(DateFormatProperty);
        set => SetValue(DateFormatProperty, value);
    }

    /// <summary>Placeholder text for the empty input field.</summary>
    public string? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(DatePicker);

    private TextBox? _textBox;
    private Button? _popupButton;
    private Popup? _popup;
    private ShadowCalendar? _calendar;
    private bool _updatingSync;

    static DatePicker()
    {
        SelectedDateProperty.Changed.AddClassHandler<DatePicker>((s, _) => s.OnSelectedDateChanged());
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _popupButton = e.NameScope.Find<Button>("PART_PopupButton");
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _calendar = e.NameScope.Find<ShadowCalendar>("PART_Calendar");

        if (_popupButton is not null)
            _popupButton.AddHandler(Button.ClickEvent, OnPopupButtonClick);

        if (_textBox is not null)
        {
            _textBox.AddHandler(KeyDownEvent, OnTextBoxKeyDown, RoutingStrategies.Tunnel);
            _textBox.AddHandler(InputElement.LostFocusEvent, OnTextBoxLostFocus);
        }

        if (_calendar is not null)
            _calendar.PropertyChanged += OnCalendarPropertyChanged;

        OnSelectedDateChanged();
    }

    private void OnSelectedDateChanged()
    {
        if (_updatingSync) return;
        _updatingSync = true;

        if (_textBox is not null)
            _textBox.Text = SelectedDate.HasValue
                ? SelectedDate.Value.ToString(DateFormat, CultureInfo.InvariantCulture)
                : string.Empty;

        if (_calendar is not null)
            _calendar.SetCurrentValue(ShadowCalendar.SelectedDateProperty, SelectedDate);

        _updatingSync = false;
    }

    private void OnPopupButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_popup is not null)
            _popup.IsOpen = !_popup.IsOpen;
    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitTextInput();
            e.Handled = true;
        }
    }

    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        CommitTextInput();
    }

    private void CommitTextInput()
    {
        if (_textBox is null) return;
        var text = _textBox.Text ?? string.Empty;
        if (DateTime.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
            SetCurrentValue(SelectedDateProperty, (DateTime?)parsed.Date);
        else
            SetCurrentValue(SelectedDateProperty, (DateTime?)null);
    }

    private void OnCalendarPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ShadowCalendar.SelectedDateProperty)
            OnCalendarDateSelected(e.GetNewValue<DateTime?>());
    }

    private void OnCalendarDateSelected(DateTime? date)
    {
        if (_updatingSync) return;
        _updatingSync = true;

        SetCurrentValue(SelectedDateProperty, date);

        if (_popup is not null)
            _popup.IsOpen = false;

        _updatingSync = false;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_calendar is not null)
            _calendar.PropertyChanged -= OnCalendarPropertyChanged;
        _textBox = null;
        _popupButton = null;
        _popup = null;
        _calendar = null;
        base.OnDetachedFromVisualTree(e);
    }
}
