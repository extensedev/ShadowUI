using Avalonia;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Additional styling options for ShadowUI buttons.</summary>
public static class ButtonAssist
{
    /// <summary>
    /// Enables a "bounce" animation (slight scale-down) when the button is pressed.
    /// Usage: <c>shadui:ButtonAssist.Bounce="True"</c>.
    /// </summary>
    public static readonly AttachedProperty<bool> BounceProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("Bounce", typeof(ButtonAssist));

    /// <summary>Gets the value of <see cref="BounceProperty"/>.</summary>
    public static bool GetBounce(Control control) => control.GetValue(BounceProperty);

    /// <summary>Sets the value of <see cref="BounceProperty"/>.</summary>
    public static void SetBounce(Control control, bool value) => control.SetValue(BounceProperty, value);
}
