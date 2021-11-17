using System.Windows.Automation.Peers;

namespace Crystal.Themes.Automation.Peers
{
  public class TabControlExAutomationPeer : TabControlAutomationPeer
  {
    public TabControlExAutomationPeer(TabControl owner)
        : base(owner)
    {
    }

    protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
    {
      return new TabItemExAutomationPeer(item, this);
    }
  }
}