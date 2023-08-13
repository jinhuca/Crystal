namespace Crystal.Themes.Automation.Peers;

/// <summary>
/// The CrystalThumbContentControlAutomationPeer class exposes the <see cref="T:Crystal.Themes.Controls.CrystalThumbContentControl"/> type to UI Automation.
/// </summary>
public class CrystalThumbContentControlAutomationPeer : FrameworkElementAutomationPeer
{
  public CrystalThumbContentControlAutomationPeer(FrameworkElement owner)
    : base(owner)
  {
  }

  protected override AutomationControlType GetAutomationControlTypeCore()
  {
    return AutomationControlType.Custom;
  }

  protected override string GetClassNameCore()
  {
    return "CrystalThumbContentControl";
  }
}