﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace Crystal.Themes.Converters;

/// <summary>
/// Converts a String into a Visibility enumeration (and back).
/// The FalseEquivalent can be declared with the "FalseEquivalent" property.
/// </summary>
[ValueConversion(typeof(string), typeof(Visibility))]
[MarkupExtensionReturnType(typeof(StringToVisibilityConverter))]
public class StringToVisibilityConverter : MarkupConverter
{
  /// <summary>
  /// Initialize the properties with standard values
  /// </summary>
  public StringToVisibilityConverter()
  {
    FalseEquivalent = Visibility.Collapsed;
    OppositeStringValue = false;
  }

  /// <summary>
  /// FalseEquivalent (default : Visibility.Collapsed => see Constructor)
  /// </summary>
  public Visibility FalseEquivalent { get; set; }

  /// <summary>
  /// Define whether the opposite boolean value is crucial (default : false)
  /// </summary>
  public bool OppositeStringValue { get; set; }

  /// <inheritdoc/>
  protected override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is null or string && targetType == typeof(Visibility))
    {
      if (OppositeStringValue)
      {
        return string.IsNullOrEmpty((string?)value) ? Visibility.Visible : FalseEquivalent;
      }

      return string.IsNullOrEmpty((string?)value) ? FalseEquivalent : Visibility.Visible;
    }

    return default(Visibility);
  }

  /// <inheritdoc/>
  protected override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return DependencyProperty.UnsetValue;
  }
}