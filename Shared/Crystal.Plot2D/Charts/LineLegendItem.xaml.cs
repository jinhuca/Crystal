using Crystal.Plot2D.Descriptions;

namespace Crystal.Plot2D.Charts;

internal sealed partial class LineLegendItem {
  public LineLegendItem() {
    InitializeComponent();
  }

  public LineLegendItem(Description description) : base(description: description) {
    InitializeComponent();
  }
}
