using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Orange token palette (Light/Dark). AOT-safe.</summary>
public partial class OrangePalette : ResourceDictionary
{
    /// <summary>Creates and loads the Orange palette dictionary.</summary>
    public OrangePalette() => AvaloniaXamlLoader.Load(this);
}