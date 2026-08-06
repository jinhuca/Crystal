using System.Windows.Controls;

namespace BiosModule.Views;

/// <summary>Full-scale BIOS view: complete firmware identity. Reached by selecting the BIOS
/// summary strip; the Back control returns to the dashboard.</summary>
public partial class BiosDetailView : UserControl {
  public BiosDetailView() {
    InitializeComponent();
  }
}
