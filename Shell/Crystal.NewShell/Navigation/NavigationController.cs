using Crystal.Infrastructure.Constants;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.NewShell.Views;

namespace Crystal.NewShell.Navigation;

/// <summary>
/// Owns the shell's content region, which now permanently hosts the dashboard. Detail views
/// open in their own top-level windows via <see cref="DetailWindowService"/> rather than
/// swapping into this region, so the dashboard stays visible behind them.
/// </summary>
public sealed class NavigationController {
  private readonly IRegionManager _regionManager;

  public NavigationController(IRegionManager regionManager) => _regionManager = regionManager;

  /// <summary>Shows the dashboard in the shell's content region. Called once at startup.</summary>
  public void NavigateToDashboard() =>
      _regionManager.RequestNavigate(RegionNames.MainContentRegionName, nameof(DashboardView));
}
