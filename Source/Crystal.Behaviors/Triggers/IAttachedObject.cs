using System.Windows;

namespace Crystal.Behaviors;

/// <summary>
/// An interface for an object that can be attached to another object.
/// </summary>
public interface IAttachedObject
{
  /// <summary>
  /// Gets the associated object.
  /// </summary>
  /// <value>The associated object.</value>
  /// <remarks>Represents the object the instance is attached to.</remarks>
  DependencyObject AssociatedObject
  {
    get;
  }

  /// <summary>
  /// Attaches to the specified object.
  /// </summary>
  /// <param name="dependencyObject">The object to attach to.</param>
  void Attach(DependencyObject dependencyObject);

  /// <summary>
  /// Detaches this instance from its associated object.
  /// </summary>
  void Detach();
}