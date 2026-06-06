using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace ShadowUI;

/// <summary>Breadcrumb container (shadcn Breadcrumb analogue). Place
/// <see cref="BreadcrumbItem"/> and <see cref="BreadcrumbSeparator"/> inside.</summary>
public class Breadcrumb : StackPanel
{
    /// <summary>Creates a horizontal breadcrumb container.</summary>
    public Breadcrumb()
    {
        Orientation = Orientation.Horizontal;
        Spacing = 8;
        VerticalAlignment = VerticalAlignment.Center;
    }
}

/// <summary>Breadcrumb item (link or current page). Class current marks the active item.</summary>
public class BreadcrumbItem : Button
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(BreadcrumbItem);
}

/// <summary>Separator between breadcrumb items (chevron).</summary>
public class BreadcrumbSeparator : TemplatedControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(BreadcrumbSeparator);
}
