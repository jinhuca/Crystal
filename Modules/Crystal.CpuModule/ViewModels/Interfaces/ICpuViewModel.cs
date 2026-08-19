using System.Windows.Input;

namespace Crystal.CpuModule.ViewModels.Interfaces;

/// <summary>
/// Root view model bound to the CPU summary tile and detail view. Composes the static
/// <see cref="SpecsViewModel"/> and the live <see cref="SensorsViewModel"/>, owns the
/// subscription to the model's streams, and exposes the two navigation commands the
/// shell wires to: open the detail view, and return to the dashboard.
/// </summary>
public interface ICpuViewModel {
  /// <summary>
  /// The static CPU information emitted once on startup, driving the summary tile and
  /// detail views.
  /// </summary>
  ICpuSpecsViewModel SpecsViewModel { get; }

  /// <summary>
  /// The live CPU readings driving the gauges (Load / Voltage / Speed / Power / Temperature)
  /// </summary>
  ICpuSensorViewModel SensorsViewModel { get; }

  /// <summary>
  /// Raises <c>ShowDetailEvent</c> so the shell swaps in the CPU detail view.
  /// </summary>
  ICommand ShowDetailCommand { get; }

  /// <summary>
  /// Raises <c>ShowDashboardEvent</c> so the shell returns to the tile dashboard.
  /// </summary>
  ICommand ShowDashboardCommand { get; }
}
