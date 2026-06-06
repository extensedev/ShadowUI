using System;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Card container (shadcn Card analogue).</summary>
public class Card : ContentControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Card);
}
