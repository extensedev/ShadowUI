using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>
///     Attached properties for smooth scrolling on <see cref="ScrollViewer" />.
///     <para>Usage: set <see cref="IsEnabledProperty" /> to <c>true</c> on a ScrollViewer.</para>
///     Adapted from ShadUI (EvoTweaker).
/// </summary>
public static class SmoothScrollAssist
{
    private static readonly bool IsMacOs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private static readonly ConditionalWeakTable<ScrollViewer, SmoothScrollController> Controllers = new();

    static SmoothScrollAssist()
    {
        IsEnabledProperty.Changed.AddClassHandler<ScrollViewer>(IsEnabledChanged);
        BaseStepSizeProperty.Changed.AddClassHandler<ScrollViewer>(BaseStepSizeChanged);
        SmoothingFactorProperty.Changed.AddClassHandler<ScrollViewer>(SmoothingFactorChanged);
    }

    /// <summary>Whether smooth scrolling is enabled.</summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, bool>("IsEnabled", typeof(SmoothScrollAssist));

    /// <summary>Gets the value of <see cref="IsEnabledProperty" />.</summary>
    public static bool GetIsEnabled(ScrollViewer scrollViewer) => scrollViewer.GetValue(IsEnabledProperty);

    /// <summary>Sets the value of <see cref="IsEnabledProperty" />.</summary>
    public static void SetIsEnabled(ScrollViewer scrollViewer, bool value) =>
        scrollViewer.SetValue(IsEnabledProperty, value);

    private static void IsEnabledChanged(ScrollViewer scrollViewer, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true && !Controllers.TryGetValue(scrollViewer, out _))
        {
            scrollViewer.IsScrollInertiaEnabled = false;

            var baseStepSize = GetBaseStepSize(scrollViewer);
            var smoothingFactor = GetSmoothingFactor(scrollViewer);
            SmoothScrollController controller = new(scrollViewer, baseStepSize, smoothingFactor);

            Controllers.Add(scrollViewer, controller);
        }
        else if (Controllers.TryGetValue(scrollViewer, out var controller))
        {
            controller.Stop();

            Controllers.Remove(scrollViewer);

            scrollViewer.IsScrollInertiaEnabled = true;
        }
    }

    /// <summary>Scroll distance per one wheel «click».</summary>
    public static readonly AttachedProperty<double> BaseStepSizeProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, double>(
            "BaseStepSize", typeof(SmoothScrollAssist), IsMacOs ? 40 : 70);

    /// <summary>Gets the value of <see cref="BaseStepSizeProperty" />.</summary>
    public static double GetBaseStepSize(ScrollViewer scrollViewer) => scrollViewer.GetValue(BaseStepSizeProperty);

    /// <summary>Sets the value of <see cref="BaseStepSizeProperty" />.</summary>
    public static void SetBaseStepSize(ScrollViewer scrollViewer, double value) =>
        scrollViewer.SetValue(BaseStepSizeProperty, value);

    private static void BaseStepSizeChanged(ScrollViewer scrollViewer, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not double newValue || !Controllers.TryGetValue(scrollViewer, out var controller))
            return;

        controller.BaseStepSize = newValue;
    }

    /// <summary>Smoothing intensity. Higher = snappier; lower = smoother/more inertial.</summary>
    public static readonly AttachedProperty<double> SmoothingFactorProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, double>(
            "SmoothingFactor", typeof(SmoothScrollAssist), IsMacOs ? 50 : 20);

    /// <summary>Gets the value of <see cref="SmoothingFactorProperty" />.</summary>
    public static double GetSmoothingFactor(ScrollViewer scrollViewer) =>
        scrollViewer.GetValue(SmoothingFactorProperty);

    /// <summary>Sets the value of <see cref="SmoothingFactorProperty" />.</summary>
    public static void SetSmoothingFactor(ScrollViewer scrollViewer, double value) =>
        scrollViewer.SetValue(SmoothingFactorProperty, value);

    private static void SmoothingFactorChanged(ScrollViewer scrollViewer, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not double newValue || !Controllers.TryGetValue(scrollViewer, out var controller))
            return;

        controller.SmoothingFactor = newValue;
    }
}
