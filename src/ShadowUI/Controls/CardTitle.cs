using System;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Card title.</summary>
public class CardTitle : ContentControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CardTitle);
}
