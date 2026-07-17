using System;

namespace Crystal.Plot2D.Common;

public sealed class ValueChangedEventArgs<T> : EventArgs {
  internal ValueChangedEventArgs(T prevValue, T currValue) {
    PreviousValue = prevValue;
    CurrentValue = currValue;
  }

  internal T PreviousValue { get; }

  internal T CurrentValue { get; }
}
