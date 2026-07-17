using Crystal.Plot2D.Common;
using System;
using System.Collections.Specialized;

namespace Crystal.Plot2D.ViewportConstraints;

/// <summary>
///   Represents a collection of <see cref="ViewportConstraint"/>s.
/// </summary>
/// <remarks>
///   ViewportConstraint that is being added should not be null.
/// </remarks>
public sealed class ConstraintCollection : NotifiableCollection<ViewportConstraint> {
  public Viewport2D Viewport { get; }

  internal ConstraintCollection(Viewport2D viewport) {
    Viewport = viewport ?? throw new ArgumentNullException(paramName: nameof(viewport));
  }

  protected override void OnItemAdding(ViewportConstraint item) {
    ArgumentNullException.ThrowIfNull(item);
  }

  protected override void OnItemAdded(ViewportConstraint item) {
    item.Changed += OnItemChanged;
    if(item is ISupportAttachToViewport attachable_) {
      attachable_.Attach(viewport: Viewport);
    }
  }

  private void OnItemChanged(object sender, EventArgs e) {
    OnCollectionChanged(e: new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset));
  }

  protected override void OnItemRemoving(ViewportConstraint item) {
    if(item is ISupportAttachToViewport attachable_) {
      attachable_.Detach(viewport: Viewport);
    }
    item.Changed -= OnItemChanged;
  }

  internal DataRect Apply(DataRect oldVisible, DataRect newVisible, Viewport2D viewport) {
    var res_ = newVisible;
    foreach(var constraint_ in this) {
      res_ = constraint_.Apply(previousDataRect: oldVisible, proposedDataRect: res_, viewport: viewport);
    }
    return res_;
  }
}
