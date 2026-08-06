using System.Windows.Controls;

namespace Crystal.Shell.Views;

/// <summary>
/// The dashboard: a grid of module summary tiles laid out per the reference design
/// (CPU top-left, GPU top-right, Memory mid-left, Storage mid-right, BIOS full-width
/// bottom). Each tile is its own region so a module can inject its summary view.
/// </summary>
public partial class DashboardView : UserControl {
  public DashboardView() {
    InitializeComponent();
  }
}
