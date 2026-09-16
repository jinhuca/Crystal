using Crystal.Controls.Threading;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Service.Process;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Windows.Input;

namespace Crystal.ProcessModule.ViewModels;

/// <summary>
/// Backs the compact Processes tile on the dashboard: the live system-wide process, thread and
/// handle totals from <see cref="SystemStatsMonitor"/>. Deliberately lightweight — it does not
/// enumerate the process list; double-clicking the tile raises <c>ShowDetailEvent</c> so the shell
/// opens the full <see cref="Views.ProcessDetailView"/> in its own window.
/// </summary>
public sealed class ProcessSummaryViewModel : BindableBase, IDisposable {
  private const string Dash = "—";
  private readonly IDisposable _statsSubscription;
  private readonly UiThreadMarshaller _ui = new();

  private string _processCountLabel = Dash;
  private string _threadCountLabel = Dash;
  private string _handleCountLabel = Dash;

  public ProcessSummaryViewModel(SystemStatsMonitor stats, IEventAggregator events) {
    ArgumentNullException.ThrowIfNull(stats);
    ArgumentNullException.ThrowIfNull(events);

    ShowDetailCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Process));

    _statsSubscription = stats.Stats.Subscribe(s => _ui.Post(() => ApplyStats(s)));
  }

  public string ProcessCountLabel { get => _processCountLabel; private set => SetProperty(ref _processCountLabel, value); }
  public string ThreadCountLabel { get => _threadCountLabel; private set => SetProperty(ref _threadCountLabel, value); }
  public string HandleCountLabel { get => _handleCountLabel; private set => SetProperty(ref _handleCountLabel, value); }

  public ICommand ShowDetailCommand { get; }

  private void ApplyStats(SystemStats s) {
    ProcessCountLabel = s.Processes.ToString("N0");
    ThreadCountLabel = s.Threads.ToString("N0");
    HandleCountLabel = s.Handles.ToString("N0");
  }

  public void Dispose() => _statsSubscription.Dispose();
}
