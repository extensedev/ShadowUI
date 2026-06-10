using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Charcoal token palette (Light/Dark). AOT-safe.</summary>
public partial class CharcoalPalette : ResourceDictionary
{
    /// <summary>Creates and loads the Charcoal palette dictionary.</summary>
    public CharcoalPalette() => AvaloniaXamlLoader.Load(this);
}