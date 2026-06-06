using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Zinc token palette (Light/Dark). AOT-safe: compiled <see cref="ResourceDictionary"/>.</summary>
public partial class ZincPalette : ResourceDictionary
{
    /// <summary>Creates and loads the Zinc palette dictionary.</summary>
    public ZincPalette() => AvaloniaXamlLoader.Load(this);
}
