﻿using Crystal.Themes.Theming;
using Crystal.Themes.Windows.Shell;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// This class eats little children.
  /// </summary>
  internal static class CrystalWindowHelpers
  {
    /// <summary>
    /// Sets the IsHitTestVisibleInChromeProperty to a MetroWindow template child
    /// </summary>
    /// <param name="window">The MetroWindow.</param>
    /// <param name="name">The name of the template child.</param>
    /// <param name="hitTestVisible"></param>
    public static void SetIsHitTestVisibleInChromeProperty<T>([NotNull] this CrystalWindow window, string name, bool hitTestVisible = true)
        where T : class
    {
      if (window is null)
      {
        throw new ArgumentNullException(nameof(window));
      }

      var inputElement = window.GetPart<T>(name) as IInputElement;
      Debug.Assert(inputElement != null, $"{name} is not a IInputElement");
      if (inputElement is not null && WindowChrome.GetIsHitTestVisibleInChrome(inputElement) != hitTestVisible)
      {
        WindowChrome.SetIsHitTestVisibleInChrome(inputElement, hitTestVisible);
      }
    }

    /// <summary>
    /// Sets the WindowChrome ResizeGripDirection to a MetroWindow template child.
    /// </summary>
    /// <param name="window">The MetroWindow.</param>
    /// <param name="name">The name of the template child.</param>
    /// <param name="direction">The direction.</param>
    public static void SetWindowChromeResizeGripDirection([NotNull] this CrystalWindow window, string name, ResizeGripDirection direction)
    {
      if (window is null)
      {
        throw new ArgumentNullException(nameof(window));
      }

      var inputElement = window.GetPart<IInputElement>(name);
      Debug.Assert(inputElement != null, $"{name} is not a IInputElement");
      if (inputElement is not null && WindowChrome.GetResizeGripDirection(inputElement) != direction)
      {
        WindowChrome.SetResizeGripDirection(inputElement, direction);
      }
    }

    /// <summary>
    /// Adapts the WindowCommands to the theme of the first opened, topmost &amp;&amp; (top || right || left) flyout
    /// </summary>
    /// <param name="window">The MetroWindow</param>
    /// <param name="flyouts">All the flyouts! Or flyouts that fall into the category described in the summary.</param>
    public static void HandleWindowCommandsForFlyouts(this CrystalWindow window, IEnumerable<Flyout> flyouts)
    {
      var allOpenFlyouts = flyouts.Where(x => x.IsOpen).ToList();

      var anyFlyoutOpen = allOpenFlyouts.Any(x => x.Position != Position.Bottom);
      if (!anyFlyoutOpen)
      {
        window.ResetAllWindowCommandsBrush();
      }

      var topFlyout = allOpenFlyouts
                      .Where(x => x.Position == Position.Top)
                      .OrderByDescending(Panel.GetZIndex)
                      .FirstOrDefault();
      if (topFlyout != null)
      {
        window.UpdateWindowCommandsForFlyout(topFlyout);
      }
      else
      {
        var leftFlyout = allOpenFlyouts
                         .Where(x => x.Position == Position.Left)
                         .OrderByDescending(Panel.GetZIndex)
                         .FirstOrDefault();
        if (leftFlyout != null)
        {
          window.UpdateWindowCommandsForFlyout(leftFlyout);
        }

        var rightFlyout = allOpenFlyouts
                          .Where(x => x.Position == Position.Right)
                          .OrderByDescending(Panel.GetZIndex)
                          .FirstOrDefault();
        if (rightFlyout != null)
        {
          window.UpdateWindowCommandsForFlyout(rightFlyout);
        }
      }
    }

    [CanBeNull]
    private static Theme? GetCurrentTheme([NotNull] CrystalWindow window)
    {
      if (window is null)
      {
        throw new ArgumentNullException(nameof(window));
      }

      var currentTheme = ThemeManager.Current.DetectTheme(window);
      if (currentTheme is null)
      {
        var application = Application.Current;
        if (application is not null)
        {
          currentTheme = application.MainWindow is null
              ? ThemeManager.Current.DetectTheme(application)
              : ThemeManager.Current.DetectTheme(application.MainWindow);
        }
      }

      return currentTheme;
    }

    public static void ResetAllWindowCommandsBrush(this CrystalWindow window)
    {
      var currentTheme = GetCurrentTheme(window);

      window.ChangeAllWindowCommandsBrush(window.OverrideDefaultWindowCommandsBrush, currentTheme);
      window.ChangeAllWindowButtonCommandsBrush(window.OverrideDefaultWindowCommandsBrush, currentTheme);
    }

    public static void UpdateWindowCommandsForFlyout(this CrystalWindow window, Flyout flyout)
    {
      var currentTheme = GetCurrentTheme(window);

      window.ChangeAllWindowCommandsBrush(window.OverrideDefaultWindowCommandsBrush, currentTheme);
      window.ChangeAllWindowButtonCommandsBrush(window.OverrideDefaultWindowCommandsBrush ?? flyout.Foreground, currentTheme, flyout.Theme, flyout.Position);
    }

    private static void ChangeAllWindowCommandsBrush(this CrystalWindow window, Brush? foregroundBrush, Theme? currentAppTheme)
    {
      if (foregroundBrush is null)
      {
        window.LeftWindowCommands?.ClearValue(Control.ForegroundProperty);
        window.RightWindowCommands?.ClearValue(Control.ForegroundProperty);
      }

      // set the theme based on current application or window theme
      var theme = currentAppTheme != null && currentAppTheme.BaseColorScheme == ThemeManager.BaseColorDark
          ? ThemeManager.BaseColorDark
          : ThemeManager.BaseColorLight;

      // set the theme to light by default
      window.LeftWindowCommands?.SetValue(WindowCommands.ThemeProperty, theme);
      window.RightWindowCommands?.SetValue(WindowCommands.ThemeProperty, theme);

      // clear or set the foreground property
      if (foregroundBrush != null)
      {
        window.LeftWindowCommands?.SetValue(Control.ForegroundProperty, foregroundBrush);
        window.RightWindowCommands?.SetValue(Control.ForegroundProperty, foregroundBrush);
      }
    }

    private static void ChangeAllWindowButtonCommandsBrush(this CrystalWindow window, Brush? foregroundBrush, Theme? currentAppTheme, FlyoutTheme flyoutTheme = FlyoutTheme.Adapt, Position position = Position.Top)
    {
      if (position == Position.Right || position == Position.Top)
      {
        if (foregroundBrush is null)
        {
          window.WindowButtonCommands?.ClearValue(Control.ForegroundProperty);
        }

        // set the theme based on color lightness
        // otherwise set the theme based on current application or window theme
        var theme = currentAppTheme != null && currentAppTheme.BaseColorScheme == ThemeManager.BaseColorDark
            ? ThemeManager.BaseColorDark
            : ThemeManager.BaseColorLight;

        theme = flyoutTheme switch
        {
          FlyoutTheme.Light => ThemeManager.BaseColorLight,
          FlyoutTheme.Dark => ThemeManager.BaseColorDark,
          FlyoutTheme.Inverse => theme == ThemeManager.BaseColorLight ? ThemeManager.BaseColorDark : ThemeManager.BaseColorLight,
          _ => theme
        };

        window.WindowButtonCommands?.SetValue(WindowButtonCommands.ThemeProperty, theme);

        // clear or set the foreground property
        if (foregroundBrush != null)
        {
          window.WindowButtonCommands?.SetValue(Control.ForegroundProperty, foregroundBrush);
        }
      }
    }
  }
}