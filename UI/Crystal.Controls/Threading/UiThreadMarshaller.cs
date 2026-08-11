using System;
using System.Windows.Threading;

namespace Crystal.Controls.Threading;

/// <summary>
/// Marshals work onto the UI thread for a ViewModel that binds a service stream emitting on a
/// background thread (the <c>*Monitor</c> poll streams run on <c>DefaultScheduler</c>, and the ETW
/// streams on their own thread). Capture one of these in the VM constructor — which runs on the UI
/// thread — so it binds the correct dispatcher for the life of the VM.
/// <para>
/// The naive pattern <c>Application.Current?.Dispatcher</c> read fresh on every emission is unsafe:
/// during app shutdown WPF tears <c>Application.Current</c> down to null, so a late background
/// emission falls through to running inline on the background thread and mutates a UI-thread-affined
/// <c>ObservableCollection</c>/<c>CollectionView</c> — throwing
/// <see cref="NotSupportedException"/>. This helper captures the dispatcher up front and drops work
/// once the dispatcher has begun shutting down, so late emissions during teardown are discarded.
/// </para>
/// </summary>
public sealed class UiThreadMarshaller {
  private readonly Dispatcher _dispatcher;

  /// <summary>Captures the current thread's dispatcher. Construct this on the UI thread.</summary>
  public UiThreadMarshaller() => _dispatcher = Dispatcher.CurrentDispatcher;

  /// <summary>
  /// Runs <paramref name="action"/> on the captured UI dispatcher: inline when already on that
  /// thread, otherwise posted to it. Silently drops the work if the dispatcher is shutting down,
  /// so a background stream emitting during app teardown can't mutate UI-affined collections.
  /// </summary>
  public void Post(Action action) {
    if (_dispatcher.HasShutdownStarted || _dispatcher.HasShutdownFinished) return;
    if (_dispatcher.CheckAccess()) action();
    else _dispatcher.BeginInvoke(action);
  }
}
