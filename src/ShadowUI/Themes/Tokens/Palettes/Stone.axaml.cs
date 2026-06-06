using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Stone token palette (Light/Dark). AOT-safe.</summary>
public partial class StonePalette : ResourceDictionary
{
    /// <summary>Creates and loads the Stone palette dictionary.</summary>
    public StonePalette() => AvaloniaXamlLoader.Load(this);
}
