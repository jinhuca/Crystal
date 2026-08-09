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

  // Default width for each Row #3 tile column (Memory/Storage/Network/BIOS): equal star shares.
  private static readonly GridLength ComponentColumnDefault = new(1, GridUnitType.Star);

  // Default widths for the bottom row's Processes (2*) and OS (1*) tile columns.
  private static readonly GridLength ProcessesColumnDefault = new(2, GridUnitType.Star);
  private static readonly GridLength OsColumnDefault = new(1, GridUnitType.Star);

  public DashboardView() {
    InitializeComponent();
  }

  /// <summary>Restores the resizable rows and the Row #3 tile columns to their default star
  /// proportions, undoing any splitter drags. The MinHeight/MinWidth floors defined in XAML are
  /// unaffected.</summary>
  public void ResetLayout() {
    CpuRow.Height = CpuDefault;
    GpuRow.Height = GpuDefault;
    ComponentsRow.Height = ComponentsDefault;
    ProcessesRow.Height = ProcessesDefault;

    MemoryCol.Width = ComponentColumnDefault;
    StorageCol.Width = ComponentColumnDefault;
    NetworkCol.Width = ComponentColumnDefault;
    BiosCol.Width = ComponentColumnDefault;

    ProcessesCol.Width = ProcessesColumnDefault;
    OsCol.Width = OsColumnDefault;
  }
}
