using System;
using Avalonia.Controls.Primitives;

namespace ShadowUI;

/// <summary>Loading indicator (shadcn Spinner analogue). Size via Classes: sm / lg.</summary>
public class Spinner : TemplatedControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Spinner);
}
