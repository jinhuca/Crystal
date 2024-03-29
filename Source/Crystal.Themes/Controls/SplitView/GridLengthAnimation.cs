﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Crystal.Themes.Controls;

/// <summary>
///     A special animation used to animates the length of a <see cref="Grid"/>.
/// </summary>
/// <seealso cref="System.Windows.Media.Animation.AnimationTimeline"/>
/// <autogeneratedoc/>
public class GridLengthAnimation : AnimationTimeline
{
  public static readonly DependencyProperty FromProperty
    = DependencyProperty.Register(nameof(From),
      typeof(GridLength),
      typeof(GridLengthAnimation));

  public GridLength From
  {
    get => (GridLength)GetValue(FromProperty);
    set => SetValue(FromProperty, value);
  }

  public static readonly DependencyProperty ToProperty
    = DependencyProperty.Register(nameof(To),
      typeof(GridLength),
      typeof(GridLengthAnimation));

  public GridLength To
  {
    get => (GridLength)GetValue(ToProperty);
    set => SetValue(ToProperty, value);
  }

  public override Type TargetPropertyType => typeof(GridLength);

  public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
  {
    var from = (GridLength)GetValue(FromProperty);
    var to = (GridLength)GetValue(ToProperty);
    if (from.GridUnitType != to.GridUnitType) // We can't animate different types, so just skip straight to it
    {
      return to;
    }

    var fromVal = from.Value;
    var toVal = to.Value;

    if (fromVal > toVal)
    {
      return new GridLength((1 - animationClock.CurrentProgress.GetValueOrDefault()) * (fromVal - toVal) + toVal, GridUnitType.Star);
    }

    return new GridLength(animationClock.CurrentProgress.GetValueOrDefault() * (toVal - fromVal) + fromVal, GridUnitType.Star);
  }

  protected override Freezable CreateInstanceCore()
  {
    return new GridLengthAnimation();
  }
}