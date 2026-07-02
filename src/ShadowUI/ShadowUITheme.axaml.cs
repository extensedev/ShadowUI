using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace ShadowUI;

/// <summary>
/// Root ShadowUI style. Added to <c>Application.Styles</c>.
/// The <see cref="BaseColor"/> property selects the shadcn neutral palette.
/// </summary>
public class ShadowUITheme : Styles
{
    private IResourceProvider? _palette;

    /// <summary>Creates the theme and applies the default palette (<see cref="ShadowUI.BaseColor.Charcoal"/>).</summary>
    public ShadowUITheme(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);
        UpdatePalette(BaseColor);
    }

    /// <summary>Base neutral palette of the theme.</summary>
    public static readonly StyledProperty<BaseColor> BaseColorProperty =
        AvaloniaProperty.Register<ShadowUITheme, BaseColor>(nameof(BaseColor), BaseColor.Charcoal);

    /// <summary>Base neutral palette of the theme.</summary>
    public BaseColor BaseColor
    {
        get => GetValue(BaseColorProperty);
        set => SetValue(BaseColorProperty, value);
    }

    static ShadowUITheme()
    {
        BaseColorProperty.Changed.AddClassHandler<ShadowUITheme>(
            (theme, e) => theme.UpdatePalette((BaseColor)e.NewValue!));
    }

    private void UpdatePalette(BaseColor color)
    {
        var palette = CreatePalette(color);
        if (_palette is not null)
        {
            Resources.MergedDictionaries.Remove(_palette);
        }

        _palette = palette;
        Resources.MergedDictionaries.Add(palette);
    }

    // AOT-safe: constructs a compiled ResourceDictionary without dynamic URI loading.
    private static IResourceProvider CreatePalette(BaseColor color) => color switch
    {
        BaseColor.Zinc => new ZincPalette(),
        BaseColor.Slate => new SlatePalette(),
        BaseColor.Stone => new StonePalette(),
        BaseColor.Gray => new GrayPalette(),
        BaseColor.Neutral => new NeutralPalette(),
        BaseColor.Graphite => new GraphitePalette(),
        BaseColor.Charcoal => new CharcoalPalette(),
        BaseColor.Ash => new AshPalette(),
        BaseColor.Blue => new BluePalette(),
        BaseColor.Green => new GreenPalette(),
        BaseColor.Violet => new VioletPalette(),
        BaseColor.Rose => new RosePalette(),
        BaseColor.Orange => new OrangePalette(),
        BaseColor.NearBlack => new NearBlackPalette(),
        _ => new ZincPalette(),
    };
}
