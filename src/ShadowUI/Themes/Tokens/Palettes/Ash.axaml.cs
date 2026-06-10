using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Ash token palette (Light/Dark). AOT-safe.</summary>
public partial class AshPalette : ResourceDictionary
{
    /// <summary>Creates and loads the Ash palette dictionary.</summary>
    public AshPalette() => AvaloniaXamlLoader.Load(this);
}