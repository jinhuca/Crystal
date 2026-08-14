using System.Threading;

namespace Crystal.ProcessModule.Tests;

// Runs a test body on a dedicated STA thread. The process-list view model builds a WPF
// ListCollectionView with live shaping; exercising it wants an STA apartment, so we spin one up
// ourselves (the project can't use Xunit.StaFact) and marshal any exception back to the caller.
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
