using System.Windows.Controls;

namespace Crystal.BiosModule.Views.SummaryViews;

/// <summary>Live board telemetry (fan, board temp, CMOS battery, voltage rails) plus the static
/// firmware backfill shown when no sensors are present. Binds to the root IBiosViewModel inherited
/// from the host tile.</summary>
public partial class BiosBoardReadingsView : UserControl {
  public BiosBoardReadingsView() => InitializeComponent();
}
