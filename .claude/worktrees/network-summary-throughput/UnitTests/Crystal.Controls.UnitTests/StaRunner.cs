using System.Threading;

namespace Crystal.Controls.UnitTests;

/// <summary>
/// Runs a test body on a dedicated STA thread. WPF objects (DependencyObject and friends)
/// require an STA apartment, but the project's NuGet PackageSourceMapping doesn't allow the
/// usual <c>Xunit.StaFact</c> package, so we spin up the apartment ourselves and marshal any
/// exception back to the calling (test) thread so failures surface normally.
/// </summary>
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
