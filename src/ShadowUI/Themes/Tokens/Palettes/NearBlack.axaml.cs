using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>NearBlack token palette (Light/Dark): Zinc-derived near-black dark. AOT-safe.</summary>
public partial class NearBlackPalette : ResourceDictionary
{
    /// <summary>Creates and loads the NearBlack palette dictionary.</summary>
    public NearBlackPalette() => AvaloniaXamlLoader.Load(this);
}
