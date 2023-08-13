// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.



// ReSharper disable once CheckNamespace
namespace Crystal.Themes.Controls;

/// <summary>
/// Represents the base class for an icon UI element.
/// </summary>
public abstract class IconElement : Control
{
  private bool isForegroundPropertyDefaultOrInherited = true;

  protected IconElement()
  {
    // nothing here
  }

  static IconElement()
  {
    ForegroundProperty.OverrideMetadata(typeof(IconElement),
      new FrameworkPropertyMetadata(SystemColors.ControlTextBrush,
        FrameworkPropertyMetadataOptions.Inherits,
        (sender, e) => ((IconElement)sender).OnForegroundPropertyChanged(e)));
  }

  protected void OnForegroundPropertyChanged(DependencyPropertyChangedEventArgs e)
  {
    var baseValueSource = DependencyPropertyHelper.GetValueSource(this, e.Property).BaseValueSource;
    isForegroundPropertyDefaultOrInherited = baseValueSource <= BaseValueSource.Inherited;
    UpdateInheritsForegroundFromVisualParent();
  }

  protected override void OnVisualParentChanged(DependencyObject oldParent)
  {
    base.OnVisualParentChanged(oldParent);
    UpdateInheritsForegroundFromVisualParent();
  }

  private void UpdateInheritsForegroundFromVisualParent()
  {
    InheritsForegroundFromVisualParent
      = isForegroundPropertyDefaultOrInherited
        && Parent != null
        && VisualParent != null
        && Parent != VisualParent;
  }

  /// <summary>Identifies the <see cref="InheritsForegroundFromVisualParent"/> dependency property.</summary>
  internal static readonly DependencyPropertyKey InheritsForegroundFromVisualParentPropertyKey
    = DependencyProperty.RegisterReadOnly(nameof(InheritsForegroundFromVisualParent),
      typeof(bool),
      typeof(IconElement),
      new PropertyMetadata(BooleanBoxes.FalseBox, (sender, e) => ((IconElement)sender).OnInheritsForegroundFromVisualParentPropertyChanged(e)));

  /// <summary>Identifies the <see cref="InheritsForegroundFromVisualParent"/> dependency property.</summary>
  public static readonly DependencyProperty InheritsForegroundFromVisualParentProperty = InheritsForegroundFromVisualParentPropertyKey.DependencyProperty;

  /// <summary>
  /// Gets whether that this element inherits the <see cref="Control.Foreground"/> form the <see cref="Visual.VisualParent"/>.
  /// </summary>
  public bool InheritsForegroundFromVisualParent
  {
    get => (bool)GetValue(InheritsForegroundFromVisualParentProperty);
    protected set => SetValue(InheritsForegroundFromVisualParentPropertyKey, BooleanBoxes.Box(value));
  }

  protected virtual void OnInheritsForegroundFromVisualParentPropertyChanged(DependencyPropertyChangedEventArgs e)
  {
    if (e.OldValue != e.NewValue)
    {
      if (e.NewValue is true)
      {
        SetBinding(VisualParentForegroundProperty,
          new Binding
          {
            Path = new PropertyPath(TextElement.ForegroundProperty),
            Source = VisualParent
          });
      }
      else
      {
        ClearValue(VisualParentForegroundProperty);
      }
    }
  }

  private static readonly DependencyProperty VisualParentForegroundProperty
    = DependencyProperty.Register(nameof(VisualParentForeground),
      typeof(Brush),
      typeof(IconElement),
      new PropertyMetadata(default(Brush), (sender, e) => ((IconElement)sender).OnVisualParentForegroundPropertyChanged(e)));

  protected Brush? VisualParentForeground
  {
    get => (Brush?)GetValue(VisualParentForegroundProperty);
    set => SetValue(VisualParentForegroundProperty, value);
  }

  protected virtual void OnVisualParentForegroundPropertyChanged(DependencyPropertyChangedEventArgs e)
  {
  }
}