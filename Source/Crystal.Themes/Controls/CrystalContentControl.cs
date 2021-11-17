using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// A ContentControl which use a transition to slide in the content.
  /// </summary>
  [TemplatePart(Name = "AfterLoadedStoryboard", Type = typeof(Storyboard))]
  [TemplatePart(Name = "AfterLoadedReverseStoryboard", Type = typeof(Storyboard))]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("WpfAnalyzers.TemplatePart", "WPF0132:Use PART prefix.", Justification = "<Pending>")]
  public class CrystalContentControl : ContentControl
  {
    private Storyboard? afterLoadedStoryboard;
    private Storyboard? afterLoadedReverseStoryboard;
    private bool transitionLoaded;

    public static readonly DependencyProperty ReverseTransitionProperty = DependencyProperty.Register(
      nameof(ReverseTransition),
      typeof(bool),
      typeof(CrystalContentControl),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

    public bool ReverseTransition
    {
      get => (bool)GetValue(ReverseTransitionProperty);
      set => SetValue(ReverseTransitionProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty TransitionsEnabledProperty = DependencyProperty.Register(
      nameof(TransitionsEnabled),
      typeof(bool),
      typeof(CrystalContentControl),
      new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));

    public bool TransitionsEnabled
    {
      get => (bool)GetValue(TransitionsEnabledProperty);
      set => SetValue(TransitionsEnabledProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty OnlyLoadTransitionProperty = DependencyProperty.Register(
      nameof(OnlyLoadTransition),
      typeof(bool),
      typeof(CrystalContentControl),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

    public bool OnlyLoadTransition
    {
      get => (bool)GetValue(OnlyLoadTransitionProperty);
      set => SetValue(OnlyLoadTransitionProperty, BooleanBoxes.Box(value));
    }

    public static readonly RoutedEvent TransitionStartedEvent = EventManager.RegisterRoutedEvent(
      nameof(TransitionStarted),
      RoutingStrategy.Bubble,
      typeof(RoutedEventHandler),
      typeof(CrystalContentControl));

    public event RoutedEventHandler TransitionStarted
    {
      add => AddHandler(TransitionStartedEvent, value);
      remove => RemoveHandler(TransitionStartedEvent, value);
    }

    public static readonly RoutedEvent TransitionCompletedEvent = EventManager.RegisterRoutedEvent(
      nameof(TransitionCompleted),
      RoutingStrategy.Bubble,
      typeof(RoutedEventHandler),
      typeof(CrystalContentControl));

    public event RoutedEventHandler TransitionCompleted
    {
      add => AddHandler(TransitionCompletedEvent, value);
      remove => RemoveHandler(TransitionCompletedEvent, value);
    }

    private static readonly DependencyPropertyKey IsTransitioningPropertyKey = DependencyProperty.RegisterReadOnly(
      nameof(IsTransitioning),
      typeof(bool),
      typeof(CrystalContentControl),
      new PropertyMetadata(BooleanBoxes.FalseBox));

    public static readonly DependencyProperty IsTransitioningProperty = IsTransitioningPropertyKey.DependencyProperty;

    public bool IsTransitioning
    {
      get => (bool)GetValue(IsTransitioningProperty);
      protected set => SetValue(IsTransitioningPropertyKey, BooleanBoxes.Box(value));
    }

    public CrystalContentControl()
    {
      DefaultStyleKey = typeof(CrystalContentControl);
      Loaded += CrystalContentControlLoaded;
      Unloaded += CrystalContentControlUnloaded;
    }

    private void CrystalContentControlIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (TransitionsEnabled && !transitionLoaded)
      {
        if (!IsVisible)
        {
          VisualStateManager.GoToState(this, ReverseTransition ? "AfterUnLoadedReverse" : "AfterUnLoaded", false);
        }
        else
        {
          VisualStateManager.GoToState(this, ReverseTransition ? "AfterLoadedReverse" : "AfterLoaded", true);
        }
      }
    }

    private void CrystalContentControlUnloaded(object sender, RoutedEventArgs e)
    {
      if (TransitionsEnabled)
      {
        UnsetStoryboardEvents();
        if (transitionLoaded)
        {
          VisualStateManager.GoToState(this, ReverseTransition ? "AfterUnLoadedReverse" : "AfterUnLoaded", false);
        }

        IsVisibleChanged -= CrystalContentControlIsVisibleChanged;
      }
    }

    private void CrystalContentControlLoaded(object sender, RoutedEventArgs e)
    {
      if (TransitionsEnabled)
      {
        if (!transitionLoaded)
        {
          SetStoryboardEvents();
          transitionLoaded = OnlyLoadTransition;
          VisualStateManager.GoToState(this, ReverseTransition ? "AfterLoadedReverse" : "AfterLoaded", true);
        }

        IsVisibleChanged -= CrystalContentControlIsVisibleChanged;
        IsVisibleChanged += CrystalContentControlIsVisibleChanged;
      }
      else
      {
        if (GetTemplateChild("RootGrid") is Grid rootGrid)
        {
          rootGrid.Opacity = 1.0;
          var transform = ((System.Windows.Media.TranslateTransform)rootGrid.RenderTransform);
          if (transform.IsFrozen)
          {
            var modifiedTransform = transform.Clone();
            modifiedTransform.X = 0;
            rootGrid.RenderTransform = modifiedTransform;
          }
          else
          {
            transform.X = 0;
          }
        }
      }
    }

    public void Reload()
    {
      if (!TransitionsEnabled || transitionLoaded)
      {
        return;
      }

      if (ReverseTransition)
      {
        VisualStateManager.GoToState(this, "BeforeLoaded", true);
        VisualStateManager.GoToState(this, "AfterUnLoadedReverse", true);
      }
      else
      {
        VisualStateManager.GoToState(this, "BeforeLoaded", true);
        VisualStateManager.GoToState(this, "AfterLoaded", true);
      }
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      afterLoadedStoryboard = GetTemplateChild("AfterLoadedStoryboard") as Storyboard;
      afterLoadedReverseStoryboard = GetTemplateChild("AfterLoadedReverseStoryboard") as Storyboard;
    }

    private void AfterLoadedStoryboardCurrentTimeInvalidated(object? sender, System.EventArgs e)
    {
      if (sender is Clock clock)
      {
        if (clock.CurrentState == ClockState.Active)
        {
          SetValue(IsTransitioningPropertyKey, BooleanBoxes.TrueBox);
          RaiseEvent(new RoutedEventArgs(TransitionStartedEvent));
        }
      }
    }

    private void AfterLoadedStoryboardCompleted(object? sender, System.EventArgs e)
    {
      if (transitionLoaded)
      {
        UnsetStoryboardEvents();
      }

      InvalidateVisual();
      SetValue(IsTransitioningPropertyKey, BooleanBoxes.FalseBox);
      RaiseEvent(new RoutedEventArgs(TransitionCompletedEvent));
    }

    private void SetStoryboardEvents()
    {
      if (afterLoadedStoryboard != null)
      {
        afterLoadedStoryboard.CurrentTimeInvalidated += AfterLoadedStoryboardCurrentTimeInvalidated;
        afterLoadedStoryboard.Completed += AfterLoadedStoryboardCompleted;
      }

      if (afterLoadedReverseStoryboard != null)
      {
        afterLoadedReverseStoryboard.CurrentTimeInvalidated += AfterLoadedStoryboardCurrentTimeInvalidated;
        afterLoadedReverseStoryboard.Completed += AfterLoadedStoryboardCompleted;
      }
    }

    private void UnsetStoryboardEvents()
    {
      if (afterLoadedStoryboard != null)
      {
        afterLoadedStoryboard.CurrentTimeInvalidated -= AfterLoadedStoryboardCurrentTimeInvalidated;
        afterLoadedStoryboard.Completed -= AfterLoadedStoryboardCompleted;
      }

      if (afterLoadedReverseStoryboard != null)
      {
        afterLoadedReverseStoryboard.CurrentTimeInvalidated -= AfterLoadedStoryboardCurrentTimeInvalidated;
        afterLoadedReverseStoryboard.Completed -= AfterLoadedStoryboardCompleted;
      }
    }
  }
}