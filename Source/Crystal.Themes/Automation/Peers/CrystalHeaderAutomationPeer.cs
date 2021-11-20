using System.Windows.Automation.Peers;

namespace Crystal.Themes.Automation.Peers
{
  /// <summary>
  /// The CrystalHeaderAutomationPeer class exposes the <see cref="T:Crystal.Themes.Controls.CrystalHeader"/> type to UI Automation.
  /// </summary>
  public class CrystalHeaderAutomationPeer : GroupBoxAutomationPeer
  {
    public CrystalHeaderAutomationPeer(GroupBox owner)
        : base(owner)
    {
    }

    protected override string GetClassNameCore()
    {
      return "CrystalHeader";
    }
  }
}