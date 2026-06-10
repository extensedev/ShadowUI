using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Graphite token palette (Light/Dark): elevated graphite dark. AOT-safe.</summary>
public partial class GraphitePalette : ResourceDictionary
{
    /// <summary>Creates and loads the Graphite palette dictionary.</summary>
    public GraphitePalette() => AvaloniaXamlLoader.Load(this);
}
