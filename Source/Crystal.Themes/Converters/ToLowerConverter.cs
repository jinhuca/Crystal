﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace Crystal.Themes.Converters;

[MarkupExtensionReturnType(typeof(ToLowerConverter))]
[ValueConversion(typeof(object), typeof(object))]
[ValueConversion(typeof(string), typeof(string))]
public class ToLowerConverter : MarkupConverter
{
  /// <inheritdoc/>
  protected override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return value is string s ? s.ToLower(culture) : value;
  }

  /// <inheritdoc/>
  protected override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return Binding.DoNothing;
  }
}