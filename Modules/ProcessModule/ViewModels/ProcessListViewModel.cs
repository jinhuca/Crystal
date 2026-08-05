using System.Collections.ObjectModel;
using System.Windows;
using ProcessModule.Models;

namespace ProcessModule.ViewModels;

/// <summary>
/// Backs the Processes tile: subscribes to the live sample stream and reconciles it into a
/// stable, PID-keyed row collection (add new, update existing in place, drop exited). Rows are
/// kept ordered by CPU% descending so the busiest process stays at the top, like Task Manager.
/// </summary>
public sealed class ProcessListViewModel : BindableBase, IDisposable {
  private readonly IDisposable _subscription;
  private readonly Dictionary<uint, ProcessRowViewModel> _rowsByPid = new();

  public ProcessListViewModel(IProcessModel model) {
    _subscription = model.Processes.Subscribe(samples => OnUi(() => Apply(samples)));
  }

  public ObservableCollection<ProcessRowViewModel> Rows { get; } = [];

  private void Apply(IReadOnlyList<ProcessSample> samples) {
    var live = new HashSet<uint>(samples.Count);

    foreach (var s in samples) {
      live.Add(s.ProcessId);
      if (_rowsByPid.TryGetValue(s.ProcessId, out var row)) {
        row.Update(s);
      } else {
        var created = new ProcessRowViewModel(s.ProcessId, s.Name);
        created.Update(s);
        _rowsByPid[s.ProcessId] = created;
        Rows.Add(created);
      }
    }

    // Drop rows for processes that are gone.
    for (int i = Rows.Count - 1; i >= 0; i--) {
      if (!live.Contains(Rows[i].ProcessId)) {
        _rowsByPid.Remove(Rows[i].ProcessId);
        Rows.RemoveAt(i);
      }
    }

    SortByCpuDescending();
  }

  // Insertion-sort the existing collection in place: moving items keeps the same row instances
  // (so per-row bindings/selection persist) and the list is nearly sorted between polls, so this
  // is cheap in practice.
  private void SortByCpuDescending() {
    for (int i = 1; i < Rows.Count; i++) {
      int j = i;
      while (j > 0 && Rows[j - 1].CpuPercent < Rows[j].CpuPercent) {
        Rows.Move(j, j - 1);
        j--;
      }
    }
  }

  private static void OnUi(Action action) {
    var dispatcher = Application.Current?.Dispatcher;
    if (dispatcher is null || dispatcher.CheckAccess()) action();
    else dispatcher.Invoke(action);
  }

  public void Dispose() => _subscription.Dispose();
}
