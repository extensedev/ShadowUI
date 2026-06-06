using Avalonia;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Attached properties for <see cref="TabControl"/>.</summary>
public static class Tabs
{
    /// <summary>When true, the tab bar uses the underline style (underlines the active tab)
    /// instead of a segmented container. Example: <c>shadui:Tabs.Underline="True"</c>.</summary>
    public static readonly AttachedProperty<bool> UnderlineProperty =
        AvaloniaProperty.RegisterAttached<TabControl, bool>("Underline", typeof(Tabs));

#pragma warning disable CS1591
    public static bool GetUnderline(TabControl control) => control.GetValue(UnderlineProperty);
    public static void SetUnderline(TabControl control, bool value) => control.SetValue(UnderlineProperty, value);
#pragma warning restore CS1591
}
