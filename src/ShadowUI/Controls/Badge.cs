using System;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Small label badge (shadcn Badge analogue).</summary>
public class Badge : ContentControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Badge);
}
