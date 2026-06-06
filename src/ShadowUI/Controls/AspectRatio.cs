using Avalonia;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Container that enforces a fixed aspect ratio (shadcn AspectRatio). Ratio = width / height.</summary>
public class AspectRatio : Decorator
{
    /// <summary>Aspect ratio (width / height). Defaults to 1.</summary>
    public static readonly StyledProperty<double> RatioProperty =
        AvaloniaProperty.Register<AspectRatio, double>(nameof(Ratio), 1.0);

    /// <summary>Aspect ratio (width / height).</summary>
    public double Ratio
    {
        get => GetValue(RatioProperty);
        set => SetValue(RatioProperty, value);
    }

    static AspectRatio()
    {
        AffectsMeasure<AspectRatio>(RatioProperty);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var width = availableSize.Width;
        var ratio = Ratio <= 0 ? 1.0 : Ratio;

        if (double.IsInfinity(width))
        {
            Child?.Measure(availableSize);
            return Child?.DesiredSize ?? default;
        }

        var height = width / ratio;
        var size = new Size(width, height);
        Child?.Measure(size);
        return size;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        Child?.Arrange(new Rect(finalSize));
        return finalSize;
    }
}
