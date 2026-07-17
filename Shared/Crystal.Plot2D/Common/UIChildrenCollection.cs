using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Plot2D.Common;

/// <summary>
/// Collection of UI children of <see cref="IndependentArrangePanel"/>.
/// </summary>
internal sealed class UIChildrenCollection : UIElementCollection {
  private readonly IndividualArrangePanel hostPanel;

  /// <summary>
  /// Initializes a new instance of the <see cref="UIChildrenCollection"/> class.
  /// </summary>
  internal UIChildrenCollection(UIElement visualParent, FrameworkElement logicalParent)
    : base(visualParent: visualParent, logicalParent: logicalParent) {
    hostPanel = (IndividualArrangePanel)visualParent;
    visualChildren = new VisualCollection(parent: visualParent);
  }

  private readonly VisualCollection visualChildren;
  public bool IsAddingMany { get; set; }

  public override int Add(UIElement element) {
    ArgumentNullException.ThrowIfNull(element);

    SetLogicalParent(element: element);

    var index = visualChildren.Add(visual: element);

    if(IsAddingMany) {
      hostPanel.InvalidateMeasure();
    }
    else {
      hostPanel.OnChildAdded(child: (FrameworkElement)element);
    }

    return index;
  }

  public override void Clear() {
    if(visualChildren.Count > 0) {
      var visualArray = new Visual[visualChildren.Count];
      for(var i = 0; i < visualChildren.Count; i++) {
        visualArray[i] = visualChildren[index: i];
      }

      visualChildren.Clear();

      for(var i = 0; i < visualArray.Length; i++) {
        if(visualArray[i] is UIElement element) {
          ClearLogicalParent(element: element);
        }
      }
    }
  }

  public override bool Contains(UIElement element) => visualChildren.Contains(visual: element);

  public override void CopyTo(Array array, int index) => visualChildren.CopyTo(array: array, index: index);

  public override void CopyTo(UIElement[] array, int index) => visualChildren.CopyTo(array: array, index: index);

  public override IEnumerator GetEnumerator() => visualChildren.GetEnumerator();

  public override int IndexOf(UIElement element) => visualChildren.IndexOf(visual: element);

  public override void Insert(int index, UIElement element) {
    ArgumentNullException.ThrowIfNull(element);

    hostPanel.OnChildAdded(child: (FrameworkElement)element);
    SetLogicalParent(element: element);
    visualChildren.Insert(index: index, visual: element);
  }

  public override void Remove(UIElement element) {
    visualChildren.Remove(visual: element);
    ClearLogicalParent(element: element);
  }

  public override void RemoveAt(int index) {
    visualChildren.RemoveAt(index: index);
    if(visualChildren[index: index] is UIElement element) {
      ClearLogicalParent(element: element);
    }
  }

  public override void RemoveRange(int index, int count) {
    var actualCount = visualChildren.Count;
    if(count > actualCount - index) {
      count = actualCount - index;
    }

    if(count > 0) {
      var visualArray = new Visual[count];
      var copyIndex = index;
      for(var i = 0; i < count; i++) {
        visualArray[i] = visualChildren[index: copyIndex];
        copyIndex++;
      }

      visualChildren.RemoveRange(index: index, count: count);

      for(var i = 0; i < count; i++) {
        if(visualArray[i] is UIElement element) {
          ClearLogicalParent(element: element);
        }
      }
    }
  }

  public override UIElement this[int index] {
    get => visualChildren[index: index] as UIElement;
    set {
      ArgumentNullException.ThrowIfNull(value);

      if(visualChildren[index: index] != value) {
        if(visualChildren[index: index] is UIElement element) {
          ClearLogicalParent(element: element);
        }

        hostPanel.OnChildAdded(child: (FrameworkElement)value);
        visualChildren[index: index] = value;
        SetLogicalParent(element: value);
      }
    }
  }

  public override int Capacity {
    get => visualChildren.Capacity;
    set => visualChildren.Capacity = value;
  }

  public override int Count => visualChildren.Count;

  public override bool IsSynchronized => visualChildren.IsSynchronized;

  public override object SyncRoot => visualChildren.SyncRoot;
}
