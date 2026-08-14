using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Crystal.GpuModule.ViewModels;

/// <summary>
/// Root view model bound to the GPU summary tile and detail view. Exposes one
/// <see cref="GpuAdapterViewModel"/> per detected adapter (integrated / dedicated columns of
/// the reference design) and the two navigation commands the shell wires to.
/// </summary>
public interface IGpuViewModel {
  ObservableCollection<GpuAdapterViewModel> Adapters { get; }

  /// <summary>Raises <c>ShowDetailEvent</c> so the shell swaps in the GPU detail view.</summary>
  ICommand ShowDetailCommand { get; }

  /// <summary>Raises <c>ShowDashboardEvent</c> so the shell returns to the tile dashboard.</summary>
  ICommand ShowDashboardCommand { get; }
}
