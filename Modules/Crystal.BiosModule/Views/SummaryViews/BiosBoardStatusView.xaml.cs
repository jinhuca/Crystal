using System.Windows.Controls;

namespace Crystal.BiosModule.Views.SummaryViews;

/// <summary>Board health header for the BIOS summary tile: the live-state dot/label, severity and
/// session-peak badges, and the driver/availability note. Inherits the root IBiosViewModel from the
/// host tile.</summary>
public partial class BiosBoardStatusView : UserControl {
  public BiosBoardStatusView() => InitializeComponent();
}
