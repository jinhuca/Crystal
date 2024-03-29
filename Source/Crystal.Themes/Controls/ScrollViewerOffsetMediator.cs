// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Crystal.Themes.Controls;

public class ScrollViewerOffsetMediator : FrameworkElement
{
  public static readonly DependencyProperty ScrollViewerProperty
    = DependencyProperty.Register(nameof(ScrollViewer),
      typeof(ScrollViewer),
      typeof(ScrollViewerOffsetMediator),
      new PropertyMetadata(default(ScrollViewer), OnScrollViewerChanged));

  private static void OnScrollViewerChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
  {
    if (e.NewValue is ScrollViewer scrollViewer)
    {
      var mediator = (ScrollViewerOffsetMediator)o;
      scrollViewer.ScrollToHorizontalOffset(mediator.HorizontalOffset);
    }
  }

  public ScrollViewer? ScrollViewer
  {
    get => (ScrollViewer?)GetValue(ScrollViewerProperty);
    set => SetValue(ScrollViewerProperty, value);
  }

  public static readonly DependencyProperty HorizontalOffsetProperty
    = DependencyProperty.Register(nameof(HorizontalOffset),
      typeof(double),
      typeof(ScrollViewerOffsetMediator),
      new PropertyMetadata(default(double), OnHorizontalOffsetChanged));

  public double HorizontalOffset
  {
    get => (double)GetValue(HorizontalOffsetProperty);
    set => SetValue(HorizontalOffsetProperty, value);
  }

  private static void OnHorizontalOffsetChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
  {
    var mediator = (ScrollViewerOffsetMediator)o;
    mediator.ScrollViewer?.ScrollToHorizontalOffset((double)e.NewValue);
  }
}