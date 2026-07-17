using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Crystal.Plot2D.Common.UndoSystem;

[DebuggerDisplay(value: "Count = {Count}")]
public sealed class ActionStack {
  public void Push(UndoAction action) {
    Stack.Push(item: action);

    if(Stack.Count == 1) {
      RaiseIsEmptyChanged();
    }
  }

  public UndoAction Pop() {
    var action = Stack.Pop();

    if(Stack.Count == 0) {
      RaiseIsEmptyChanged();
    }

    return action;
  }

  public void Clear() {
    Stack.Clear();
    RaiseIsEmptyChanged();
  }

  public int Count => Stack.Count;
  public bool IsEmpty => Stack.Count == 0;
  public Stack<UndoAction> Stack { get; } = new();
  private void RaiseIsEmptyChanged() => IsEmptyChanged.Raise(sender: this);
  public event EventHandler IsEmptyChanged;
}
