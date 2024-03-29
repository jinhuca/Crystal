﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Crystal.Behaviors;

/// <summary>
/// Defines a transition effect shader that transitions from one visual to another visual
/// using an interpolated value between 0 and 1.
/// </summary>
public abstract class TransitionEffect : ShaderEffect
{
  #region Dependency Properties
  /// <summary>
  /// Brush-valued properties that turn into sampler-properties in the shader.
  /// Represents the image present in the final state of the transition.
  /// </summary>
  public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(TransitionEffect), 0, SamplingMode.NearestNeighbor);

  /// <summary>
  /// Brush-valued properties that turn into sampler-properties in the shader.
  /// Represents the image present in the initial state of the transition.
  /// </summary>
  public static readonly DependencyProperty OldImageProperty = RegisterPixelShaderSamplerProperty("OldImage", typeof(TransitionEffect), 1, SamplingMode.NearestNeighbor);

  /// <summary>
  /// A Dependency property as the backing store for Progress.
  /// Also used to represent the state of a transition from start to finish (range between 0 and 1).
  /// </summary>
  public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(TransitionEffect), new PropertyMetadata(0.0, PixelShaderConstantCallback(0)));

  #endregion

  #region Constructors

  /// <summary>
  /// Creates a modifiable clone (deep copy) of the <see cref="T:TransitionEffect"/> using its current values.
  /// </summary>
  public new TransitionEffect CloneCurrentValue()
  {
    return (TransitionEffect)base.CloneCurrentValue();
  }

  /// <summary>
  /// Makes a deep copy of the transition effect. Implements CloneCurrentValue in Silverlight.
  /// </summary>
  /// <returns>A clone of current instance of transition effect.</returns>
  protected abstract TransitionEffect DeepCopy();

  /// <summary>
  /// Updates the shader's variables to the default values.
  /// </summary>
  protected TransitionEffect()
  {
    // Update each DependencyProperty that's registered with a shader register.  This
    // is needed to ensure the shader gets sent the proper default value.
    UpdateShaderValue(InputProperty);
    UpdateShaderValue(OldImageProperty);
    UpdateShaderValue(ProgressProperty);
  }

  #endregion

  /// <summary>
  /// Gets or sets the Input variable within the shader.
  /// </summary>
  public Brush Input
  {
    get => (Brush)GetValue(InputProperty);
    set => SetValue(InputProperty, value);
  }

  /// <summary>
  /// Gets or sets the OldImage variable within the shader.
  /// </summary>
  public Brush OldImage
  {
    get => (Brush)GetValue(OldImageProperty);
    set => SetValue(OldImageProperty, value);
  }

  /// <summary>
  /// Gets or sets the Progress variable within the shader.
  /// </summary>
  public double Progress
  {
    get => (double)GetValue(ProgressProperty);
    set => SetValue(ProgressProperty, value);
  }
}