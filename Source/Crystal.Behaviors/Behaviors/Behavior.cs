﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Animation;

namespace Crystal.Behaviors
{
	/// <summary>
	/// Encapsulates state information and zero or more ICommands into an attachable object.
	/// </summary>
	/// <typeparam name="T">The type the <see cref="Behavior&lt;T&gt;"/> can be attached to.</typeparam>
	/// <remarks>
	///		Behavior is the base class for providing attachable state and commands to an object.
	///		The types the Behavior can be attached to can be controlled by the generic parameter.
	///		Override OnAttached() and OnDetaching() methods to hook and unhook any necessary handlers
	///		from the AssociatedObject.
	///	</remarks>
	public abstract class Behavior<T> : Behavior where T : DependencyObject
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Behavior&lt;T&gt;"/> class.
		/// </summary>
		protected Behavior() : base(typeof(T))
		{
		}

		/// <summary>
		/// Gets the object to which this <see cref="Behavior&lt;T&gt;"/> is attached.
		/// </summary>
		protected new T AssociatedObject => (T)base.AssociatedObject;
	}

	/// <summary>
	/// Encapsulates state information and zero or more ICommands into an attachable object.
	/// </summary>
	/// <remarks>
	/// This is an infrastructure class. Behavior authors should derive from Behavior&lt;T&gt; instead of from this class.
	/// </remarks>
	public abstract class Behavior : Animatable, IAttachedObject
	{
		private readonly Type associatedType;
		private DependencyObject associatedObject;

		internal event EventHandler AssociatedObjectChanged;

		/// <summary>
		/// The type to which this behavior can be attached.
		/// </summary>
		protected Type AssociatedType
		{
			get
			{
				ReadPreamble();
				return associatedType;
			}
		}

		/// <summary>
		/// Gets the object to which this behavior is attached.
		/// </summary>
		protected DependencyObject AssociatedObject
		{
			get
			{
				ReadPreamble();
				return associatedObject;
			}
		}

		internal Behavior(Type associatedType)
		{
			this.associatedType = associatedType;
		}

		/// <summary>
		/// Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
		protected virtual void OnAttached()
		{
		}

		/// <summary>
		/// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
		/// </summary>
		/// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
		protected virtual void OnDetaching()
		{
		}

		protected override Freezable CreateInstanceCore()
		{
			Type classType = GetType();
			return (Freezable)Activator.CreateInstance(classType);
		}

		private void OnAssociatedObjectChanged()
		{
			if (AssociatedObjectChanged != null)
			{
				AssociatedObjectChanged(this, EventArgs.Empty);
			}
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
		/// <exception cref="InvalidOperationException">The Behavior is already hosted on a different element.</exception>
		/// <exception cref="InvalidOperationException">dependencyObject does not satisfy the Behavior type constraint.</exception>
		public void Attach(DependencyObject dependencyObject)
		{
			if (dependencyObject != AssociatedObject)
			{
				if (AssociatedObject != null)
				{
					throw new InvalidOperationException(ExceptionStringTable.CannotHostBehaviorMultipleTimesExceptionMessage);
				}

				// todo jekelly: what do we do if dependencyObject is null?

				// Ensure the type constraint is met
				if (dependencyObject != null && !AssociatedType.IsInstanceOfType(dependencyObject))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
																															ExceptionStringTable.TypeConstraintViolatedExceptionMessage,
																															GetType().Name,
																															dependencyObject.GetType().Name,
																															AssociatedType.Name));
				}

				WritePreamble();
				associatedObject = dependencyObject;
				WritePostscript();
				OnAssociatedObjectChanged();
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
			OnAssociatedObjectChanged();
		}

		#endregion
	}
}
