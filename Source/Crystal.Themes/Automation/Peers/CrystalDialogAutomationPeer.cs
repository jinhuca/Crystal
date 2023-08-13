namespace Crystal.Themes.Automation.Peers;

public class CrystalDialogAutomationPeer : FrameworkElementAutomationPeer
{
  public CrystalDialogAutomationPeer(CrystalDialogBase owner)
    : base(owner)
  {
  }

  protected override string GetClassNameCore()
  {
    return Owner.GetType().Name;
  }

  protected override AutomationControlType GetAutomationControlTypeCore()
  {
    return AutomationControlType.Custom;
  }

  protected override string GetNameCore()
  {
    var nameCore = base.GetNameCore();
    if (string.IsNullOrEmpty(nameCore))
    {
      nameCore = ((CrystalDialogBase)Owner).Title;
    }

    if (string.IsNullOrEmpty(nameCore))
    {
      nameCore = ((CrystalDialogBase)Owner).Name;
    }

    if (string.IsNullOrEmpty(nameCore))
    {
      nameCore = GetClassNameCore();
    }

    return nameCore!;
  }
}