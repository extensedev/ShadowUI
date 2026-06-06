using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Neutral token palette (Light/Dark). AOT-safe.</summary>
public partial class NeutralPalette : ResourceDictionary
{
    /// <summary>Creates and loads the Neutral palette dictionary.</summary>
    public NeutralPalette() => AvaloniaXamlLoader.Load(this);
}
