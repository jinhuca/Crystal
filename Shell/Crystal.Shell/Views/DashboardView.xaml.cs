using System.Windows;
using System.Windows.Controls;

namespace Crystal.Shell.Views;

/// <summary>
/// The dashboard: a grid of module summary tiles laid out per the reference design
/// (CPU top-left, GPU top-right, Memory mid-left, Storage mid-right, BIOS full-width
/// bottom). Each tile is its own region so a module can inject its summary view.
/// The content rows are user-resizable via GridSplitters; <see cref="ResetLayout"/>
/// restores their default star proportions.
/// </summary>
public partial class DashboardView : UserControl {
  // Default row heights, kept in one place so ResetLayout and the XAML stay in sync.
  private static readonly GridLength CpuDefault = new(1, GridUnitType.Star);
  private static readonly GridLength GpuDefault = new(1, GridUnitType.Star);
  private static readonly GridLength ComponentsDefault = new(1.2, GridUnitType.Star);
  private static readonly GridLength ProcessesDefault = new(1.6, GridUnitType.Star);

  public DashboardView() {
    InitializeComponent();
  }

  /// <summary>Restores the resizable rows to their default star proportions, undoing any
  /// splitter drags. The MinHeight floors defined in XAML are unaffected.</summary>
  public void ResetLayout() {
    CpuRow.Height = CpuDefault;
    GpuRow.Height = GpuDefault;
    ComponentsRow.Height = ComponentsDefault;
    ProcessesRow.Height = ProcessesDefault;
  }
}
