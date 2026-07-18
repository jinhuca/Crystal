using System;
using System.Threading;
using Xunit;

namespace Crystal.Plot2D.Tests;

/// <summary>
/// Helper class for running WPF tests on STA thread.
/// Use RunOnSTA to wrap test code that requires STA apartment.
/// </summary>
public static class STATestHelper {
  /// <summary>
  /// Runs the given action on an STA thread.
  /// </summary>
  public static void RunOnSTA(Action action) {
    // If already on STA thread, just run it
    if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) {
      action();
      return;
    }

    // Run on new STA thread
    Exception? exception = null;
    var staThread = new Thread(() => {
      try {
        action();
      }
      catch (Exception ex) {
        exception = ex;
      }
    }) {
      IsBackground = false
    };

    staThread.SetApartmentState(ApartmentState.STA);
    staThread.Start();
    staThread.Join();

    if (exception != null) {
      throw exception;
    }
  }

  /// <summary>
  /// Runs the given function on an STA thread and returns the result.
  /// </summary>
  public static T RunOnSTA<T>(Func<T> func) {
    // If already on STA thread, just run it
    if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) {
      return func();
    }

    // Run on new STA thread
    T? result = default;
    Exception? exception = null;
    var staThread = new Thread(() => {
      try {
        result = func();
      }
      catch (Exception ex) {
        exception = ex;
      }
    }) {
      IsBackground = false
    };

    staThread.SetApartmentState(ApartmentState.STA);
    staThread.Start();
    staThread.Join();

    if (exception != null) {
      throw exception;
    }

    return result!;
  }
}

/// <summary>
/// Base class for WPF tests. Automatically runs all tests on STA thread.
/// </summary>
public abstract class WPFTestBase : IDisposable {
  protected void RunTest(Action testAction) {
    STATestHelper.RunOnSTA(testAction);
  }

  protected T RunTest<T>(Func<T> testFunc) {
    return STATestHelper.RunOnSTA(testFunc);
  }

  public virtual void Dispose() {
  }
}
