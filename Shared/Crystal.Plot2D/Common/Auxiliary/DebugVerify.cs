using System;
using System.Diagnostics;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class DebugVerify {
  [Conditional(conditionString: "DEBUG")]
  [DebuggerStepThrough]
  public static void Is(bool condition) {
    if(!condition) {
      throw new ArgumentException(message: Strings.Exceptions.AssertionFailed);
    }
  }

  [Conditional(conditionString: "DEBUG")]
  [DebuggerStepThrough]
  public static void IsNotNaN(double d) {
    Is(condition: !double.IsNaN(d: d));
  }

  [Conditional(conditionString: "DEBUG")]
  [DebuggerStepThrough]
  public static void IsNotNaN(Vector vec) {
    IsNotNaN(d: vec.X);
    IsNotNaN(d: vec.Y);
  }

  [Conditional(conditionString: "DEBUG")]
  [DebuggerStepThrough]
  public static void IsNotNaN(Point point) {
    IsNotNaN(d: point.X);
    IsNotNaN(d: point.Y);
  }

  [Conditional(conditionString: "DEBUG")]
  [DebuggerStepThrough]
  public static void IsFinite(double d) {
    Is(condition: !double.IsInfinity(d: d) && !double.IsNaN(d: d));
  }

  [Conditional(conditionString: "DEBUG")]
  [DebuggerStepThrough]
  public static void IsNotNull(object obj) {
    Is(condition: obj != null);
  }
}
