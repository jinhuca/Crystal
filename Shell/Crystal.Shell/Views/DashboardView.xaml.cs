using Crystal.Controls.Loading;
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
  /// <summary>
  /// Default row heights, kept in one place so ResetLayout and the XAML stay in sync.
  /// </summary>
  public GridLength CpuDefault {  get; } = new(1, GridUnitType.Star);

  /// <summary>
  /// Default row heights, kept in one place so ResetLayout and the XAML stay in sync.
  /// </summary>
  public GridLength GpuDefault {  get; } = new(1, GridUnitType.Star);

  /// <summary>
  /// Default row heights, kept in one place so ResetLayout and the XAML stay in sync.
  /// </summary>
  public GridLength ComponentsDefault {  get; } = new(1.2, GridUnitType.Star);

  /// <summary>
  /// Default row heights, kept in one place so ResetLayout and the XAML stay in sync.
  /// </summary>
  public GridLength ProcessesDefault {  get; } = new(1.6, GridUnitType.Star);

  /// <summary>
  /// Default width for each Row #3 tile column (Memory/Storage/Network/BIOS): equal star shares.
  /// </summary>
  public GridLength ComponentColumnDefault { get; } = new(1, GridUnitType.Star);

  /// <summary>
  /// Default widths for the bottom row's Processes (2*) and OS (1*) tile columns.
  /// </summary>
  public GridLength ProcessesColumnDefault { get; } = new(1, GridUnitType.Star);

  /// <summary>
  /// Default widths for the bottom row's Processes (2*) and OS (1*) tile columns.
  /// </summary>
  public GridLength OsColumnDefault { get; } = new(0.334, GridUnitType.Star);

  /// <summary>
  /// Initializes a new instance of the <see cref="DashboardView"/> class.
  /// </summary>
  public DashboardView() {
    InitializeComponent();
    // Each tile is an async-warming LoadingHost that swaps its spinner for real content on a
    // background thread; the star-sized rows/columns only settle to their true sizes once that
    // content lands. Re-apply the default layout each time a tile settles (the event bubbles up
    // from any LoadingHost) so the dashboard lands at its default proportions with no manual reset.
    // ResetLayout is idempotent, so running it per-tile simply converges as the last tile arrives.
    AddHandler(LoadingHost.SettledEvent, new RoutedEventHandler((_, _) => ResetLayout()));
  }

  /// <summary>
  /// Restores the resizable rows and the Row #3 tile columns to their default star proportions, 
  /// undoing any splitter drags. The MinHeight/MinWidth floors defined in XAML are unaffected.
  /// </summary>
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
