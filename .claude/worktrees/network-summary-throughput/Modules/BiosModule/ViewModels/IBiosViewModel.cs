using System.Windows.Input;

namespace BiosModule.ViewModels;

/// <summary>Root view model bound to the BIOS summary tile and detail view: firmware identity
/// fields and the two navigation commands the shell wires to.</summary>
public interface IBiosViewModel {
  string Manufacturer { get; }
  string Version { get; }
  string ReleaseDate { get; }
  string SerialNumber { get; }
  string SmbiosSpecVersion { get; }
  string Status { get; }

  ICommand ShowDetailCommand { get; }
  ICommand ShowDashboardCommand { get; }
}
