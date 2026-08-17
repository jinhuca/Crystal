using System.Windows.Controls;

namespace Crystal.BiosModule.Views.SummaryViews;

/// <summary>Header row for the BIOS summary tile: the accent title, manufacturer and version.
/// Inherits the root IBiosViewModel from the host tile.</summary>
public partial class BiosHeaderView : UserControl {
  public BiosHeaderView() => InitializeComponent();
}
