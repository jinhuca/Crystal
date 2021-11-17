using System.Windows.Automation.Peers;
using Crystal.Themes.Automation.Peers;

namespace Crystal.Themes.Controls
{
  public class CrystalHeader : GroupBox
  {
    static CrystalHeader()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(CrystalHeader), new FrameworkPropertyMetadata(typeof(CrystalHeader)));
    }

    /// <summary>
    /// Creates AutomationPeer (<see cref="M:System.Windows.UIElement.OnCreateAutomationPeer" />)
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return new CrystalHeaderAutomationPeer(this);
    }
  }
}