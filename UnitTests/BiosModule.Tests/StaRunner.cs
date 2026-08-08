using System.Threading;

namespace BiosModule.Tests;

// Runs a test body on a dedicated STA thread. Constructing a WPF FrameworkElement (e.g.
// PerformanceGraph) requires an STA apartment; the project can't use Xunit.StaFact, so we spin the
// apartment up ourselves and marshal any exception back to the calling thread.
internal static class StaRunner {
  public static void Run(Action body) {
    Exception? captured = null;
    var thread = new Thread(() => {
      try {
        body();
      } catch (Exception ex) {
        captured = ex;
      }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (captured != null) {
      System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(captured).Throw();
    }
  }
}
