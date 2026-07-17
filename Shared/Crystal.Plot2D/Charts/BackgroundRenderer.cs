using System.Windows;

namespace Crystal.Plot2D.Charts;

internal static class BackgroundRenderer {
  #region RenderingFinished event

  private static readonly RoutedEvent renderingFinished = EventManager.RegisterRoutedEvent(
    name: "RenderingFinished",
    routingStrategy: RoutingStrategy.Bubble,
    handlerType: typeof(RoutedEventHandler),
    ownerType: typeof(BackgroundRenderer));

  public static void RaiseRenderingFinished(FrameworkElement eventSource) {
    eventSource.RaiseEvent(e: new RoutedEventArgs(routedEvent: renderingFinished));
  }

  #endregion

  #region RenderingState property

  public static RenderingState GetRenderingState(DependencyObject obj) {
    return (RenderingState)obj.GetValue(dp: RenderingStateProperty);
  }

  public static void SetRenderingState(DependencyObject obj, RenderingState value) {
    obj.SetValue(dp: RenderingStateProperty, value: value);
  }

  public static readonly DependencyProperty RenderingStateProperty = DependencyProperty.RegisterAttached(
    name: "RenderingState",
    propertyType: typeof(RenderingState),
    ownerType: typeof(BackgroundRenderer),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: RenderingState.DuringRendering));

  #endregion RenderingState property

  #region UpdateRequestedEvent

  public static readonly RoutedEvent UpdateRequested = EventManager.RegisterRoutedEvent(
    name: "UpdateRequested",
    routingStrategy: RoutingStrategy.Bubble,
    handlerType: typeof(RoutedEventHandler),
    ownerType: typeof(BackgroundRenderer));

  public static void RaiseUpdateRequested(FrameworkElement eventSource) {
    eventSource.RaiseEvent(e: new RoutedEventArgs(routedEvent: UpdateRequested));
  }

  #endregion

  #region UsesBackgroundRendering

  public static bool GetUsesBackgroundRendering(DependencyObject obj) {
    return (bool)obj.GetValue(dp: UsesBackgroundRenderingProperty);
  }

  public static void SetUsesBackgroundRendering(DependencyObject obj, bool value) {
    obj.SetValue(dp: UsesBackgroundRenderingProperty, value: value);
  }

  public static readonly DependencyProperty UsesBackgroundRenderingProperty = DependencyProperty.RegisterAttached(
    name: "UsesBackgroundRendering",
    propertyType: typeof(bool),
    ownerType: typeof(BackgroundRenderer),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: false));

  #endregion // end of UsesBackgroundRendering
}

internal enum RenderingState {
  DuringRendering,
  RenderingFinished
}
