using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace ShadowUI;

/// <summary>Input mask type for OtpInput.</summary>
public enum OtpInputType
{
    /// <summary>Digits 0-9 only.</summary>
    Digit,
    /// <summary>Letters and digits.</summary>
    AlphaNumeric,
}

/// <summary>One-time password input (OTP): N independent cells with auto-advance and paste support.</summary>
public class OtpInput : TemplatedControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<int> LengthProperty =
        AvaloniaProperty.Register<OtpInput, int>(nameof(Length), defaultValue: 6);

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<OtpInput, string?>(nameof(Value), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<OtpInputType> InputTypeProperty =
        AvaloniaProperty.Register<OtpInput, OtpInputType>(nameof(InputType), defaultValue: OtpInputType.Digit);
#pragma warning restore CS1591

    /// <summary>Number of input cells.</summary>
    public int Length
    {
        get => GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    /// <summary>Aggregated value from all cells (TwoWay).</summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Allowed character type: digits or alphanumeric.</summary>
    public OtpInputType InputType
    {
        get => GetValue(InputTypeProperty);
        set => SetValue(InputTypeProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(OtpInput);

    private readonly List<TextBox> _cells = new();
    private StackPanel? _cellsPanel;
    private bool _updatingValue = false;

    static OtpInput()
    {
        LengthProperty.Changed.AddClassHandler<OtpInput>((s, _) => s.RebuildCells());
        ValueProperty.Changed.AddClassHandler<OtpInput>((s, _) => s.SyncCellsFromValue());
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _cellsPanel = e.NameScope.Find<StackPanel>("PART_CellsPanel");
        BuildCells();
    }

    private void BuildCells()
    {
        if (_cellsPanel is null) return;
        _cells.Clear();
        _cellsPanel.Children.Clear();
        for (int i = 0; i < Length; i++)
        {
            var cell = CreateCell(i);
            _cells.Add(cell);
            _cellsPanel.Children.Add(cell);
        }
        SyncCellsFromValue();
    }

    private void RebuildCells() => BuildCells();

    private TextBox CreateCell(int index)
    {
        var cell = new TextBox
        {
            MaxLength = 1,
            Width = 40,
            Height = 40,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        cell.Classes.Add("otp-cell");
        cell.AddHandler(TextBox.TextChangedEvent, (_, _) => OnCellTextChanged(index));
        cell.AddHandler(KeyDownEvent, (_, e) => OnCellKeyDown(index, e), RoutingStrategies.Tunnel);
        return cell;
    }

    private void OnCellTextChanged(int index)
    {
        if (_updatingValue) return;
        if (index < 0 || index >= _cells.Count) return;
        var cell = _cells[index];
        var text = cell.Text ?? "";
        if (!IsCharAllowed(text))
        {
            cell.Text = "";
            return;
        }
        if (text.Length == 1 && index + 1 < _cells.Count)
            _cells[index + 1].Focus();
        SyncValueFromCells();
    }

    private void OnCellKeyDown(int index, KeyEventArgs e)
    {
        if (index < 0 || index >= _cells.Count) return;
        if (e.Key == Key.Back && string.IsNullOrEmpty(_cells[index].Text) && index > 0)
        {
            _cells[index - 1].Focus();
            e.Handled = true;
        }
        if (e.Key == Key.V && e.KeyModifiers == KeyModifiers.Control)
        {
            _ = HandlePasteAsync(index);
            e.Handled = true;
        }
    }

    private bool IsCharAllowed(string text)
    {
        if (string.IsNullOrEmpty(text)) return true;
        return InputType == OtpInputType.Digit
            ? char.IsDigit(text[0])
            : char.IsLetterOrDigit(text[0]);
    }

    private async Task HandlePasteAsync(int startIndex)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null) return;
        var pasted = await clipboard.TryGetTextAsync();
        if (string.IsNullOrEmpty(pasted)) return;
        _updatingValue = true;
        for (int i = 0; i < pasted.Length && (startIndex + i) < _cells.Count; i++)
        {
            var ch = pasted[i].ToString();
            if (IsCharAllowed(ch))
                _cells[startIndex + i].Text = ch;
        }
        _updatingValue = false;
        SyncValueFromCells();
    }

    private void SyncValueFromCells()
    {
        if (_updatingValue) return;
        _updatingValue = true;
        SetCurrentValue(ValueProperty, string.Concat(_cells.Select(c => c.Text ?? "")));
        _updatingValue = false;
    }

    private void SyncCellsFromValue()
    {
        if (_updatingValue) return;
        if (_cells.Count == 0) return;
        var val = Value ?? "";
        _updatingValue = true;
        for (int i = 0; i < _cells.Count; i++)
            _cells[i].Text = i < val.Length ? val[i].ToString() : "";
        _updatingValue = false;
    }
}
