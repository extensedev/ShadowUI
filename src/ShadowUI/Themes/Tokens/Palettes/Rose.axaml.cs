using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowUI;

/// <summary>Rose token palette (Light/Dark). AOT-safe.</summary>
public partial class RosePalette : ResourceDictionary
{
    /// <summary>Creates and loads the Rose palette dictionary.</summary>
    public RosePalette() => AvaloniaXamlLoader.Load(this);
}