using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Crystal.Plot2D.Common.UndoSystem;

public sealed class UndoProvider : INotifyPropertyChanged {
  internal UndoProvider() {
    UndoStack.IsEmptyChanged += OnUndoStackIsEmptyChanged;
    RedoStack.IsEmptyChanged += OnRedoStackIsEmptyChanged;
  }

  private bool IsEnabled { get; } = true;

  private void OnUndoStackIsEmptyChanged(object sender, EventArgs e) {
    PropertyChanged.Raise(sender: this, propertyName: "CanUndo");
  }

  private void OnRedoStackIsEmptyChanged(object sender, EventArgs e) {
    PropertyChanged.Raise(sender: this, propertyName: "CanRedo");
  }

  internal void AddAction(UndoAction action) {
    if(!IsEnabled) {
      return;
    }

    if(State != UndoState.None) {
      return;
    }

    UndoStack.Push(action: action);
    RedoStack.Clear();
  }

  internal void Undo() {
    var action = UndoStack.Pop();
    RedoStack.Push(action: action);

    State = UndoState.Undoing;
    try {
      action.Undo();
    }
    finally {
      State = UndoState.None;
    }
  }

  internal void Redo() {
    var action = RedoStack.Pop();
    UndoStack.Push(action: action);

    State = UndoState.Redoing;
    try {
      action.Do();
    }
    finally {
      State = UndoState.None;
    }
  }

  internal bool CanUndo => !UndoStack.IsEmpty;
  internal bool CanRedo => !RedoStack.IsEmpty;
  private UndoState State { get; set; } = UndoState.None;
  private ActionStack UndoStack { get; } = new();
  private ActionStack RedoStack { get; } = new();

  private Dictionary<CaptureKeyHolder, object> CaptureHolders { get; } = new();

  #region INotifyPropertyChanged Members

  public event PropertyChangedEventHandler PropertyChanged;

  #endregion

  internal void CaptureOldValue(DependencyObject target, DependencyProperty property, object oldValue) {
    CaptureHolders[key: new CaptureKeyHolder { Target = target, Property = property }] = oldValue;
  }

  internal void CaptureNewValue(DependencyObject target, DependencyProperty property, object newValue) {
    var holder = new CaptureKeyHolder { Target = target, Property = property };
    if(CaptureHolders.ContainsKey(key: holder)) {
      var oldValue = CaptureHolders[key: holder];
      CaptureHolders.Remove(key: holder);

      if(!object.Equals(objA: oldValue, objB: newValue)) {
        var action = new DependencyPropertyChangedUndoAction(target: target, property: property, oldValue: oldValue, newValue: newValue);
        AddAction(action: action);
      }
    }
  }

  private sealed class CaptureKeyHolder {
    public DependencyObject Target { get; init; }
    public DependencyProperty Property { get; init; }

    public override int GetHashCode() {
      return Target.GetHashCode() ^ Property.GetHashCode();
    }

    public override bool Equals(object obj) {
      if(obj == null) {
        return false;
      }

      if(obj is not CaptureKeyHolder other) {
        return false;
      }

      return Target == other.Target && Property == other.Property;
    }
  }
}

internal enum UndoState {
  None,
  Undoing,
  Redoing
}
