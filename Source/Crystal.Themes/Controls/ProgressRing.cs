// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Crystal.Themes.Controls;

[TemplateVisualState(Name = "Large", GroupName = "SizeStates")]
[TemplateVisualState(Name = "Small", GroupName = "SizeStates")]
[TemplateVisualState(Name = "Inactive", GroupName = "ActiveStates")]
[TemplateVisualState(Name = "Active", GroupName = "ActiveStates")]
public class ProgressRing : Control
{
  /// <summary>Identifies the <see cref="BindableWidth"/> dependency property.</summary>
  private static readonly DependencyPropertyKey BindableWidthPropertyKey
    = DependencyProperty.RegisterReadOnly(nameof(BindableWidth),
      typeof(double),
      typeof(ProgressRing),
      new PropertyMetadata(default(double), OnBindableWidthPropertyChanged));

  /// <summary>Identifies the <see cref="BindableWidth"/> dependency property.</summary>
  public static readonly DependencyProperty BindableWidthProperty = BindableWidthPropertyKey.DependencyProperty;

  public double BindableWidth
  {
    get => (double)GetValue(BindableWidthProperty);
    protected set => SetValue(BindableWidthPropertyKey, value);
  }

  /// <summary>Identifies the <see cref="IsActive"/> dependency property.</summary>
  public static readonly DependencyProperty IsActiveProperty
    = DependencyProperty.Register(nameof(IsActive),
      typeof(bool),
      typeof(ProgressRing),
      new PropertyMetadata(BooleanBoxes.TrueBox, OnIsActivePropertyChanged));

  public bool IsActive
  {
    get => (bool)GetValue(IsActiveProperty);
    set => SetValue(IsActiveProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="IsLarge"/> dependency property.</summary>
  public static readonly DependencyProperty IsLargeProperty
    = DependencyProperty.Register(nameof(IsLarge),
      typeof(bool),
      typeof(ProgressRing),
      new PropertyMetadata(BooleanBoxes.TrueBox, OnIsLargePropertyChanged));

  public bool IsLarge
  {
    get => (bool)GetValue(IsLargeProperty);
    set => SetValue(IsLargeProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="MaxSideLength"/> dependency property.</summary>
  private static readonly DependencyPropertyKey MaxSideLengthPropertyKey
    = DependencyProperty.RegisterReadOnly(nameof(MaxSideLength),
      typeof(double),
      typeof(ProgressRing),
      new PropertyMetadata(default(double)));

  /// <summary>Identifies the <see cref="MaxSideLength"/> dependency property.</summary>
  public static readonly DependencyProperty MaxSideLengthProperty = MaxSideLengthPropertyKey.DependencyProperty;

  public double MaxSideLength
  {
    get => (double)GetValue(MaxSideLengthProperty);
    protected set => SetValue(MaxSideLengthPropertyKey, value);
  }

  /// <summary>Identifies the <see cref="EllipseDiameter"/> dependency property.</summary>
  private static readonly DependencyPropertyKey EllipseDiameterPropertyKey
    = DependencyProperty.RegisterReadOnly(nameof(EllipseDiameter),
      typeof(double),
      typeof(ProgressRing),
      new PropertyMetadata(default(double)));

  /// <summary>Identifies the <see cref="EllipseDiameter"/> dependency property.</summary>
  public static readonly DependencyProperty EllipseDiameterProperty = EllipseDiameterPropertyKey.DependencyProperty;

  public double EllipseDiameter
  {
    get => (double)GetValue(EllipseDiameterProperty);
    protected set => SetValue(EllipseDiameterPropertyKey, value);
  }

  /// <summary>Identifies the <see cref="EllipseOffset"/> dependency property.</summary>
  private static readonly DependencyPropertyKey EllipseOffsetPropertyKey
    = DependencyProperty.RegisterReadOnly(nameof(EllipseOffset),
      typeof(Thickness),
      typeof(ProgressRing),
      new PropertyMetadata(default(Thickness)));

  /// <summary>Identifies the <see cref="EllipseOffset"/> dependency property.</summary>
  public static readonly DependencyProperty EllipseOffsetProperty = EllipseOffsetPropertyKey.DependencyProperty;

  public Thickness EllipseOffset
  {
    get => (Thickness)GetValue(EllipseOffsetProperty);
    protected set => SetValue(EllipseOffsetPropertyKey, value);
  }

  /// <summary>Identifies the <see cref="EllipseDiameterScale"/> dependency property.</summary>
  public static readonly DependencyProperty EllipseDiameterScaleProperty
    = DependencyProperty.Register(nameof(EllipseDiameterScale),
      typeof(double),
      typeof(ProgressRing),
      new PropertyMetadata(1D));

  public double EllipseDiameterScale
  {
    get => (double)GetValue(EllipseDiameterScaleProperty);
    set => SetValue(EllipseDiameterScaleProperty, value);
  }

  private List<Action>? deferredActions = new List<Action>();

  static ProgressRing()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
    VisibilityProperty.OverrideMetadata(
      typeof(ProgressRing),
      new FrameworkPropertyMetadata(
        (ringObject, e) =>
        {
          if (e.NewValue != e.OldValue)
          {
            var ring = ringObject as ProgressRing;

            ring?.SetCurrentValue(IsActiveProperty, BooleanBoxes.Box((Visibility)e.NewValue == Visibility.Visible));
          }
        }));
  }

  public ProgressRing()
  {
    SizeChanged += OnSizeChanged;
  }

  private static void OnBindableWidthPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
  {
    if (!(dependencyObject is ProgressRing ring))
    {
      return;
    }

    var action = new Action(
      () =>
      {
        ring.SetEllipseDiameter((double)dependencyPropertyChangedEventArgs.NewValue);
        ring.SetEllipseOffset((double)dependencyPropertyChangedEventArgs.NewValue);
        ring.SetMaxSideLength((double)dependencyPropertyChangedEventArgs.NewValue);
      });

    if (ring.deferredActions != null)
    {
      ring.deferredActions.Add(action);
    }
    else
    {
      action();
    }
  }

  private void SetMaxSideLength(double width)
  {
    SetValue(MaxSideLengthPropertyKey, width <= 20d ? 20d : width);
  }

  private void SetEllipseDiameter(double width)
  {
    SetValue(EllipseDiameterPropertyKey, (width / 8) * EllipseDiameterScale);
  }

  private void SetEllipseOffset(double width)
  {
    SetValue(EllipseOffsetPropertyKey, new Thickness(0, width / 2, 0, 0));
  }

  private static void OnIsLargePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
  {
    var ring = dependencyObject as ProgressRing;

    ring?.UpdateLargeState();
  }

  private void UpdateLargeState()
  {
    Action action;

    if (IsLarge)
    {
      action = () => VisualStateManager.GoToState(this, "Large", true);
    }
    else
    {
      action = () => VisualStateManager.GoToState(this, "Small", true);
    }

    if (deferredActions != null)
    {
      deferredActions.Add(action);
    }
    else
    {
      action();
    }
  }

  private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
  {
    SetValue(BindableWidthPropertyKey, ActualWidth);
  }

  private static void OnIsActivePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
  {
    var ring = dependencyObject as ProgressRing;

    ring?.UpdateActiveState();
  }

  private void UpdateActiveState()
  {
    Action action;

    if (IsActive)
    {
      action = () => VisualStateManager.GoToState(this, "Active", true);
    }
    else
    {
      action = () => VisualStateManager.GoToState(this, "Inactive", true);
    }

    if (deferredActions != null)
    {
      deferredActions.Add(action);
    }
    else
    {
      action();
    }
  }

  public override void OnApplyTemplate()
  {
    // make sure the states get updated
    UpdateLargeState();
    UpdateActiveState();
    base.OnApplyTemplate();
    if (deferredActions != null)
    {
      foreach (var action in deferredActions)
      {
        action();
      }
    }

    deferredActions = null;
  }
}