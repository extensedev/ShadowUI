using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;

namespace ShadowUI;

/// <summary>Simple preset-palette color picker (quick shadcn analogue):
/// swatch button + popover with a color grid.</summary>
public class ColorPicker : TemplatedControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<ColorPicker, Color>(
            nameof(SelectedColor), Color.Parse("#2563EB"),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
#pragma warning restore CS1591

    /// <summary>Currently selected color.</summary>
    public Color SelectedColor { get => GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ColorPicker);

    private Popup? _popup;
    private Border? _preview;
    private TextBlock? _hex;

    static ColorPicker()
    {
        SelectedColorProperty.Changed.AddClassHandler<ColorPicker>((c, _) => c.UpdatePreview());
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var trigger = e.NameScope.Find<Button>("PART_Trigger");
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        _preview = e.NameScope.Find<Border>("PART_Swatch");
        _hex = e.NameScope.Find<TextBlock>("PART_Hex");
        if (trigger is not null)
            trigger.AddHandler(Button.ClickEvent, (_, _) => { if (_popup is not null) _popup.IsOpen = !_popup.IsOpen; });

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_preview is not null) _preview.Background = new SolidColorBrush(SelectedColor);
        if (_hex is not null) _hex.Text = "#" + SelectedColor.ToString().Substring(3).ToUpperInvariant();
    }
}
