using System;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Card subtitle / description.</summary>
public class CardDescription : ContentControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CardDescription);
}
