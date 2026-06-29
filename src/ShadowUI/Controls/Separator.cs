using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace ShadowUI;

/// <summary>
/// Thin divider line (shadcn Separator analogue) with optional edge-fade.
/// Set <see cref="FadeEdges"/> to fade both ends smoothly to transparent; assign a
/// <c>LinearGradientBrush</c> to <see cref="TemplatedControl.Background"/> to color the
/// line with a custom gradient — the edge-fade composes with any background brush.
/// </summary>
public class Separator : TemplatedControl
{
#pragma warning disable CS1591
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Separator, Orientation>(nameof(Orientation));

    public static readonly StyledProperty<bool> FadeEdgesProperty =
        AvaloniaProperty.Register<Separator, bool>(nameof(FadeEdges));

    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<Separator, double>(nameof(Thickness), defaultValue: 1);
#pragma warning restore CS1591

    /// <summary>Layout direction. Horizontal (default) draws a wide line; Vertical draws a tall one.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>When true, both ends fade smoothly to transparent via an axis-aware opacity mask.</summary>
    public bool FadeEdges
    {
        get => GetValue(FadeEdgesProperty);
        set => SetValue(FadeEdgesProperty, value);
    }

    /// <summary>Line thickness in device-independent pixels (defaults to 1).</summary>
    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Separator);

    static Separator()
    {
        OrientationProperty.Changed.AddClassHandler<Separator>((s, _) => s.UpdatePseudoClasses());
        FadeEdgesProperty.Changed.AddClassHandler<Separator>((s, _) => s.UpdatePseudoClasses());
    }

    /// <summary>Initializes the separator and applies the initial pseudo-class state.</summary>
    public Separator() => UpdatePseudoClasses();

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
        PseudoClasses.Set(":fade", FadeEdges);
    }
}
