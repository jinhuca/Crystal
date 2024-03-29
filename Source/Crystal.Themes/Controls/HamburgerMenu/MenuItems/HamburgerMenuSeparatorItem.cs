﻿namespace Crystal.Themes.Controls;

/// <summary>
/// The HamburgerMenuSeparatorItem provides an separator based implementation for HamburgerMenu entries.
/// </summary>
public class HamburgerMenuSeparatorItem : HamburgerMenuItemBase, IHamburgerMenuSeparatorItem
{
  protected override Freezable CreateInstanceCore()
  {
    return new HamburgerMenuSeparatorItem();
  }
}