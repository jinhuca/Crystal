﻿using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Windows.Controls.Primitives;

namespace Crystal.Behaviors;

/// <summary>
/// Represents an attachable object that encapsulates a unit of functionality.
/// </summary>
/// <typeparam name="T">The type to which this action can be attached.</typeparam>
public abstract class TriggerAction<T> : TriggerAction where T : DependencyObject
{
  /// <summary>
  /// Initializes a new instance of the <see cref="TriggerAction&lt;T&gt;"/> class.
  /// </summary>
  protected TriggerAction() : base(typeof(T))
  {
  }

  /// <summary>
  /// Gets the object to which this <see cref="TriggerAction&lt;T&gt;"/> is attached.
  /// </summary>
  /// <value>The associated object.</value>
  protected new T AssociatedObject => (T)base.AssociatedObject;

  /// <summary>
  /// Gets the associated object type constraint.
  /// </summary>
  /// <value>The associated object type constraint.</value>
  protected sealed override Type AssociatedObjectTypeConstraint => base.AssociatedObjectTypeConstraint;
}

/// <summary>
/// Represents an attachable object that encapsulates a unit of functionality.
/// </summary>
/// <remarks>This is an infrastructure class. Action authors should derive from TriggerAction&lt;T&gt; instead of this class.</remarks>
[DefaultTrigger(typeof(UIElement), typeof(EventTrigger), "MouseLeftButtonDown")]
[DefaultTrigger(typeof(ButtonBase), typeof(EventTrigger), "Click")]
public abstract class TriggerAction : Animatable, IAttachedObject
{
  private bool isHosted;
  private DependencyObject associatedObject;
  private Type associatedObjectTypeConstraint;

  public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled",
    typeof(bool),
    typeof(TriggerAction),
    new FrameworkPropertyMetadata(true));

  /// <summary>
  /// Gets or sets a value indicating whether this action will run when invoked. This is a dependency property.
  /// </summary>
  /// <value>
  /// 	<c>True</c> if this action will be run when invoked; otherwise, <c>False</c>.
  /// </value>
  public bool IsEnabled
  {
    get => (bool)GetValue(IsEnabledProperty);
    set => SetValue(IsEnabledProperty, value);
  }

  /// <summary>
  /// Gets the object to which this action is attached.
  /// </summary>
  /// <value>The associated object.</value>
  protected DependencyObject AssociatedObject
  {
    get
    {
      ReadPreamble();
      return associatedObject;
    }
  }

  /// <summary>
  /// Gets the associated object type constraint.
  /// </summary>
  /// <value>The associated object type constraint.</value>
  protected virtual Type AssociatedObjectTypeConstraint
  {
    get
    {
      ReadPreamble();
      return associatedObjectTypeConstraint;
    }
  }

  /// <summary>
  /// Gets or sets a value indicating whether this instance is attached.
  /// </summary>
  /// <value><c>True</c> if this instance is attached; otherwise, <c>False</c>.</value>
  internal bool IsHosted
  {
    get
    {
      ReadPreamble();
      return isHosted;
    }
    set
    {
      WritePreamble();
      isHosted = value;
      WritePostscript();
    }
  }

  internal TriggerAction(Type associatedObjectTypeConstraint)
  {
    this.associatedObjectTypeConstraint = associatedObjectTypeConstraint;
  }

  /// <summary>
  /// Attempts to invoke the action.
  /// </summary>
  /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
  internal void CallInvoke(object parameter)
  {
    if (IsEnabled)
    {
      Invoke(parameter);
    }
  }

  /// <summary>
  /// Invokes the action.
  /// </summary>
  /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
  protected abstract void Invoke(object parameter);

  /// <summary>
  /// Called after the action is attached to an AssociatedObject.
  /// </summary>
  protected virtual void OnAttached()
  {
  }

  /// <summary>
  /// Called when the action is being detached from its AssociatedObject, but before it has actually occurred.
  /// </summary>
  protected virtual void OnDetaching()
  {
  }

  /// <summary>
  /// When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Freezable"/> derived class.
  /// </summary>
  /// <returns>The new instance.</returns>
  protected override Freezable CreateInstanceCore()
  {
    Type classType = GetType();
    return (Freezable)Activator.CreateInstance(classType);
  }

  #region IAttachedObject Members

  /// <summary>
  /// Gets the associated object.
  /// </summary>
  /// <value>The associated object.</value>
  DependencyObject IAttachedObject.AssociatedObject => AssociatedObject;

  /// <summary>
  /// Attaches to the specified object.
  /// </summary>
  /// <param name="dependencyObject">The object to attach to.</param>
  /// <exception cref="InvalidOperationException">Cannot host the same TriggerAction on more than one object at a time.</exception>
  /// <exception cref="InvalidOperationException">dependencyObject does not satisfy the TriggerAction type constraint.</exception>
  public void Attach(DependencyObject dependencyObject)
  {
    if (dependencyObject != AssociatedObject)
    {
      if (AssociatedObject != null)
      {
        throw new InvalidOperationException(ExceptionStringTable.CannotHostTriggerActionMultipleTimesExceptionMessage);
      }

      // Ensure the type constraint is met
      if (dependencyObject != null && !AssociatedObjectTypeConstraint.IsAssignableFrom(dependencyObject.GetType()))
      {
        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
          ExceptionStringTable.TypeConstraintViolatedExceptionMessage,
          GetType().Name,
          dependencyObject.GetType().Name,
          AssociatedObjectTypeConstraint.Name));
      }

      WritePreamble();
      associatedObject = dependencyObject;
      WritePostscript();

      OnAttached();
    }
  }

  /// <summary>
  /// Detaches this instance from its associated object.
  /// </summary>
  public void Detach()
  {
    OnDetaching();
    WritePreamble();
    associatedObject = null;
    WritePostscript();
  }

  #endregion
}