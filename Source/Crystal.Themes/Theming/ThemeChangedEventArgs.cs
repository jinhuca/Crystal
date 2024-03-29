﻿#nullable enable
namespace Crystal.Themes.Theming;

/// <summary>
/// Class which is used as argument for an event to signal theme changes.
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
  /// <summary>
  /// Creates a new instance of this class.
  /// </summary>
  public ThemeChangedEventArgs(object? target, ResourceDictionary targetResourceDictionary, Theme? oldTheme, Theme newTheme)
  {
    Target = target;
    TargetResourceDictionary = targetResourceDictionary;
    OldTheme = oldTheme;
    NewTheme = newTheme;
  }

  /// <summary>
  /// The target object for which was targeted by the theme change.
  /// </summary>
  public object? Target { get; }

  /// <summary>
  /// The <see cref="ResourceDictionary"/> for which was targeted by the theme change.
  /// </summary>
  public ResourceDictionary TargetResourceDictionary { get; }

  /// <summary>
  /// The old theme.
  /// </summary>
  public Theme? OldTheme { get; set; }

  /// <summary>
  /// The new theme.
  /// </summary>
  public Theme NewTheme { get; set; }
}