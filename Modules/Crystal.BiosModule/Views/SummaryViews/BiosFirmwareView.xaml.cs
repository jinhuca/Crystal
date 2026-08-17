using System.Windows.Controls;

namespace Crystal.BiosModule.Views.SummaryViews;

/// <summary>Firmware interface and Secure Boot posture for the BIOS summary tile. Inherits the root
/// IBiosViewModel from the host tile.</summary>
public partial class BiosFirmwareView : UserControl {
  public BiosFirmwareView() => InitializeComponent();
}
