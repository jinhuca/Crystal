using System.Windows.Controls;

namespace Crystal.OSModule.Views;

/// <summary>Full-scale OS view: headline version/build/architecture/uptime tiles above a two-column
/// identity grid (edition, install/boot provenance, machine/user identity, locale/time zone).
/// Reached by selecting the OS summary tile; the Back control returns to the dashboard.</summary>
public partial class OsDetailView : UserControl {
  public OsDetailView() {
    InitializeComponent();
  }
}
