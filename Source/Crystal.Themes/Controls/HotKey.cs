// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Crystal.Themes.Native;
using Crystal.Themes.Standard;

namespace Crystal.Themes.Controls
{
  public class HotKey : IEquatable<HotKey>
  {
    public HotKey(Key key, ModifierKeys modifierKeys = ModifierKeys.None)
    {
      Key = key;
      ModifierKeys = modifierKeys;
    }

    public Key Key { get; }

    public ModifierKeys ModifierKeys { get; }

    public override bool Equals(object? obj)
    {
      return obj is HotKey key && Equals(key);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return ((int)Key * 397) ^ (int)ModifierKeys;
      }
    }

    public bool Equals(HotKey? other)
    {
      if (other is null)
      {
        return false;
      }

      return Key == other.Key && ModifierKeys == other.ModifierKeys;
    }

#pragma warning disable 618
    public override string ToString()
    {
      var sb = new StringBuilder();
      if ((ModifierKeys & ModifierKeys.Alt) == ModifierKeys.Alt)
      {
        sb.Append(GetLocalizedKeyStringUnsafe(Constants.VK_MENU));
        sb.Append("+");
      }

      if ((ModifierKeys & ModifierKeys.Control) == ModifierKeys.Control)
      {
        sb.Append(GetLocalizedKeyStringUnsafe(Constants.VK_CONTROL));
        sb.Append("+");
      }

      if ((ModifierKeys & ModifierKeys.Shift) == ModifierKeys.Shift)
      {
        sb.Append(GetLocalizedKeyStringUnsafe(Constants.VK_SHIFT));
        sb.Append("+");
      }

      if ((ModifierKeys & ModifierKeys.Windows) == ModifierKeys.Windows)
      {
        sb.Append("Windows+");
      }

      sb.Append(GetLocalizedKeyString(Key));
      return sb.ToString();
    }
#pragma warning restore 618

    private static string GetLocalizedKeyString(Key key)
    {
      if (key >= Key.BrowserBack && key <= Key.LaunchApplication2)
      {
        return key.ToString();
      }

      var vkey = KeyInterop.VirtualKeyFromKey(key);
      return GetLocalizedKeyStringUnsafe(vkey) ?? key.ToString();
    }

#pragma warning disable 618
    private static string? GetLocalizedKeyStringUnsafe(int key)
    {
      // strip any modifier keys
      long keyCode = key & 0xffff;

      var sb = new StringBuilder(256);

      long scanCode = NativeMethods.MapVirtualKey((uint)keyCode, NativeMethods.MapType.MAPVK_VK_TO_VSC);

      // shift the scancode to the high word
      scanCode = (scanCode << 16);
      if (keyCode == 45 ||
          keyCode == 46 ||
          keyCode == 144 ||
          (33 <= keyCode && keyCode <= 40))
      {
        // add the extended key flag
        scanCode |= 0x1000000;
      }

      var resultLength = UnsafeNativeMethods.GetKeyNameText((int)scanCode, sb, 256);
      return resultLength > 0 ? sb.ToString() : null;
    }
#pragma warning restore 618
  }
}