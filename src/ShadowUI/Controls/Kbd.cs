using System;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Keyboard key badge (shadcn Kbd analogue).</summary>
public class Kbd : ContentControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Kbd);
}
