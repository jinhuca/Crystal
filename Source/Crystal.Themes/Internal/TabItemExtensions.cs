using System.Reflection;

namespace Crystal.Themes.Internal;

internal static class TabItemExtensions
{
  private static readonly MethodInfo? SetFocusMethodInfo = typeof(TabItem).GetMethod("SetFocus", BindingFlags.NonPublic | BindingFlags.Instance);

  internal static bool SetFocus(this TabItem tabItem)
  {
    return (bool?)SetFocusMethodInfo?.Invoke(tabItem, null) ?? false;
  }
}