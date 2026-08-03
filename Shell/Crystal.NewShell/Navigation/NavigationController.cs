using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.NewShell.Views;

namespace Crystal.NewShell.Navigation;

/// <summary>
/// Owns the shell's single swappable content region. Starts on the dashboard, then
/// listens for <see cref="ShowDetailEvent"/> (navigate to a module's full-scale detail
/// view) and <see cref="ShowDashboardEvent"/> (return to the tile dashboard).
/// <para>
/// A long-lived singleton: it must outlive the events it subscribes to, and Prism's
/// event aggregator holds only weak references, so a transient would be collected and
/// stop responding.
/// </para>
/// </summary>
public sealed class NavigationController {
  private readonly IRegionManager _regionManager;

  public NavigationController(IRegionManager regionManager, IEventAggregator events) {
    _regionManager = regionManager;
    events.GetEvent<ShowDetailEvent>().Subscribe(NavigateToDetail);
    events.GetEvent<ShowDashboardEvent>().Subscribe(NavigateToDashboard);
  }

  /// <summary>Shows the dashboard. Called once at startup and on every "Back".</summary>
  public void NavigateToDashboard() =>
      _regionManager.RequestNavigate(RegionNames.MainContentRegionName, nameof(DashboardView));

  private void NavigateToDetail(string detailViewName) =>
      _regionManager.RequestNavigate(RegionNames.MainContentRegionName, detailViewName);
}
