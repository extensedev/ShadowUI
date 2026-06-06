using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Gray token palette (Light/Dark). AOT-safe.</summary>
public partial class GrayPalette : ResourceDictionary
{
    /// <summary>Creates and loads the Gray palette dictionary.</summary>
    public GrayPalette() => AvaloniaXamlLoader.Load(this);
}
