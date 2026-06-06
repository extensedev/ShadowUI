using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace ShadowUI;

/// <summary>Avatar: image with a text fallback (initials), like shadcn Avatar.</summary>
public class Avatar : TemplatedControl
{
    /// <summary>Avatar image. If null, the initials from <see cref="Fallback"/> are shown.</summary>
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Avatar, IImage?>(nameof(Source));

    /// <summary>Fallback text (e.g., initials).</summary>
    public static readonly StyledProperty<string?> FallbackProperty =
        AvaloniaProperty.Register<Avatar, string?>(nameof(Fallback));

    /// <summary>Avatar image.</summary>
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>Fallback text.</summary>
    public string? Fallback
    {
        get => GetValue(FallbackProperty);
        set => SetValue(FallbackProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Avatar);
}
