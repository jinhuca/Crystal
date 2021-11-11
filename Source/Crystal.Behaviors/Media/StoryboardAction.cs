﻿using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Crystal.Behaviors
{
  /// <summary>
  /// An abstract class that provides the ability to target a Storyboard.
  /// </summary>
  /// <remarks>
  /// For action authors, this class provides a standard way to target a Storyboard. Design tools may choose to provide a 
  /// special editing experience for classes that inherit from this action, thereby improving the designer experience.
  /// </remarks>
  public abstract class StoryboardAction : TriggerAction<DependencyObject>
  {
    public static readonly DependencyProperty StoryboardProperty = DependencyProperty.Register("Storyboard", typeof(Storyboard), typeof(StoryboardAction),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnStoryboardChanged)));

    /// <summary>
    /// The targeted Storyboard. This is a dependency property.
    /// </summary>
    public Storyboard Storyboard
    {
      get { return (Storyboard)GetValue(StoryboardProperty); }
      set { SetValue(StoryboardProperty, value); }
    }

    private static void OnStoryboardChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
      StoryboardAction storyboardAction = sender as StoryboardAction;
      if (storyboardAction != null)
      {
        storyboardAction.OnStoryboardChanged(args);
      }
    }

    /// <summary>
    /// This method is called when the Storyboard property is changed.
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnStoryboardChanged(DependencyPropertyChangedEventArgs args)
    {
    }
  }

  public enum ControlStoryboardOption
  {
    Play,
    Stop,
    TogglePlayPause,
    Pause,
    Resume,
    SkipToFill
  }

  /// <summary>
  /// An action that will change the state of a targeted storyboard when invoked.
  /// </summary>
  [CLSCompliant(false)]
  public class ControlStoryboardAction : StoryboardAction
  {
    public static readonly DependencyProperty ControlStoryboardProperty = DependencyProperty.Register("ControlStoryboardOption", typeof(ControlStoryboardOption), typeof(ControlStoryboardAction));

    public ControlStoryboardAction()
    {
    }

    public ControlStoryboardOption ControlStoryboardOption
    {
      get { return (ControlStoryboardOption)GetValue(ControlStoryboardProperty); }
      set { SetValue(ControlStoryboardProperty, value); }
    }

    /// <summary>
    /// This method is called when some criteria is met and the action should be invoked. This method will attempt to 
    /// change the targeted storyboard in a way defined by the ControlStoryboardOption.
    /// </summary>
    /// <param name="parameter"></param>
    protected override void Invoke(object parameter)
    {
      if (AssociatedObject != null && Storyboard != null)
      {
        switch (ControlStoryboardOption)
        {
          case ControlStoryboardOption.Play:
            Storyboard.Begin();
            break;
          case ControlStoryboardOption.Stop:
            Storyboard.Stop();
            break;
          case ControlStoryboardOption.TogglePlayPause:
            ClockState clockState = ClockState.Stopped;
            bool isPaused = false;
            try
            {
              clockState = Storyboard.GetCurrentState();
              isPaused = Storyboard.GetIsPaused();
            }
            catch (InvalidOperationException)
            {
            }
            if (clockState == ClockState.Stopped)
            {
              Storyboard.Begin();
            }
            else if (isPaused)
            {
              Storyboard.Resume();
            }
            else
            {
              Storyboard.Pause();
            }
            break;
          case ControlStoryboardOption.Pause:
            Storyboard.Pause();
            break;
          case ControlStoryboardOption.Resume:
            Storyboard.Resume();
            break;
          case ControlStoryboardOption.SkipToFill:
            Storyboard.SkipToFill();
            break;
        }
      }
    }
  }
}
