using System;
using Avalonia.Controls.Primitives;

namespace ShadowUI;

/// <summary>Pulsing skeleton placeholder (shadcn Skeleton analogue).</summary>
public class Skeleton : TemplatedControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Skeleton);
}
