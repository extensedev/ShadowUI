using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Slate token palette (Light/Dark). AOT-safe.</summary>
public partial class SlatePalette : ResourceDictionary
{
    /// <summary>Creates and loads the Slate palette dictionary.</summary>
    public SlatePalette() => AvaloniaXamlLoader.Load(this);
}
