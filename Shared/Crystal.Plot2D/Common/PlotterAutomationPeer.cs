using System.Windows.Automation.Peers;

namespace Crystal.Plot2D.Common;

public sealed class PlotterAutomationPeer : FrameworkElementAutomationPeer {
  public PlotterAutomationPeer(PlotterBase owner)
    : base(owner: owner) {
  }

  protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;

  protected override string GetClassNameCore() => "Plotter";
}
