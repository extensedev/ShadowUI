using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Violet token palette (Light/Dark). AOT-safe.</summary>
public partial class VioletPalette : ResourceDictionary
{
    /// <summary>Creates and loads the Violet palette dictionary.</summary>
    public VioletPalette() => AvaloniaXamlLoader.Load(this);
}