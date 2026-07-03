using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ShadowUI;

/// <summary>
///     Resolves a dialog/sheet card's effective max height as the smaller of an explicit
///     max-height setting and the available window/overlay height (minus a margin).
/// </summary>
public sealed class DialogCardMaxHeightConverter : IMultiValueConverter
{
    /// <summary>Shared instance for AOT-safe use from AXAML via <c>x:Static</c>.</summary>
    public static readonly DialogCardMaxHeightConverter Instance = new();

    /// <summary>Converts (explicit max height, overlay height) plus a margin parameter to the effective max height.</summary>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var explicitMax = values.Count > 0 && values[0] is double d0 ? d0 : double.PositiveInfinity;
        var overlayHeight = values.Count > 1 && values[1] is double d1 ? d1 : double.PositiveInfinity;
        var margin = parameter switch
        {
            double m => m,
            string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => 0d,
        };

        var windowCap = double.IsFinite(overlayHeight)
            ? Math.Max(0d, overlayHeight - margin)
            : double.PositiveInfinity;

        if (!double.IsFinite(explicitMax))
            return windowCap;

        if (!double.IsFinite(windowCap))
            return explicitMax;

        return Math.Min(explicitMax, windowCap);
    }
}
