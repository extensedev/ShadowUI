using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Blue token palette (Light/Dark). AOT-safe.</summary>
public partial class BluePalette : ResourceDictionary
{
    /// <summary>Creates and loads the Blue palette dictionary.</summary>
    public BluePalette() => AvaloniaXamlLoader.Load(this);
}