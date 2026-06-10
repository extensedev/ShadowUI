using System;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Horizontal toolbar: a container for buttons, toggle groups and separators.</summary>
public class Toolbar : ItemsControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Toolbar);
}
