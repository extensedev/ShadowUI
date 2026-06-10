using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Green token palette (Light/Dark). AOT-safe.</summary>
public partial class GreenPalette : ResourceDictionary
{
    /// <summary>Creates and loads the Green palette dictionary.</summary>
    public GreenPalette() => AvaloniaXamlLoader.Load(this);
}