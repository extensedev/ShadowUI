using System;
using Avalonia.Controls;

namespace ShadowUI;

/// <summary>Application menu bar (shadcn Menubar analogue). Keyboard navigation ←→ and Esc is inherited from Avalonia Menu.</summary>
public class Menubar : Menu
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Menubar);
}

/// <summary>Top-level menu bar item. StyleKeyOverride separates its style from the base MenuItem.</summary>
public class MenubarItem : MenuItem
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(MenubarItem);
}
