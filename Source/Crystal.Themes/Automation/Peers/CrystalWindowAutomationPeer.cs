using System.Windows.Automation.Peers;

namespace Crystal.Themes.Automation.Peers
{
  public class CrystalWindowAutomationPeer : WindowAutomationPeer
  {
    public CrystalWindowAutomationPeer([NotNull] Window owner)
        : base(owner)
    {
    }

    protected override string GetClassNameCore()
    {
      return "MetroWindow";
    }
  }
}