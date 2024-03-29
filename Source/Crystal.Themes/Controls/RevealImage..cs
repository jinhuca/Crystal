﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Crystal.Themes.Controls;

[TemplatePart(Name = "PART_Text", Type = typeof(TextBlock))]
public class RevealImage : Control
{
  private TextBlock? textBlock;

  /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
  public static readonly DependencyProperty TextProperty
    = DependencyProperty.Register(nameof(Text),
      typeof(string),
      typeof(RevealImage),
      new UIPropertyMetadata(string.Empty));

  public string Text
  {
    get => (string)GetValue(TextProperty);
    set => SetValue(TextProperty, value);
  }

  /// <summary>Identifies the <see cref="Image"/> dependency property.</summary>
  public static readonly DependencyProperty ImageProperty
    = DependencyProperty.Register(nameof(Image),
      typeof(ImageSource),
      typeof(RevealImage),
      new UIPropertyMetadata(null));

  public ImageSource? Image
  {
    get => (ImageSource?)GetValue(ImageProperty);
    set => SetValue(ImageProperty, value);
  }

  static RevealImage()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(RevealImage), new FrameworkPropertyMetadata(typeof(RevealImage)));
  }

  public override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    textBlock = GetTemplateChild("PART_Text") as TextBlock;
  }

  private static void AnimateText(string textToAnimate, TextBlock? txt, TimeSpan timeSpan)
  {
    if (txt is null)
    {
      return;
    }

    var story = new Storyboard { FillBehavior = FillBehavior.HoldEnd };
    var stringAnimationUsingKeyFrames = new StringAnimationUsingKeyFrames { Duration = new Duration(timeSpan) };

    var tmp = string.Empty;
    foreach (var c in textToAnimate)
    {
      var stringKeyFrame = new DiscreteStringKeyFrame
      {
        KeyTime = KeyTime.Paced
      };
      tmp += c;
      stringKeyFrame.Value = tmp;
      stringAnimationUsingKeyFrames.KeyFrames.Add(stringKeyFrame);
    }

    Storyboard.SetTargetName(stringAnimationUsingKeyFrames, txt.Name);
    Storyboard.SetTargetProperty(stringAnimationUsingKeyFrames, new PropertyPath(TextBlock.TextProperty));
    story.Children.Add(stringAnimationUsingKeyFrames);
    story.Begin(txt);
  }

  protected override void OnMouseEnter(MouseEventArgs e)
  {
    base.OnMouseEnter(e);

    AnimateText(Text.ToUpper(), textBlock, TimeSpan.FromSeconds(.25));
  }
}