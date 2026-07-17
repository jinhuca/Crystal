using System;

namespace Crystal.Plot2D.Common.UndoSystem;

public sealed class LambdaUndoAction : UndoAction {
  public LambdaUndoAction(Action doAction, Action undoAction) {
    DoAction = doAction ?? throw new ArgumentNullException(paramName: nameof(doAction));
    UndoAction = undoAction ?? throw new ArgumentNullException(paramName: nameof(undoAction));
  }

  private Action DoAction { get; }

  private Action UndoAction { get; }

  public override void Do() => DoAction();

  public override void Undo() => UndoAction();
}
