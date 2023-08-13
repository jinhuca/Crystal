// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Crystal.Themes.Controls;

public class PivotItem : ContentControl
{
  public static readonly DependencyProperty HeaderProperty
    = DependencyProperty.Register(nameof(Header),
      typeof(object),
      typeof(PivotItem),
      new PropertyMetadata(default(string)));

  /// <summary>
  /// Gets or sets the Header of the <see cref="PivotItem"/>.
  /// </summary>
  public object? Header
  {
    get => GetValue(HeaderProperty);
    set => SetValue(HeaderProperty, value);
  }

  /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty HeaderTemplateProperty
    = DependencyProperty.Register(nameof(HeaderTemplate),
      typeof(DataTemplate),
      typeof(PivotItem));

  /// <summary>
  /// Gets or sets the HeaderTemplate of the <see cref="PivotItem"/>.
  /// </summary>
  public DataTemplate? HeaderTemplate
  {
    get => (DataTemplate?)GetValue(HeaderTemplateProperty);
    set => SetValue(HeaderTemplateProperty, value);
  }

  static PivotItem()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(PivotItem), new FrameworkPropertyMetadata(typeof(PivotItem)));
  }

  public PivotItem()
  {
    RequestBringIntoView += (s, e) => { e.Handled = true; };
  }
}