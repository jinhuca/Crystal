using System;
using System.Diagnostics;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class Verify {
  [DebuggerStepThrough]
  public static void IsTrue(this bool condition) {
    if(!condition) {
      throw new ArgumentException(message: Strings.Exceptions.AssertionFailedSearch);
    }
  }

  [DebuggerStepThrough]
  public static void IsTrue(this bool condition, string paramName) {
    if(!condition) {
      throw new ArgumentException(message: Strings.Exceptions.AssertionFailedSearch, paramName: paramName);
    }
  }

  public static void IsTrueWithMessage(this bool condition, string message) {
    if(!condition) {
      throw new ArgumentException(message: message);
    }
  }

  [DebuggerStepThrough]
  public static void AssertNotNull(object obj) {
    IsTrue(condition: obj != null);
  }

  public static void VerifyNotNull(this object obj, string paramName) {
    if(obj == null) {
      throw new ArgumentNullException(paramName: paramName);
    }
  }

  public static void VerifyNotNull(this object obj) {
    VerifyNotNull(obj: obj, paramName: "value");
  }

  [DebuggerStepThrough]
  public static void AssertIsNotNaN(this double d) {
    IsTrue(condition: !double.IsNaN(d: d));
  }

  [DebuggerStepThrough]
  public static void AssertIsFinite(this double d) {
    IsTrue(condition: !double.IsInfinity(d: d) && !double.IsNaN(d: d));
  }
}
