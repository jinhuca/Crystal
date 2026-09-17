using Crystal.Controls.Loading;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Shell.Views;

/// <summary>
/// The dashboard: a grid of module summary tiles laid out per the reference design
/// (CPU and GPU full-width rows, Memory/Storage/Network on the components row, and
/// BIOS/Operating System/Processes on the bottom row). Each tile is its own region so a
/// module can inject its summary view.
/// The content rows are user-resizable via GridSplitters; <see cref="ResetLayout"/>
/// restores their default star proportions.
/// </summary>
public partial class DashboardView : UserControl {
  /// <summary>
  /// Default row heights, kept in one place so ResetLayout and the XAML stay in sync.
  /// </summary>
  public GridLength CpuGpuSummaryViewDefaultHeight {  get; } = new(1, GridUnitType.Star);

  /// <summary>
  /// Default row heights, kept in one place so ResetLayout and the XAML stay in sync.
  /// </summary>
  public GridLength GpuSummaryViewDefaultHeight {  get; } = new(1, GridUnitType.Star);

  /// <summary>
  /// Default row heights, kept in one place so ResetLayout and the XAML stay in sync.
  /// </summary>
  public GridLength SummaryViewDefaultHeight {  get; } = new(1.2, GridUnitType.Star);

  /// <summary>
  /// Default height of the bottom row (BIOS/Network/OS).
  /// </summary>
  public GridLength BiosSummaryViewDefaultHeight {  get; } = new(0.4, GridUnitType.Star);


  /// <summary>
  /// Default width for the equal components-row tile columns (Memory/Storage): full star shares.
  /// </summary>
  public GridLength ComponentColumnDefault { get; } = new(1, GridUnitType.Star);

  /// <summary>
  /// Default width for the equal bottom-row tile columns (BIOS/Operating System): full star shares.
  /// </summary>
  public GridLength BottomColumnDefault { get; } = new(1, GridUnitType.Star);


  public GridLength MemorySummaryViewDefaultColumnWidth { get; } = new(.4, GridUnitType.Star);

  public GridLength StorageSummaryViewDefaultColumnWidth { get; } = new(.4, GridUnitType.Star);

  public GridLength NetworkSummaryViewDefaultColumnWidth { get; } = new(0.2, GridUnitType.Star);

  /// <summary>
  /// Default width for the narrower BIOS column on the bottom row (0.3 star vs. the
  /// full-star BIOS/Operating System pair).
  /// </summary>
  public GridLength BiosSummaryViewDefaultColumnWidth { get; } = new(.4, GridUnitType.Star);

  /// <summary>
  /// Default width for the equal bottom-row tile columns (BIOS/Operating System): full star shares.
  /// </summary>
  public GridLength OsSummaryViewColumnDefaultWidth { get; } = new(.4, GridUnitType.Star);

  /// <summary>
  /// Default width for the narrower Processes column on the bottom row (0.3 star vs. the
  /// full-star BIOS/Operating System pair).
  /// </summary>
  public GridLength ProcessSummaryViewColumnDefaultWidth { get; } = new(0.2, GridUnitType.Star);


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
  /// Restores the resizable rows and the components-/bottom-row tile columns to their default star
  /// proportions, undoing any splitter drags. The MinHeight/MinWidth floors defined in XAML are
  /// unaffected.
  /// </summary>
  public void ResetLayout() {
    CpuRow.Height = CpuGpuSummaryViewDefaultHeight;
    GpuRow.Height = GpuSummaryViewDefaultHeight;
    ComponentsRow.Height = SummaryViewDefaultHeight;
    BottomRow.Height = BiosSummaryViewDefaultHeight;

    MemoryCol.Width = MemorySummaryViewDefaultColumnWidth;
    StorageCol.Width = StorageSummaryViewDefaultColumnWidth;
    NetworkCol.Width = NetworkSummaryViewDefaultColumnWidth;

    BiosCol.Width = BiosSummaryViewDefaultColumnWidth;
    OsCol.Width = OsSummaryViewColumnDefaultWidth;
    ProcessCol.Width = ProcessSummaryViewColumnDefaultWidth;
  }
}
