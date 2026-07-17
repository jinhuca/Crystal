using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Plot2D.Common;

/// <summary>
///   Represents a custom Panel, which performs Arrange of its children independently, and does not remeasure or rearrange itself 
///   or all children when one child is added or removed.
///   
///   Is intended to be a base class for special layout panels, in which each child is arranged independently from each other child,
///   e.g. panel with child's position viewport bound to a rectangle in viewport coordinates.
/// </summary>
public abstract class IndividualArrangePanel : Panel {
  /// <summary>
  ///   Initializes a new instance of the <see cref="IndependentArrangePanel"/> class.
  /// </summary>
  protected IndividualArrangePanel() { }

  /// <summary>
  ///   Creates a new <see cref="T:System.Windows.Controls.UIElementCollection"/>.
  /// </summary>
  /// <param name="logicalParent">
  ///   The logical parent element of the collection to be created.
  /// </param>
  /// <returns>
  ///   An ordered collection of elements that have the specified logical parent.
  /// </returns>
  protected sealed override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
    => new UIChildrenCollection(visualParent: this, logicalParent: logicalParent);

  internal bool InBatchAdd => Children1.IsAddingMany;

  internal virtual void BeginBatchAdd() => Children1.IsAddingMany = true;

  internal virtual void EndBatchAdd() => Children1.IsAddingMany = false;

  /// <summary>
  ///   Called when child is added.
  /// </summary>
  /// <param name="child">
  ///   The added child.
  /// </param>
  protected internal virtual void OnChildAdded(FrameworkElement child) {
  }

  #region Overrides

  /// <inheritdoc />
  /// <summary>
  ///   Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)" />, 
  ///   and returns a child at the specified index from a collection of child elements.
  /// </summary>
  /// <param name="index">
  ///   The zero-based index of the requested child element in the collection.
  /// </param>
  /// <returns>
  ///   The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
  /// </returns>
  protected sealed override Visual GetVisualChild(int index) => Children[index: index];

  /// <inheritdoc />
  /// <summary>
  ///   Gets the number of visual child elements within this element.
  /// </summary>
  /// <value></value>
  /// <returns>
  ///   The number of visual child elements for this element.
  /// </returns>
  protected sealed override int VisualChildrenCount => Children.Count;

  /// <inheritdoc />
  /// <summary>
  ///   Gets an enumerator for logical child elements of this element.
  /// </summary>
  /// <value></value>
  /// <returns>
  ///   An enumerator for logical child elements of this element.
  /// </returns>
  protected sealed override IEnumerator LogicalChildren => Children.GetEnumerator();

  #endregion

  internal Vector InternalVisualOffset {
    get => VisualOffset;
    set => VisualOffset = value;
  }

  internal UIChildrenCollection Children1 { get; }
}